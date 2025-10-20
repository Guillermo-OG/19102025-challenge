using System.Text.Json;
using GameOfLife.API.Services;
using GameOfLife.Core.Abstractions;
using GameOfLife.Core.Models;

namespace GameOfLife.API.Services;

public sealed class FileBoardStore : IBoardStore
{
    private readonly string _path;
    private readonly IRuleParser _parser;
    private readonly Dictionary<string, BoardEntity> _boards = new();
    private readonly object _gate = new();

    private record PersistedCell(int X, int Y);
    private record PersistedBoard(
        string Id,
        int Width,
        int Height,
        int Generation,
        string Rule,
        List<PersistedCell> LiveCells,
        DateTime CreatedUtc,
        DateTime UpdatedUtc
    );

    public FileBoardStore(string path, IRuleParser parser)
    {
        _path = path;
        _parser = parser;
        Load();
    }

    public BoardEntity Create(BoardState initial, IGameOfLifeRule rule)
    {
        lock (_gate)
        {
            var id = Guid.NewGuid().ToString("n");
            var entity = new BoardEntity(id, initial, rule);
            _boards[id] = entity;
            PersistLocked();
            return entity;
        }
    }

    public bool TryGet(string id, out BoardEntity? entity)
    {
        lock (_gate)
        {
            return _boards.TryGetValue(id, out entity);
        }
    }

    public IEnumerable<BoardEntity> All()
    {
        lock (_gate)
        {
            
            return _boards.Values.ToArray();
        }
    }

    public void Save(BoardEntity entity)
    {
        lock (_gate)
        {
            
            PersistLocked();
        }
    }

    private void Load()
    {
        lock (_gate)
        {
            try
            {
                if (!File.Exists(_path)) return;

                var json = File.ReadAllText(_path);
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var persisted = JsonSerializer.Deserialize<List<PersistedBoard>>(json, opts) ?? new();

                _boards.Clear();
                foreach (var pb in persisted)
                {
                    var live = pb.LiveCells.Select(c => new Cell(c.X, c.Y));
                    var state = BoardState.FromLiveCells(pb.Width, pb.Height, live, pb.Generation);

                    if (!_parser.TryParse(pb.Rule, out var rule, out _))
                        rule = _parser.Default;

                    
                    
                    var entity = new BoardEntity(pb.Id, state, rule!);
                    entity.UpdatedUtc = pb.UpdatedUtc;
                    _boards[pb.Id] = entity;
                }
            }
            catch
            {
                
                _boards.Clear();
            }
        }
    }

    private void PersistLocked()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var all = _boards.Values.Select(e =>
        {
            var live = e.State.LiveCells().Select(c => new PersistedCell(c.X, c.Y)).ToList();
            return new PersistedBoard(
                e.Id,
                e.State.Width,
                e.State.Height,
                e.State.Generation,
                e.Rule.ToNotation(),
                live,
                e.CreatedUtc,
                e.UpdatedUtc
            );
        }).ToList();

        var opts = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(all, opts);
        var tmp = _path + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(_path))
            File.Replace(tmp, _path, null);
        else
            File.Move(tmp, _path);
    }
}