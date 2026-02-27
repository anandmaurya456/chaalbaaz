using Chaalbaaz.Core.DTOs;
using Chaalbaaz.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chaalbaaz.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AnalysisController : ControllerBase
{
    private readonly IAnalysisService _analysisService;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(IAnalysisService analysisService, ILogger<AnalysisController> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
    }

    /// <summary>
    /// Analyse a chess position from a FEN string.
    /// </summary>
    [HttpPost("position")]
    [ProducesResponseType(typeof(ApiResponse<AnalysisResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AnalysisResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AnalysePosition(
        [FromBody] AnalyseRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Fen))
            return BadRequest(ApiResponse<AnalysisResultDto>.Fail("FEN string is required"));

        var result = await _analysisService.AnalysePositionAsync(
            request.Fen, request.Depth, request.TopMoves, ct);

        var dto = new AnalysisResultDto(
            result.Fen,
            new MoveEvaluationDto(result.BestMove.Move, result.BestMove.MoveSan,
                result.BestMove.CentipawnScore, result.BestMove.MateIn,
                result.BestMove.Depth, result.BestMove.PrincipalVariation),
            result.TopMoves.Select(m => new MoveEvaluationDto(
                m.Move, m.MoveSan, m.CentipawnScore, m.MateIn, m.Depth, m.PrincipalVariation
            )).ToList(),
            result.Turn,
            result.IsCheck,
            result.IsCheckmate,
            result.IsStalemate,
            result.Cached,
            result.AnalysedAt
        );

        return Ok(ApiResponse<AnalysisResultDto>.Ok(dto));
    }

    /// <summary>
    /// Analyse position AND broadcast suggestion to session via SignalR.
    /// Called by the Chrome extension on every board change.
    /// </summary>
    [HttpPost("session/{sessionId}/analyse")]
    [ProducesResponseType(typeof(ApiResponse<AnalysisResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalyseAndBroadcast(
        string sessionId,
        [FromBody] UpdateFenRequest request,
        CancellationToken ct)
    {
        var result = await _analysisService.AnalyseAndBroadcastAsync(sessionId, request.Fen, ct);

        var dto = new AnalysisResultDto(
            result.Fen,
            new MoveEvaluationDto(result.BestMove.Move, result.BestMove.MoveSan,
                result.BestMove.CentipawnScore, result.BestMove.MateIn,
                result.BestMove.Depth, result.BestMove.PrincipalVariation),
            result.TopMoves.Select(m => new MoveEvaluationDto(
                m.Move, m.MoveSan, m.CentipawnScore, m.MateIn, m.Depth, m.PrincipalVariation
            )).ToList(),
            result.Turn,
            result.IsCheck,
            result.IsCheckmate,
            result.IsStalemate,
            result.Cached,
            result.AnalysedAt
        );

        return Ok(ApiResponse<AnalysisResultDto>.Ok(dto));
    }
}
