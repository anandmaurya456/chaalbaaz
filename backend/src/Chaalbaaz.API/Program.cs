using Chaalbaaz.API.Hubs;
using Chaalbaaz.API.Middleware;
using Chaalbaaz.Application.Services;
using Chaalbaaz.Core.Interfaces;
using Chaalbaaz.Infrastructure.Repositories;
using Chaalbaaz.Infrastructure.Stockfish;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ---- Serilog ----
builder.Host.UseSerilog((ctx, config) =>
    config.ReadFrom.Configuration(ctx.Configuration)
          .WriteTo.Console()
          .WriteTo.ApplicationInsights(
              ctx.Configuration["ApplicationInsights:ConnectionString"],
              TelemetryConverter.Traces));

// ---- Controllers + Swagger ----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Chaalbaaz API", Version = "v1" });
});

// ---- SignalR ----
builder.Services.AddSignalR();

// ---- Redis ----
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

// ---- CORS (allow Chrome extension) ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("ChaalbaazPolicy", policy =>
        policy.WithOrigins(
                "chrome-extension://*",
                "https://www.chess.com",
                "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// ---- HTTP Client → Python Engine ----
builder.Services.AddHttpClient<IStockfishEngineClient, StockfishEngineClient>(client =>
{
    var engineUrl = builder.Configuration["StockfishEngine:BaseUrl"] ?? "http://localhost:8001";
    client.BaseAddress = new Uri(engineUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ---- Application Services ----
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<IGameSessionRepository, RedisGameSessionRepository>();

// ---- Health Checks ----
builder.Services.AddHealthChecks()
    .AddRedis(redisConnectionString, name: "redis");

var app = builder.Build();

// ---- Middleware Pipeline ----
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ChaalbaazPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();

// ---- Routes ----
app.MapControllers();
app.MapHub<ChessHub>("/hubs/chess");
app.MapHealthChecks("/health");

app.Logger.LogInformation("🚀 Chaalbaaz API started on {Env}", app.Environment.EnvironmentName);

app.Run();
