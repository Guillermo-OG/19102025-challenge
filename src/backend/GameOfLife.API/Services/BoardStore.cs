using GameOfLife.Core.Abstractions;
using GameOfLife.Core.Models;

namespace GameOfLife.API.Services;

public interface IBoardStore
{
    BoardEntity Create(BoardState initial, IGameOfLifeRule rule);
    bool TryGet(string id, out BoardEntity? entity);
    IEnumerable<BoardEntity> All();

    
    void Save(BoardEntity entity);
}

public sealed class BoardEntity
{
    public string Id { get; }
    public object SyncRoot { get; } = new object();

    public BoardState State { get; set; }
    public IGameOfLifeRule Rule { get; set; }
    public DateTime CreatedUtc { get; }
    public DateTime UpdatedUtc { get; set; }

    public BoardEntity(string id, BoardState state, IGameOfLifeRule rule)
    {
        Id = id;
        State = state;
        Rule = rule;
        CreatedUtc = DateTime.UtcNow;
        UpdatedUtc = CreatedUtc;
    }
}