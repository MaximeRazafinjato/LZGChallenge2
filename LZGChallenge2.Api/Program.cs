using Microsoft.EntityFrameworkCore;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Options;
using LZGChallenge2.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure options
builder.Services.Configure<RiotApiOptions>(
    builder.Configuration.GetSection(RiotApiOptions.SectionName));

// Configure SignalR
builder.Services.AddSignalR();

// Configure CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite default port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure memory cache
builder.Services.AddMemoryCache();

// Configure HTTP client for Riot API
builder.Services.AddHttpClient<IRiotApiService, RiotApiService>();

// Register services
builder.Services.AddSingleton<RateLimitService>();
builder.Services.AddScoped<IRiotApiService, RiotApiService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IMatchUpdateService, MatchUpdateService>();

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

app.UseAuthorization();

app.MapControllers();
app.MapHub<LZGChallenge2.Api.Hubs.LeaderboardHub>("/leaderboardHub");

app.Run();
