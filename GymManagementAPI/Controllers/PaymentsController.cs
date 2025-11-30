using GymManagementAPI.Data;
using GymManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace GymManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly CashfreeService _cashfree;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;

        public PaymentsController(AppDbContext db, CashfreeService cashfree, IConfiguration config, IHttpClientFactory httpFactory)
        {
            _db = db;
            _cashfree = cashfree;
            _config = config;
            _httpFactory = httpFactory;
        }

        public class CreateOrderRequest { 
            public int UserId { get; set; } 
            public int PlanId { get; set; } 
            public decimal Amount { get; set; } 
        }
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest req)
        {
            // 1) load user & plan (optional)
            if (req == null) return BadRequest("Invalid payload");
            var user = await _db.Users.FindAsync(req.UserId);
            if (user == null) return BadRequest("User not found");

            // 2) create Payment (PENDING)
            var payment = new Payment
            {
                UserId = req.UserId,
                PlanId = req.PlanId,
                Amount = req.Amount,
                PaymentStatus = "PENDING",
                PaymentGateway = "Cashfree",
                Currency = "INR",
                CreatedAt = DateTime.UtcNow
            };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(); // to get payment.Id

            // 3) call Cashfree
            var customerId = $"user-{req.UserId}";
            var customerPhone = user.PhoneNumber ?? "";
            var callback = _config.GetValue<string>("Payment:CallbackUrl");
            var webHook = _config.GetValue<string>("Payment:WebhookUrl");
            var cfResp = await _cashfree.CreateOrderAsync(req.Amount, "INR", customerId, customerPhone, callback, webHook);

            var root = cfResp.RootElement;
            string? orderId = null;
            string? sessionId = null;
            string? cfStatus = null;
            string? orderStatus = null;
            string? checkoutLink = null; // hosted checkout link if present
            string responseJson = root.ToString();

            // common properties from different CF responses
            if (root.TryGetProperty("status", out var statusEl))
                cfStatus = statusEl.GetString();

            if (root.TryGetProperty("order_id", out var orderIdEl))
                orderId = orderIdEl.GetString();
            else if (root.TryGetProperty("cf_order_id", out var cfOrderEl))
                orderId = cfOrderEl.GetString();

            if (root.TryGetProperty("payment_session_id", out var sessEl))
                sessionId = sessEl.GetString();

            // check for hosted checkout / payment link fields
            if (root.TryGetProperty("payment_link", out var plEl))
                checkoutLink = plEl.GetString();
            else if (root.TryGetProperty("payment_url", out var puEl))
                checkoutLink = puEl.GetString();
            else if (root.TryGetProperty("checkout_url", out var cuEl))
                checkoutLink = cuEl.GetString();

            if (root.TryGetProperty("order_status", out var osEl))
                orderStatus = osEl.GetString();
            else if (root.TryGetProperty("order_state", out var os2El))
                orderStatus = os2El.GetString();

            // success logic: either status OK or we have both orderId + sessionId (session is required by SDK)
            bool success =
            (!string.IsNullOrEmpty(cfStatus) && cfStatus.Equals("OK", StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrEmpty(orderId) && !string.IsNullOrEmpty(sessionId))
            || !string.IsNullOrEmpty(checkoutLink);

            if (!success)
            {
                // Save debug info and return 502
                _db.PaymentGatewayOrders.Add(new PaymentGatewayOrder
                {
                    PaymentId = payment.Id,
                    OrderId = orderId ?? string.Empty,
                    Amount = req.Amount,
                    Currency = "INR",
                    RequestJson = "", // if you stored request
                    ResponseJson = responseJson,
                    Status = "FAILED",
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                return StatusCode(502, new { message = "Cashfree order failed", raw = responseJson });
            }

            // 6) Save PaymentGatewayOrder
            var gatewayOrder = new PaymentGatewayOrder
            {
                PaymentId = payment.Id,
                OrderId = orderId ?? sessionId ?? string.Empty,
                PaymentSessionId = sessionId ?? string.Empty,
                Amount = req.Amount,
                Currency = "INR",
                RequestJson = "", // optional
                ResponseJson = responseJson,
                Status = orderStatus ?? "CREATED",
                CreatedAt = DateTime.UtcNow
            };
            _db.PaymentGatewayOrders.Add(gatewayOrder);
            await _db.SaveChangesAsync();

            // link back to Payment
            payment.OrderId = gatewayOrder.OrderId; // prefer gatewayOrder.OrderId
            payment.UpdatedAt = DateTime.UtcNow;
            _db.Payments.Update(payment);
            await _db.SaveChangesAsync();

            // 8) return the proper fields
            return Ok(new
            {
                // IMPORTANT: return the Cashfree session id (string) as payment_session_id
                payment_session_id = sessionId,          // <-- the actual session string Cashfree returns
                order_id = orderId,
                paymentId = payment.Id,                 // local DB id (useful)
                amount = req.Amount,
                currency = "INR",
                checkoutLink = checkoutLink,            // optional hosted checkout url
                raw = responseJson                       // optional debug field (remove in prod)
            });
        }


        [HttpGet("verify-order/{orderId}")]
        public async Task<IActionResult> VerifyOrder(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) return BadRequest("orderId required");

            var gatewayOrder = await _db.PaymentGatewayOrders
                .Include(g => g.Payment)
                .FirstOrDefaultAsync(g => g.OrderId == orderId);
            // try to get linked details (payment -> plan)
            var existingPayment = gatewayOrder.Payment;
            int? localPaymentId = existingPayment?.Id;
            int? linkedUserId = existingPayment?.UserId;
            int? linkedPlanId = existingPayment?.PlanId;
            string? role = _db.Users.Where(g => g.Id == linkedUserId).Select(g => g.Role).FirstOrDefault();
            int? linkedGymId = null;
            int? linkedTrainerId = null;

            if (linkedPlanId.HasValue)
            {
                var plan = await _db.Plans.AsNoTracking().FirstOrDefaultAsync(pl => pl.Id == linkedPlanId.Value);
                if (plan != null)
                {
                    linkedGymId = plan.GymId;
                    linkedTrainerId = plan.TrainerId;
                }
            }

            // If DB already has a final status, return it (fast path)
            var finalStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "SUCCESS", "PAID", "COMPLETED", "FAILED", "REFUNDED", "CANCELLED"
    };
            if (gatewayOrder != null && !string.IsNullOrWhiteSpace(gatewayOrder.Status) && finalStatuses.Contains(gatewayOrder.Status))
            {
                return Ok(new
                {
                    status = gatewayOrder.Status.ToUpperInvariant(),
                    orderId,
                    source = "db",
                    userId = linkedUserId,
                    planId = linkedPlanId,
                    gymId = linkedGymId,
                    trainerId = linkedTrainerId,
                    payment_session_id = gatewayOrder.PaymentSessionId ?? "", // ALWAYS RETURN SESSION ID
                    isMember = role == "Member" ? true : false,
                    isAdmin = role == "Admin" ? true : false,
                });
            }

            // Prepare HTTP client and headers
            var baseUrl = _config.GetValue<string>("Cashfree:BaseUrl")?.TrimEnd('/') ?? "https://sandbox.cashfree.com";
            var client = _httpFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);

            var appId = _config.GetValue<string>("Cashfree:AppId");
            var secret = _config.GetValue<string>("Cashfree:Secret");
            var apiVersion = _config.GetValue<string>("Cashfree:ApiVersion") ?? "2023-08-01";

            if (!string.IsNullOrWhiteSpace(appId))
            {
                client.DefaultRequestHeaders.Remove("x-client-id");
                client.DefaultRequestHeaders.Add("x-client-id", appId);
            }
            if (!string.IsNullOrWhiteSpace(secret))
            {
                client.DefaultRequestHeaders.Remove("x-client-secret");
                client.DefaultRequestHeaders.Add("x-client-secret", secret);
            }
            client.DefaultRequestHeaders.Remove("x-api-version");
            client.DefaultRequestHeaders.Add("x-api-version", apiVersion);

            // Endpoint used for PG APIs
            var path = $"/pg/orders/{Uri.EscapeDataString(orderId)}/payments";

            string body;
            try
            {
                var resp = await client.GetAsync(path);
                body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    // helpful for debugging - return raw body
                    return StatusCode(502, new { message = "Cashfree query failed", raw = body });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error calling Cashfree", error = ex.Message });
            }

            // Parse response robustly
            string canonical = "PENDING"; // default

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                // Helper to test an element for success
                static bool IsSuccessElement(JsonElement el)
                {
                    if (el.ValueKind != JsonValueKind.Object) return false;
                    if (el.TryGetProperty("payment_status", out var ps) &&
                        string.Equals(ps.GetString(), "SUCCESS", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (el.TryGetProperty("order_status", out var os) &&
                        (string.Equals(os.GetString(), "PAID", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(os.GetString(), "SUCCESS", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(os.GetString(), "COMPLETED", StringComparison.OrdinalIgnoreCase)))
                        return true;
                    return false;
                }

                // CASE 1: root is object and has 'data' array
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tx in dataEl.EnumerateArray())
                    {
                        if (IsSuccessElement(tx))
                        {
                            canonical = "SUCCESS";
                            break;
                        }
                    }

                    // optionally check top-level order_status if still pending
                    if (canonical == "PENDING" && root.TryGetProperty("order_status", out var osTop))
                        canonical = osTop.GetString() ?? canonical;
                }
                // CASE 2: root is an array of payments (your current sample)
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in root.EnumerateArray())
                    {
                        if (IsSuccessElement(item))
                        {
                            canonical = "SUCCESS";
                            break;
                        }
                    }
                }
                // CASE 3: root is object but no 'data' array; maybe has payment_status/order_status top-level
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("payment_status", out var psTop) &&
                        string.Equals(psTop.GetString(), "SUCCESS", StringComparison.OrdinalIgnoreCase))
                    {
                        canonical = "SUCCESS";
                    }
                    else if (root.TryGetProperty("order_status", out var osTop2))
                    {
                        var s = osTop2.GetString() ?? "";
                        if (string.Equals(s, "PAID", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s, "SUCCESS", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(s, "COMPLETED", StringComparison.OrdinalIgnoreCase))
                            canonical = "SUCCESS";
                        else if (string.Equals(s, "ACTIVE", StringComparison.OrdinalIgnoreCase) || string.Equals(s, "CREATED", StringComparison.OrdinalIgnoreCase))
                            canonical = "PENDING";
                        else
                            canonical = s.ToUpperInvariant();
                    }
                }
            }
            catch (Exception ex)
            {
                // parsing error: leave canonical as PENDING but return raw for debugging
                return StatusCode(500, new { message = "Failed to parse Cashfree response", error = ex.Message, raw = body });
            }

            // Normalize canonical to final enums we use
            canonical = canonical?.ToUpperInvariant() switch
            {
                "PAID" => "SUCCESS",
                "COMPLETED" => "SUCCESS",
                "SUCCESS" => "SUCCESS",
                "FAILED" => "FAILED",
                "CANCELLED" => "FAILED",
                "REFUNDED" => "REFUNDED",
                "ACTIVE" => "PENDING",
                "CREATED" => "PENDING",
                _ => canonical ?? "PENDING"
            };

            // Update DB if gatewayOrder exists
            if (gatewayOrder != null)
            {
                gatewayOrder.Status = canonical;
                gatewayOrder.ResponseJson = body;
                gatewayOrder.UpdatedAt = DateTime.UtcNow;
                _db.PaymentGatewayOrders.Update(gatewayOrder);

                if (gatewayOrder.Payment != null)
                {
                    if (canonical == "SUCCESS")
                        gatewayOrder.Payment.PaymentStatus = "SUCCESS";
                    gatewayOrder.Payment.UpdatedAt = DateTime.UtcNow;
                    _db.Payments.Update(gatewayOrder.Payment);
                }
                await _db.SaveChangesAsync();
            }

            return Ok(new { status = canonical, orderId, raw = body,
                userId = linkedUserId,
                planId = linkedPlanId,
                gymId = linkedGymId,
                trainerId = linkedTrainerId,
                payment_session_id = gatewayOrder?.PaymentSessionId ?? "",
                isMember = role == "Member" ? true : false,
                isAdmin = role == "Admin" ? true : false,
            });
        }


        // POST api/Payments/webhook
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            // 1) Read raw body (we need raw string for verification + logging)
            string body;
            using (var sr = new StreamReader(Request.Body))
            {
                body = await sr.ReadToEndAsync();
            }

            // 2) Optional signature header (do not store by default)
            var signatureHeader =
                Request.Headers["x-webhook-signature"].FirstOrDefault()
                ?? Request.Headers["x-cf-signature"].FirstOrDefault();

            // 3) If you configured a webhook secret in appsettings, verify HMAC SHA256
            var webhookSecret = _config.GetValue<string>("Cashfree:WebhookSecret");
            if (!string.IsNullOrWhiteSpace(webhookSecret) && !string.IsNullOrWhiteSpace(signatureHeader))
            {
                try
                {
                    using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(webhookSecret));
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
                    var base64 = Convert.ToBase64String(hash);
                    var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                    // Accept either base64 or hex representation from header
                    if (!(string.Equals(signatureHeader, base64, StringComparison.OrdinalIgnoreCase)
                       || string.Equals(signatureHeader, hex, StringComparison.OrdinalIgnoreCase)))
                    {
                        // signature mismatch -> log and reject
                        var sigLog = new PaymentWebhookLog
                        {
                            OrderId = "",
                            EventType = "signature_mismatch",
                            Payload = body,
                            ReceivedAt = DateTime.UtcNow,
                            Processed = true,
                            ProcessingResult = "SIGNATURE_MISMATCH"
                        };
                        _db.PaymentWebhookLogs.Add(sigLog);
                        await _db.SaveChangesAsync();
                        return Unauthorized();
                    }
                }
                catch
                {
                    // on verification exception, continue but log
                    var errorLog = new PaymentWebhookLog
                    {
                        OrderId = "",
                        EventType = "signature_check_error",
                        Payload = body,
                        ReceivedAt = DateTime.UtcNow,
                        Processed = true,
                        ProcessingResult = "SIGNATURE_CHECK_ERROR"
                    };
                    _db.PaymentWebhookLogs.Add(errorLog);
                    await _db.SaveChangesAsync();
                    return StatusCode(400);
                }
            }

            // 4) Parse JSON payload safely
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(body);
            }
            catch (Exception ex)
            {
                var parseLog = new PaymentWebhookLog
                {
                    OrderId = "",
                    EventType = "invalid_json",
                    Payload = body,
                    ReceivedAt = DateTime.UtcNow,
                    Processed = true,
                    ProcessingResult = "INVALID_JSON: " + ex.Message
                };
                _db.PaymentWebhookLogs.Add(parseLog);
                await _db.SaveChangesAsync();
                return BadRequest("invalid json");
            }

            using (doc)
            {
                var root = doc.RootElement;

                // 5) Get order id (flexible)
                var orderId = "";
                string rootJson = root.GetRawText();
                Console.WriteLine(rootJson);
                if (root.TryGetProperty("data", out var data) && 
                    data.TryGetProperty("order", out var order) && 
                    order.TryGetProperty("order_id", out var oid)) 
                { orderId = oid.GetString(); }

                var eventType = root.TryGetProperty("type", out var et)
                  ? et.GetString() ?? "webhook"
                  : "webhook";
                string webhookKey = ComputeWebhookKey(root, body, orderId);
                // 6) Save minimal webhook log (with PaymentId lookup attempt)
                var log = new PaymentWebhookLog
                {
                    OrderId = orderId ?? string.Empty,
                    EventType = eventType ?? "webhook",
                    Payload = body,
                    ReceivedAt = DateTime.UtcNow,
                    Processed = false,
                    ProcessingResult = string.Empty,
                     WebhookKey = webhookKey
                };

                _db.PaymentWebhookLogs.Add(log);
                try
                {
                    await _db.SaveChangesAsync(); // if duplicate, unique index will throw DbUpdateException
                }

                catch (DbUpdateException ex) when(IsUniqueConstraintViolation(ex))
        {
                    // Duplicate webhook detected (another request already inserted the same WebhookKey)
                    // Return 200 so the gateway doesn't retry.
                    return Ok();
                }


                // 7) Try to find linked gatewayOrder and Payment
                PaymentGatewayOrder? gatewayOrder = null;
                Payment? payment = null;
                if (!string.IsNullOrEmpty(orderId))
                {
                    gatewayOrder = await _db.PaymentGatewayOrders.Include(g => g.Payment)
                                        .FirstOrDefaultAsync(g => g.OrderId == orderId);
                    payment = gatewayOrder?.Payment;
                    if (payment == null)
                    {
                        payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                    }
                }

                // 8) Map status and tx id
                string? paymentStatus = null;

                if (root.TryGetProperty("data", out var pdata) &&
                    pdata.TryGetProperty("payment", out var paymentd) &&
                    paymentd.TryGetProperty("payment_status", out var ps))
                {
                    paymentStatus = ps.GetString();
                }

                string? txId = null;

                if (root.TryGetProperty("data", out var tdata) &&
                    tdata.TryGetProperty("payment", out var paymentt))
                {
                    // Check cf_payment_id first
                    if (paymentt.TryGetProperty("cf_payment_id", out var cfid))
                    {
                        txId = cfid.GetRawText().Trim('"');
                    }
                    // If cf_payment_id not present, check payment_id
                    else if (paymentt.TryGetProperty("payment_id", out var pid))
                    {
                        txId = pid.GetRawText().Trim('"');
                    }
                }


                // 9) Update relevant DB records inside a transaction
                using (var txDb = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        if (payment != null)
                        {
                            if (!string.IsNullOrEmpty(paymentStatus))
                            {
                                payment.PaymentStatus = paymentStatus switch
                                {
                                    var s when s.Equals("PAID", StringComparison.OrdinalIgnoreCase) => "SUCCESS",
                                    var s when s.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase) => "SUCCESS",
                                    var s when s.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) => "SUCCESS",
                                    var s when s.Equals("FAILED", StringComparison.OrdinalIgnoreCase) => "FAILED",
                                    var s when s.Equals("REFUNDED", StringComparison.OrdinalIgnoreCase) => "REFUNDED",
                                    _ => payment.PaymentStatus
                                };
                            }

                            if (!string.IsNullOrEmpty(txId)) payment.TransactionId = txId;
                            payment.UpdatedAt = DateTime.UtcNow;
                            _db.Payments.Update(payment);
                        }

                        if (gatewayOrder != null)
                        {
                            gatewayOrder.Status = paymentStatus ?? gatewayOrder.Status;
                            gatewayOrder.ResponseJson = body;
                            gatewayOrder.UpdatedAt = DateTime.UtcNow;
                            _db.PaymentGatewayOrders.Update(gatewayOrder);
                        }

                        // -----------------------
                        // NEW: Create Member if payment is successful and member doesn't exist
                        // -----------------------
                        var isPaymentSuccess = payment != null &&
                                               !string.IsNullOrEmpty(payment.PaymentStatus) &&
                                               payment.PaymentStatus.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase);

                        if (isPaymentSuccess && payment.UserId != null && payment.PlanId != null)
                        {
                            // Check if member already exists for this user+plan (change uniqueness rule if needed)
                            var existingMember = await _db.Members
                                .FirstOrDefaultAsync(m => m.UserId == payment.UserId && m.PlanId == payment.PlanId);

                            if (existingMember == null)
                            {
                                // Get user for details
                                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == payment.UserId);
                                if (user != null)
                                {
                                    // Get plan to find gym id
                                    var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == payment.PlanId);
                                    int gymId = plan.GymId; // adjust if GymId is non-nullable

                                    // compute age same as your CreateMember method
                                    int age = 0;
                                    if (user.DateOfBirth.HasValue)
                                    {
                                        var dob = user.DateOfBirth.Value;
                                        age = DateTime.Now.Year - dob.Year;
                                        if (dob > DateTime.Now.AddYears(-age)) age--;
                                    }

                                    var newMember = new Member
                                    {
                                        MemberName = user.FullName,
                                        Email = user.Email,
                                        GymId = gymId,          // if Member.GymId is non-nullable int, ensure plan != null or set default
                                        TrainerId = plan.TrainerId,       // set if you can determine trainer
                                        UserId = payment.UserId.Value,
                                        PlanId = payment.PlanId.Value,
                                        Age = age
                                    };

                                    _db.Members.Add(newMember);
                                    // do not call SaveChanges here; outer SaveChanges persists this insert as part of transaction
                                }
                                else
                                {
                                    // Optional: log or annotate that user was not found so member creation skipped
                                    // e.g. log.ProcessingResult += " | MEMBER_SKIPPED_USER_NOT_FOUND";
                                }
                            }
                            else
                            {
                                // optional: update existing member fields if you want to sync name/email/plan etc.
                            }
                        }

                        // Update webhook log with PaymentId (if found) and mark processed
                        log.PaymentId = payment?.Id;
                        log.Processed = true;
                        log.ProcessingResult = "OK";
                        _db.PaymentWebhookLogs.Update(log);

                        await _db.SaveChangesAsync();
                        await txDb.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await txDb.RollbackAsync();
                        // update log with failure reason
                        log.Processed = false;
                        log.ProcessingResult = "PROCESSING_ERROR: " + ex.Message;
                        _db.PaymentWebhookLogs.Update(log);
                        await _db.SaveChangesAsync();

                        // Return 200 so gateway doesn't rapidly retry; you may return 500 to force retry if desired
                        return StatusCode(200);
                    }
                }

                return Ok();
            }
        }

     

        // GET api/Payments/status/{orderId}
        [HttpGet("status/{orderId}")]
        public async Task<IActionResult> GetPaymentStatus(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) return BadRequest("orderId required");

            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (payment == null) return NotFound();

            return Ok(new
            {
                payment.Id,
                payment.OrderId,
                payment.PaymentStatus,
                payment.TransactionId,
                payment.Amount,
                payment.UpdatedAt
            });
        }

        private string ComputeWebhookKey(JsonElement root, string body, string orderIdCandidate)
        {
            // prefer stable fields in this order:
            // 1) data.order.order_id
            // 2) data.payment.cf_payment_id or data.payment.gateway_payment_id
            // 3) payment_gateway_details.gateway_payment_id or gateway_order_id
            // 4) fallback: hash of full payload

            try
            {
                if (!string.IsNullOrEmpty(orderIdCandidate))
                    return "order:" + orderIdCandidate;

                if (root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("payment", out var payment))
                {
                    if (payment.TryGetProperty("cf_payment_id", out var cf) && cf.ValueKind != JsonValueKind.Null)
                    {
                        return "cfpay:" + cf.GetRawText().Trim('"');
                    }
                    if (payment.TryGetProperty("gateway_payment_id", out var gp) && gp.ValueKind != JsonValueKind.Null)
                    {
                        return "gp:" + gp.GetRawText().Trim('"');
                    }
                }

                if (root.TryGetProperty("data", out var d2) &&
                    d2.TryGetProperty("payment_gateway_details", out var pgd))
                {
                    if (pgd.TryGetProperty("gateway_payment_id", out var gpid) && gpid.ValueKind != JsonValueKind.Null)
                        return "gpd:" + gpid.GetRawText().Trim('"');
                    if (pgd.TryGetProperty("gateway_order_id", out var goid) && goid.ValueKind != JsonValueKind.Null)
                        return "gpo:" + goid.GetRawText().Trim('"');
                }
            }
            catch
            {
                // ignore and fallback to payload hash
            }

            // fallback: SHA256 hash of body
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(body));
            return "payload:" + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            // SQL Server: 2601 or 2627 indicate unique constraint violation
            if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                return sqlEx.Number == 2627 || sqlEx.Number == 2601;
            }

            // Postgres / others: inspect message (optionally extend for other providers)
            if (ex.InnerException != null && ex.InnerException.Message?.ToLowerInvariant().Contains("unique") == true)
                return true;

            return false;
        }



    }
}
