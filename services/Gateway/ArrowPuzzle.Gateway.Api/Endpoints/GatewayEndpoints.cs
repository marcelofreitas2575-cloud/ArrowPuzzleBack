using System.Security.Claims;
using ArrowPuzzle.Gateway.Api.Authentication;
using ArrowPuzzle.Gateway.Application.Levels.GetLevel;
using ArrowPuzzle.Gateway.Application.Progress.CompleteLevel;
using ArrowPuzzle.Gateway.Application.Progress.GetProgress;
using ArrowPuzzle.Gateway.Application.Sessions;
using ArrowPuzzle.Gateway.Application.Sessions.ApplyMove;
using ArrowPuzzle.Gateway.Application.Sessions.GetSession;
using ArrowPuzzle.Gateway.Application.Sessions.StartLevel;
using Grpc.Core;
using MediatR;

namespace ArrowPuzzle.Gateway.Api.Endpoints;

public static class GatewayEndpoints
{
    public static RouteGroupBuilder MapArrowPuzzleApi(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1").RequireAuthorization();

        group.MapGet("/levels/{levelId}", GetLevelAsync);
        group.MapGet("/me/progress", GetMyProgressAsync);
        group.MapPost("/me/progress/complete", CompleteLevelAsync);
        group.MapPost("/sessions", StartSessionAsync);
        group.MapGet("/sessions/{sessionId}", GetSessionAsync);
        group.MapPost("/sessions/{sessionId}/moves", ApplyMoveAsync);

        return group;
    }

    private static async Task<IResult> GetLevelAsync(string levelId, IMediator mediator, CancellationToken cancellationToken)
    {
        try
        {
            var level = await mediator.Send(new GetLevelQuery(levelId), cancellationToken);
            return level is null ? Results.NotFound() : Results.Ok(level);
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (RpcException ex)
        {
            return ToProblemResult(ex);
        }
    }

    private static async Task<IResult> CompleteLevelAsync(
        CompleteLevelRequest body,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.LevelId))
        {
            return Results.BadRequest(new { error = "levelId is required." });
        }

        try
        {
            var userId = UserClaims.GetSubjectId(user);
            var progress = await mediator.Send(
                new CompleteLevelCommand(userId, body.LevelId, body.SessionId),
                cancellationToken);
            return Results.Ok(progress);
        }
        catch (LevelCompletionNotVerifiedException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (SessionAccessDeniedException ex)
        {
            return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (RpcException ex)
        {
            return ToProblemResult(ex);
        }
    }

    private static async Task<IResult> GetMyProgressAsync(
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = UserClaims.GetSubjectId(user);
            var progress = await mediator.Send(new GetProgressQuery(userId), cancellationToken);
            return Results.Ok(progress);
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (RpcException ex)
        {
            return ToProblemResult(ex);
        }
    }

    private static async Task<IResult> StartSessionAsync(
        StartSessionRequest body,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.LevelId))
        {
            return Results.BadRequest(new { error = "levelId is required." });
        }

        try
        {
            var userId = UserClaims.GetSubjectId(user);
            var session = await mediator.Send(new StartLevelCommand(body.LevelId, userId), cancellationToken);
            return Results.Created($"/api/v1/sessions/{session.SessionId}", session);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (RpcException ex)
        {
            return ToProblemResult(ex);
        }
    }

    private static async Task<IResult> GetSessionAsync(
        string sessionId,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = UserClaims.GetSubjectId(user);
            var session = await mediator.Send(new GetSessionQuery(sessionId, userId), cancellationToken);
            return session is null ? Results.NotFound() : Results.Ok(session);
        }
        catch (SessionAccessDeniedException ex)
        {
            return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (RpcException ex)
        {
            return ToProblemResult(ex);
        }
    }

    private static async Task<IResult> ApplyMoveAsync(
        string sessionId,
        ApplyMoveRequest body,
        ClaimsPrincipal user,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.ArrowId))
        {
            return Results.BadRequest(new { error = "arrowId is required." });
        }

        try
        {
            var userId = UserClaims.GetSubjectId(user);
            var result = await mediator.Send(
                new ApplyMoveCommand(sessionId, userId, body.ArrowId),
                cancellationToken);

            return Results.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (SessionAccessDeniedException ex)
        {
            return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (RpcException ex)
        {
            return ToProblemResult(ex);
        }
    }

    private static IResult ToProblemResult(RpcException ex) =>
        ex.StatusCode switch
        {
            StatusCode.InvalidArgument => Results.BadRequest(new { error = ex.Status.Detail }),
            StatusCode.NotFound => Results.NotFound(new { error = ex.Status.Detail }),
            StatusCode.Unavailable => Results.Problem(
                detail: ex.Status.Detail,
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Upstream service unavailable"),
            _ => Results.Problem(
                detail: ex.Status.Detail,
                statusCode: StatusCodes.Status502BadGateway,
                title: "Upstream service error"),
        };

    public sealed record StartSessionRequest(string LevelId);

    public sealed record ApplyMoveRequest(string ArrowId);

    public sealed record CompleteLevelRequest(string LevelId, string? SessionId = null);
}
