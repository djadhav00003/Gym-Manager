// Services/CashfreeService.cs
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

public class CashfreeService
{
    private readonly HttpClient _http;
    private readonly string _appId;
    private readonly string _secret;
    private readonly string _baseUrl;
    private readonly string _apiVersion; // common default, confirm in docs

    public CashfreeService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _appId = config["Cashfree:AppId"] ?? throw new InvalidOperationException("Cashfree:AppId missing");
        _secret = config["Cashfree:Secret"] ?? throw new InvalidOperationException("Cashfree:Secret missing");
        _baseUrl = config["Cashfree:BaseUrl"]?.TrimEnd('/') ?? "https://sandbox.cashfree.com";
        // Prefer explicit API version from config; fall back to a reasonable default
        _apiVersion = config["Cashfree:ApiVersion"] ?? "2023-08-01";
    }

    public async Task<JsonDocument> CreateOrderAsync(decimal amount, string currency, string customerId, string customerPhone, string callback = null, string webHook = null)
    {
        var req = new
        {
            order_currency = currency,
            order_amount = Math.Round(amount, 2),
            customer_details = new
            {
                customer_id = customerId,
                customer_phone = customerPhone
            },
            // optional fields:
            order_meta = new { return_url = callback,
                notify_url = webHook
            } // not required if you want callback handled separately
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/pg/orders")
        {
            Content = JsonContent.Create(req)
        };

        request.Headers.Add("x-api-version", _apiVersion);
        request.Headers.Add("x-client-id", _appId);
        request.Headers.Add("x-client-secret", _secret);

        var resp = await _http.SendAsync(request);
        var body = await resp.Content.ReadAsStringAsync();

        resp.EnsureSuccessStatusCode(); // throws if non-2xx

        return JsonDocument.Parse(body);
    }
}
