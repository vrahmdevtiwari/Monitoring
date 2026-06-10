using System.IdentityModel.Tokens.Jwt;
using AspNetCoreRateLimit;
using Identity.Common.Implementation.Interface;
using Identity.Common.Implementation.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Monitoring.APIResponses;
using Monitoring.CommonFunction;
using Monitoring.Data;
using Monitoring.Store;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 102400; // 64KB
    options.Limits.MaxRequestBodySize = int.MaxValue;   // ✅ merged — duplicate ConfigureKestrel hata diya
});

builder.Services.AddDbContext<SSODBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SSODBConnection")));

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<EndagentAPI>();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bypass certificate validation for OIDC communication
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "mon";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.SessionStore = new MySessionStore();
    // ✅ Cookie lifetime set karo — session.js server-side check se 20 min idle par
    //    logout karega, cookie 60 min tak valid rahegi taaki pehle expire na ho
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["SSOServer:Login"];
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ClientId = "Web_Monitor_Client";
    options.ClientSecret = "secret";
    options.ResponseType = "code";
    options.Scope.Add("User");

    options.MapInboundClaims = false;
    options.DisableTelemetry = true;

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";
    options.RequireHttpsMetadata = false;
    options.ClaimActions.MapJsonKey("role", "role");
    options.ClaimActions.MapJsonKey("OrgId", "OrgId");
    options.SignedOutCallbackPath = "/signout-callback-oidc";
    options.SignedOutRedirectUri = "/Home/Index";

    options.Events = new OpenIdConnectEvents
    {
        // ✅ BUG FIX — Pehle `new AuthenticationProperties()` se existing properties
        //    override ho jaati thi (ExpiresUtc, IssuedUtc sab lost).
        //    Ab context.Properties ke saath merge karte hain — pehle se jo properties
        //    hain wo safe rahti hain.
        OnTokenValidated = async context =>
        {
            var accessToken = context.TokenEndpointResponse?.AccessToken;
            var idToken = context.TokenEndpointResponse?.IdToken;
            var refreshToken = context.TokenEndpointResponse?.RefreshToken;

            var tokens = new List<AuthenticationToken>();

            if (!string.IsNullOrEmpty(accessToken))
                tokens.Add(new AuthenticationToken { Name = "access_token", Value = accessToken });
            if (!string.IsNullOrEmpty(idToken))
                tokens.Add(new AuthenticationToken { Name = "id_token", Value = idToken });
            if (!string.IsNullOrEmpty(refreshToken))
                tokens.Add(new AuthenticationToken { Name = "refresh_token", Value = refreshToken });

            // ✅ Existing context.Properties mein merge karo — override nahi
            if (tokens.Count > 0)
                context.Properties?.StoreTokens(tokens);

            var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Tokens stored. Expires at: {ExpiresAt}",
                context.Properties?.GetTokenValue("expires_at"));

            await Task.CompletedTask;
        },

        OnRedirectToIdentityProviderForSignOut = async context =>
        {
            context.ProtocolMessage.IssuerAddress =
                $"{builder.Configuration["SSOServer:Login"]}/connect/endsession";
            context.ProtocolMessage.IdTokenHint =
                context.HttpContext.GetTokenAsync("id_token").Result;
            context.ProtocolMessage.PostLogoutRedirectUri =
                builder.Configuration["SSOServer:Home"] + "/signout-callback-oidc";
            await Task.CompletedTask;
        },

        OnRemoteFailure = context =>
        {
            context.Response.Redirect("/");
            context.HandleResponse();
            return Task.FromResult(0);
        }
    };

    options.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
});

builder.Services.AddAuthorization();

builder.Services.AddTransient<ITokenServices, TokenServices>();
builder.Services.AddTransient<ITokenExtraction, TokenExtraction>();

// ---- Rate Limiting ----
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// ========================================================================
var app = builder.Build();
// ========================================================================

// ---- CORS ----
var allowedOrigins = new[]
{
    "https://fonts.googleapis.com",
    "https://unpkg.com",
    "https://fonts.gstatic.com"
};

// Security Headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    context.Response.Headers.Append("X-Xss-Protection", "1");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.Append("Permissions-Policy",
        "accelerometer=(), camera=(), geolocation=(), gyroscope=(), " +
        "magnetometer=(), microphone=(), payment=(), usb=()");
    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}
