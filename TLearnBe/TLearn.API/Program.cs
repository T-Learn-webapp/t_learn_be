using TLearn.Application;
using TLearn.Infrastructure;
using TLearn.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StackExchange.Redis;
using TLearn.API.Extensions;
using TLearn.API.Hubs;
using TLearn.Infrastructure.Data.Configurations;
using TLearn.Infrastructure.Hubs;
using TLearn.Infrastructure.Services;

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
builder.Services.AddHttpContextAccessor(); 
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRedisService, RedisService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService,NotificationService>();
// === Redis ===
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration["Redis:ConnectionString"];
    return ConnectionMultiplexer.Connect(configuration!);
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