using TLearn.Application;
using TLearn.Infrastructure;
using TLearn.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StackExchange.Redis;
using TLearn.API.Extensions;
using TLearn.API.Hubs;
using TLearn.Application.Features.FlashCards.Commons;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Hubs;
using TLearn.Infrastructure.Services;
using TLearn.Infrastructure.Services.Payments;
using TLearn.Infrastructure.Services.PayOs;
using TLearn.Infrastructure.Services.Subscriptions;

var builder = WebApplication.CreateBuilder(args);

// === Add Controllers ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// === Add Services ===
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

// === Add Authentication ===
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // SignalR JWT handling
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // For WebSocket connections
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/collaborationHub"))
                {
                    context.Token = accessToken;
                    return Task.CompletedTask;
                }

                // For HTTP requests - get from cookie
                var tokenFromCookie = context.Request.Cookies["accessToken"];
                if (!string.IsNullOrEmpty(tokenFromCookie))
                {
                    context.Token = tokenFromCookie;
                }

                return Task.CompletedTask;
            }
        };
    });


// === DI Services ===
builder.Services.AddHttpClient<OllamaAiService>(client => { client.Timeout = TimeSpan.FromSeconds(120); });


builder.Services.AddHttpContextAccessor();

builder.Services.Configure<PayOSOptions>(
    builder.Configuration.GetSection("PayOS"));

builder.Services.AddHttpClient<IPayOSService, PayOSService>(client => { client.Timeout = TimeSpan.FromSeconds(30); });

builder.Services.AddScoped<IAiService, OllamaAiService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IAiUsageLimiter, AiUsageLimiter>();


builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPaymentStatusService, PaymentStatusService>();
// === Redis ===
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnectionString = builder.Configuration["Redis:ConnectionString"];

    if (string.IsNullOrWhiteSpace(redisConnectionString))
    {
        throw new InvalidOperationException(
            "Redis connection string is missing. Please configure Redis:ConnectionString.");
    }

    var redisOptions = CreateRedisConfiguration(redisConnectionString);
    return ConnectionMultiplexer.Connect(redisOptions);
});

// === SignalR ===
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100KB
});

// === Authorization ===
builder.Services.AddAuthorization();

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Quan trọng cho WebSocket
    });
});

// === Build App ===
var app = builder.Build();

// === Configure Pipeline ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// WebSocket middleware - phải ở đầu pipeline
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});

// CORS
app.UseCors("AllowNextJs");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapHub<CollaborationHub>("/collaborationHub");
app.MapHub<TodoHub>("/hubs/todo");
app.MapSignalRHub();
// Run migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TLearnDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();

static ConfigurationOptions CreateRedisConfiguration(string redisConnectionString)
{
    ConfigurationOptions redisOptions;

    if (Uri.TryCreate(redisConnectionString, UriKind.Absolute, out var redisUri) &&
        (redisUri.Scheme == "redis" || redisUri.Scheme == "rediss"))
    {
        redisOptions = new ConfigurationOptions
        {
            AbortOnConnectFail = false,
            Ssl = redisUri.Scheme == "rediss",
            ConnectTimeout = 15000,
            SyncTimeout = 15000,
            AsyncTimeout = 15000,
            KeepAlive = 30
        };

        redisOptions.EndPoints.Add(redisUri.Host, redisUri.Port);

        if (!string.IsNullOrWhiteSpace(redisUri.UserInfo))
        {
            var userInfoParts = redisUri.UserInfo.Split(':', 2);
            if (userInfoParts.Length == 2)
            {
                redisOptions.User = Uri.UnescapeDataString(userInfoParts[0]);
                redisOptions.Password = Uri.UnescapeDataString(userInfoParts[1]);
            }
            else
            {
                redisOptions.Password = Uri.UnescapeDataString(userInfoParts[0]);
            }
        }
    }
    else
    {
        redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;
        redisOptions.ConnectTimeout = 15000;
        redisOptions.SyncTimeout = 15000;
        redisOptions.AsyncTimeout = 15000;
        redisOptions.KeepAlive = 30;
    }

    return redisOptions;
}