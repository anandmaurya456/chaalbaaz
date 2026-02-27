using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Chaalbaaz.API.Hubs;

/// <summary>
/// SignalR hub — Chrome extension connects here to receive real-time move suggestions.
/// </summary>
public class ChessHub : Hub
{
    private readonly ILogger<ChessHub> _logger;

    public ChessHub(ILogger<ChessHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extension calls this to join a session group and start receiving suggestions.
    /// </summary>
    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        _logger.LogInformation("Client {ConnectionId} joined session {SessionId}",
            Context.ConnectionId, sessionId);

        await Clients.Caller.SendAsync("JoinedSession", new
        {
            sessionId,
            message = "Connected to Chaalbaaz — suggestions incoming ♟️"
        });
    }

    /// <summary>
    /// Extension sends new FEN when board changes — triggers analysis pipeline.
    /// </summary>
    public async Task SendFenUpdate(string sessionId, string fen)
    {
        _logger.LogInformation("FEN update from {ConnectionId} for session {SessionId}: {Fen}",
            Context.ConnectionId, sessionId, fen);

        // Notify all group members that a new position is being analysed
        await Clients.Group(sessionId).SendAsync("AnalysisStarted", new { fen });
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
