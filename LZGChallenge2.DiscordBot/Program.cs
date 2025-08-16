using LZGChallenge2.DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using LZGChallenge2.Api.Data;
using LZGChallenge2.Api.Services;
using LZGChallenge2.Api.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<RiotApiOptions>(builder.Configuration.GetSection("RiotApi"));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IRiotApiService, RiotApiService>();
builder.Services.AddSingleton<RateLimitService>();
builder.Services.AddScoped<MatchUpdateService>();

builder.Services.AddHostedService<DiscordBotService>();
builder.Services.AddHostedService<NotificationService>();

builder.Logging.AddConsole();

var host = builder.Build();

await host.RunAsync();
