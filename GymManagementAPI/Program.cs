using Google.Apis.Auth.OAuth2;
using GymManagementAPI.Data;
using GymManagementAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<JwtService>();
builder.Services.AddSingleton<FirebaseStorageService>();

// LOAD FIREBASE JSON FROM SECRET (NOT FROM FILE PATH)
// ----------------------------------------------------
var firebaseJson = builder.Configuration["Firebase:CredentialsJson"];
if (!string.IsNullOrEmpty(firebaseJson))
{
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(firebaseJson));
    var googleCredential = GoogleCredential.FromStream(stream);
    builder.Services.AddSingleton(sp => googleCredential);
}
else
{
    // Local fallback (development only)
    var credPath = builder.Configuration["Firebase:CredentialsPath"];
    if (!string.IsNullOrEmpty(credPath) && File.Exists(credPath))
    {
        using var stream = File.OpenRead(credPath);
        var googleCredential = GoogleCredential.FromStream(stream);
        builder.Services.AddSingleton(sp => googleCredential);
    }
}


// CORS setup
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:4200", "http://localhost:4200",
            "https://gympro-management.netlify.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
        .AllowCredentials();
    });
});



// Configure SQL Server Connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
/////////////////////////////////////////////////
// Ensure these settings exist in appsettings.json or env vars:
// "Jwt:Key", "Jwt:Issuer", "Jwt:Audience"
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    // Optional: fail fast in dev if jwt config missing
    // throw new InvalidOperationException("JWT configuration missing in appsettings/environment.");
}
// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
options.RequireHttpsMetadata = true;
options.SaveToken = false;

options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateIssuerSigningKey = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromSeconds(30),
    ValidIssuer = jwtIssuer,
    ValidAudience = jwtAudience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
};

    // ?? IMPORTANT: Read AccessToken from HttpOnly cookie
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("AccessToken"))
            {
                context.Token = context.Request.Cookies["AccessToken"];
            }
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddAuthorization();
builder.Services.AddHttpClient<CashfreeService>();

var app = builder.Build();

// Middleware order: CORS -> HTTPS -> Auth -> AuthZ -> Endpoints
/////////////////////////////////////////////////
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseSwagger();
//app.UseSwaggerUI();
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