else
{
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseIpRateLimiting();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Logger Implementation
var loggerFactory = app.Services.GetService<ILoggerFactory>();
loggerFactory.AddFile($@"{Directory.GetCurrentDirectory()}\Logs\log.txt");

app.Run();




//using System.IdentityModel.Tokens.Jwt;
//using AspNetCoreRateLimit;
//using Identity.Common.Implementation.Interface;
//using Identity.Common.Implementation.Services;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.EntityFrameworkCore;
//using Monitoring.APIResponses;
//using Monitoring.CommonFunction;
//using Monitoring.Data;
//using Monitoring.Store;

//var builder = WebApplication.CreateBuilder(args);
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.Limits.MaxRequestHeadersTotalSize = 102400; // 64KB (double the original)
//});
//builder.Services.AddDbContext<SSODBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SSODBConnection")));
//builder.Services.AddHttpClient();
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddTransient<EndagentAPI>();
//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// Bypass certificate validation for OIDC communication
//JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
//})
//.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
//{
//    options.Cookie.Name = "mon";
//    options.Cookie.SameSite = SameSiteMode.None; //before None;
//    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//    options.SessionStore = new MySessionStore();
//})
//.AddOpenIdConnect(options =>
//{
//    options.Authority = builder.Configuration["SSOServer:Login"];
//    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//    options.ClientId = "Web_Monitor_Client";
//    options.ClientSecret = "secret";
//    options.ResponseType = "code";
//    options.Scope.Add("User");

//    options.MapInboundClaims = false;
//    options.DisableTelemetry = true;

//    options.SaveTokens = true;
//    options.GetClaimsFromUserInfoEndpoint = true;
//    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
//    options.TokenValidationParameters.NameClaimType = "name";
//    options.TokenValidationParameters.RoleClaimType = "role";
//    options.RequireHttpsMetadata = false;
//    options.ClaimActions.MapJsonKey("role", "role");
//    options.ClaimActions.MapJsonKey("OrgId", "OrgId");
//    options.SignedOutCallbackPath = "/signout-callback-oidc";
//    options.SignedOutRedirectUri = "/Home/Index";

//    options.Events = new OpenIdConnectEvents
//    {
//        OnTokenValidated = async context =>
//        {
//            // Manually store tokens in authentication properties
//            var accessToken = context.TokenEndpointResponse?.AccessToken;
//            var idToken = context.TokenEndpointResponse?.IdToken;
//            var refreshToken = context.TokenEndpointResponse?.RefreshToken;

//            var props = new AuthenticationProperties();
//            props.StoreTokens(new[]
//            {
//            new AuthenticationToken { Name = "access_token", Value = accessToken },
//            new AuthenticationToken { Name = "id_token", Value = idToken },
//            new AuthenticationToken { Name = "refresh_token", Value = refreshToken }
//            });

//            context.Properties = props;

//            var expiresAt = context.Properties.GetTokenValue("expires_at");
//            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
//            logger.LogInformation($"Token expires at: {expiresAt}");
//        },
//        OnRedirectToIdentityProviderForSignOut = async context =>
//        {
//            context.ProtocolMessage.IssuerAddress = $"{builder.Configuration["SSOServer:Login"]}/connect/endsession";
//            context.ProtocolMessage.IdTokenHint = context.HttpContext.GetTokenAsync("id_token").Result;
//            context.ProtocolMessage.PostLogoutRedirectUri = builder.Configuration["SSOServer:Home"] + "/signout-callback-oidc";
//            await Task.CompletedTask;
//        },
//        OnRemoteFailure = context =>
//        {
//            context.Response.Redirect("/");
//            context.HandleResponse();
//            return Task.FromResult(0);
//        }
//    };

//    options.BackchannelHttpHandler = new HttpClientHandler
//    {
//        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
//    };
//});
//builder.Services.AddAuthorization();

//builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = int.MaxValue);

//builder.Services.AddTransient<ITokenServices, TokenServices>();
//builder.Services.AddTransient<ITokenExtraction, TokenExtraction>();

////Rate Limiting
//builder.Services.AddMemoryCache();
//builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
//builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
//builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
//builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
//builder.Services.AddInMemoryRateLimiting();

//var app = builder.Build();

//// Set CORS Headers based on allowed origins
//var allowedOrigins = new[] {
//    "https://fonts.googleapis.com/*",
//    "https://unpkg.com/*",
//    "https://fonts.googleapis.com/*;"
//    };

//// Security Headers middleware
//app.Use(async (context, next) =>
//{
//    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
//    context.Response.Headers.Add("X-Xss-Protection", "1");
//    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
//    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
//    context.Response.Headers.Add("Referrer-Policy", "no-referrer");
//    context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

//    await next();
//});

////Configure the HTTP request pipeline.

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//    app.UseHsts();
//}
//else
//{
//    app.UseStatusCodePagesWithReExecute("/Error/{0}");
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseIpRateLimiting();
//app.UseRouting();

//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");
//// Logger Implementation
//var loggerFactory = app.Services.GetService<ILoggerFactory>();
//loggerFactory.AddFile($@"{Directory.GetCurrentDirectory()}\Logs\log.txt");
//app.Run();
