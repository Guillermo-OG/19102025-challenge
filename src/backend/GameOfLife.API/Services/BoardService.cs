using GameOfLife.API.DTOs;
using GameOfLife.Core.Abstractions;
using GameOfLife.Core.Models;
using GameOfLife.Core.Services;

namespace GameOfLife.API.Services;

public interface IBoardService
{
    Task<BoardEntity> CreateAsync(CreateBoardRequest req, CancellationToken ct);
    Task<BoardEntity> GetAsync(string id, CancellationToken ct);

    
    Task<BoardEntity> UpdateAsync(string id, UpdateBoardRequest req, CancellationToken ct);

    Task<(BoardEntity entity, BoardState result)> NextAsync(string id, NextRequest req, CancellationToken ct);
    Task<(BoardEntity entity, BoardState result)> AdvanceAsync(string id, AdvanceRequest req, CancellationToken ct);
    Task<(BoardEntity entity, ConclusionResult result)> FinalAsync(string id, FinalRequest req, CancellationToken ct);
    BoardResponse ToDto(BoardEntity entity, BoardState? stateOverride = null, string? ruleOverrideNotation = null);
}

public sealed class BoardService : IBoardService
{
    private readonly IBoardStore _store;
    private readonly IGameOfLifeEngine _engine;
    private readonly IRuleParser _parser;

    public BoardService(IBoardStore store, IGameOfLifeEngine engine, IRuleParser parser)
    {
        _store = store;
        _engine = engine;
        _parser = parser;
    }

    public Task<BoardEntity> CreateAsync(CreateBoardRequest req, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var rule = ParseOrDefault(req.Rule);
        var live = req.LiveCells.Select(c => new Cell(c.X, c.Y));
        var state = BoardState.FromLiveCells(req.Width, req.Height, live);
        var entity = _store.Create(state, rule);
        return Task.FromResult(entity);
    }

    public Task<BoardEntity> GetAsync(string id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!_store.TryGet(id, out var entity) || entity is null)
            throw new KeyNotFoundException($"Board '{id}' was not found.");
        return Task.FromResult(entity);
    }

    public async Task<BoardEntity> UpdateAsync(string id, UpdateBoardRequest req, CancellationToken ct)
    {
        var entity = await GetAsync(id, ct);
        var rule = ParseOr(entity.Rule, req.Rule);
        lock (entity.SyncRoot)
        {
            var gen = req.Generation ?? 0;
            var live = req.LiveCells.Select(c => new Cell(c.X, c.Y));
            var state = BoardState.FromLiveCells(entity.State.Width, entity.State.Height, live, gen);

            entity.State = state;
            entity.Rule = rule;
            entity.UpdatedUtc = DateTime.UtcNow;

            _store.Save(entity);
        }
        return entity;
    }

    public async Task<(BoardEntity entity, BoardState result)> NextAsync(string id, NextRequest req, CancellationToken ct)
    {
        var entity = await GetAsync(id, ct);
        var rule = ParseOr(entity.Rule, req.Rule);
        BoardState result;
        lock (entity.SyncRoot)
        {
            result = _engine.Next(entity.State, rule);
            if (req.Persist)
            {
                entity.State = result;
                entity.Rule = rule;
                entity.UpdatedUtc = DateTime.UtcNow;
                _store.Save(entity); 
            }
        }
        return (entity, result);
    }

    public async Task<(BoardEntity entity, BoardState result)> AdvanceAsync(string id, AdvanceRequest req, CancellationToken ct)
    {
        if (req.Steps < 1) throw new ArgumentException("Steps must be >= 1.", nameof(req.Steps));
        var entity = await GetAsync(id, ct);
        var rule = ParseOr(entity.Rule, req.Rule);
        BoardState result;
        lock (entity.SyncRoot)
        {
            result = SequenceRunner.Advance(entity.State, _engine, rule, req.Steps);
            if (req.Persist)
            {
                entity.State = result;
                entity.Rule = rule;
                entity.UpdatedUtc = DateTime.UtcNow;
                _store.Save(entity); 
            }
        }
        return (entity, result);
    }

    public async Task<(BoardEntity entity, ConclusionResult result)> FinalAsync(string id, FinalRequest req, CancellationToken ct)
    {
        if (req.MaxAttempts < 1) throw new ArgumentException("MaxAttempts must be >= 1.", nameof(req.MaxAttempts));
        var entity = await GetAsync(id, ct);
        var rule = ParseOr(entity.Rule, req.Rule);
        ConclusionResult result;
        lock (entity.SyncRoot)
        {
            result = SequenceRunner.FindConclusion(entity.State, _engine, rule, req.MaxAttempts);
            if (req.Persist && (result.Status is ConclusionStatus.Extinct or ConclusionStatus.Stable))
            {
                entity.State = result.Last;
                entity.Rule = rule;
                entity.UpdatedUtc = DateTime.UtcNow;
                _store.Save(entity); 
            }
        }
        return (entity, result);
    }

    public BoardResponse ToDto(BoardEntity entity, BoardState? stateOverride = null, string? ruleOverrideNotation = null)
    {
        var state = stateOverride ?? entity.State;
        var ruleText = ruleOverrideNotation ?? entity.Rule.ToNotation();
        var live = state.LiveCells().ToArray();
        return new BoardResponse
        {
            Id = entity.Id,
            Width = state.Width,
            Height = state.Height,
            Generation = state.Generation,
            Rule = ruleText,
            LiveCells = live.Select(c => new CellDto(c.X, c.Y)).ToArray(),
            LiveCount = live.Length
        };
    }

    private IGameOfLifeRule ParseOr(IGameOfLifeRule current, string? maybeNotation)
    {
        if (string.IsNullOrWhiteSpace(maybeNotation)) return current;
        if (!_parser.TryParse(maybeNotation!, out var rule, out var err) || rule is null)
            throw new ArgumentException(err ?? "Invalid rule.", nameof(maybeNotation));
        return rule;
    }

    private IGameOfLifeRule ParseOrDefault(string? maybeNotation)
    {
        if (string.IsNullOrWhiteSpace(maybeNotation)) return _parser.Default;
        if (!_parser.TryParse(maybeNotation!, out var rule, out var err) || rule is null)
            throw new ArgumentException(err ?? "Invalid rule.", nameof(maybeNotation));
        return rule;
    }
}