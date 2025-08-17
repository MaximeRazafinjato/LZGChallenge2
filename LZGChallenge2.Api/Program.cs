using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Options;
using LZGChallenge2.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentEmail.Core.Defaults;
using FluentEmail.Smtp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure MongoDB
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"]!;
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"]!;
builder.Services.AddMongoDb(mongoConnectionString, mongoDatabaseName);

// Configure options
builder.Services.Configure<RiotApiOptions>(
    builder.Configuration.GetSection(RiotApiOptions.SectionName));
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

// Configure SignalR
builder.Services.AddSignalR();

// Configure JWT Authentication
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromMinutes(5) // Tolérance pour la synchronisation d'horloge
        };

        // Configuration pour SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/leaderboardHub"))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:5175") // Vite ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure memory cache
builder.Services.AddMemoryCache();

// Configure HTTP client for Riot API
builder.Services.AddHttpClient<IRiotApiService, RiotApiService>();

// Configure FluentEmail
var emailOptions = builder.Configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>()!;
builder.Services
    .AddFluentEmail(emailOptions.FromEmail, emailOptions.FromName)
    .AddSmtpSender(emailOptions.SmtpHost, emailOptions.SmtpPort, emailOptions.Username, emailOptions.Password);

// Register services
builder.Services.AddSingleton<RateLimitService>();
builder.Services.AddScoped<IRiotApiService, RiotApiService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IMatchUpdateService, MatchUpdateService>();

// Register authentication services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<LZGChallenge2.Api.Hubs.LeaderboardHub>("/leaderboardHub");

app.Run();
