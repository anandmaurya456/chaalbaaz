using System.Text.Json;
using Chaalbaaz.Core.Interfaces;
using Chaalbaaz.Core.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Chaalbaaz.Infrastructure.Repositories;

public class RedisGameSessionRepository : IGameSessionRepository
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisGameSessionRepository> _logger;
    private readonly IDatabase _db;

    private const string SessionKeyPrefix = "chaalbaaz:session:";
    private const string UsernameKeyPrefix = "chaalbaaz:username:";
    private static readonly TimeSpan SessionTtl = TimeSpan.FromHours(6);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public RedisGameSessionRepository(
        IConnectionMultiplexer redis,
        ILogger<RedisGameSessionRepository> logger)
    {
        _redis = redis;
        _logger = logger;
        _db = redis.GetDatabase();
    }

    public async Task<GameSession?> GetByIdAsync(string sessionId, CancellationToken ct = default)
    {
        var key = $"{SessionKeyPrefix}{sessionId}";
        var data = await _db.StringGetAsync(key);
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<GameSession>(data!, JsonOptions);
    }

    public async Task<GameSession?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var usernameKey = $"{UsernameKeyPrefix}{username.ToLower()}";
        var sessionId = await _db.StringGetAsync(usernameKey);
        if (sessionId.IsNullOrEmpty) return null;
        return await GetByIdAsync(sessionId!, ct);
    }

    public async Task<GameSession> CreateAsync(GameSession session, CancellationToken ct = default)
    {
        var sessionKey = $"{SessionKeyPrefix}{session.Id}";
        var usernameKey = $"{UsernameKeyPrefix}{session.ChessComUsername.ToLower()}";

        var json = JsonSerializer.Serialize(session, JsonOptions);

        var tx = _db.CreateTransaction();
        _ = tx.StringSetAsync(sessionKey, json, SessionTtl);
        _ = tx.StringSetAsync(usernameKey, session.Id, SessionTtl);

        await tx.ExecuteAsync();

        _logger.LogInformation("Created session {SessionId} for {Username}", session.Id, session.ChessComUsername);
        return session;
    }

    public async Task<GameSession> UpdateAsync(GameSession session, CancellationToken ct = default)
    {
        session.UpdatedAt = DateTime.UtcNow;
        var key = $"{SessionKeyPrefix}{session.Id}";
        var json = JsonSerializer.Serialize(session, JsonOptions);
        await _db.StringSetAsync(key, json, SessionTtl);
        return session;
    }

    public async Task DeleteAsync(string sessionId, CancellationToken ct = default)
    {
        var session = await GetByIdAsync(sessionId, ct);
        if (session is null) return;

        var sessionKey = $"{SessionKeyPrefix}{sessionId}";
        var usernameKey = $"{UsernameKeyPrefix}{session.ChessComUsername.ToLower()}";

        await _db.KeyDeleteAsync(new RedisKey[] { sessionKey, usernameKey });
        _logger.LogInformation("Deleted session {SessionId}", sessionId);
    }
}
