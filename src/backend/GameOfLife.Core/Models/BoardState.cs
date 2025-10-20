using System.Text;

namespace GameOfLife.Core.Models;

public sealed class BoardState
{
    public int Width { get; }
    public int Height { get; }
    public int Generation { get; }
    private readonly bool[] _cells; 

    public BoardState(int width, int height, bool[] cells, int generation = 0)
    {
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException("Board dimensions must be positive.");
        if (cells is null || cells.Length != width * height) throw new ArgumentException("Cells array length must equal width*height.");
        Width = width;
        Height = height;
        _cells = cells;
        Generation = generation;
    }

    public bool IsAlive(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return false; 
        return _cells[y * Width + x];
    }

    public IEnumerable<Cell> LiveCells()
    {
        for (int y = 0; y < Height; y++)
        {
            int rowOffset = y * Width;
            for (int x = 0; x < Width; x++)
            {
                if (_cells[rowOffset + x])
                    yield return new Cell(x, y);
            }
        }
    }

    public bool IsEmpty
    {
        get
        {
            for (int i = 0; i < _cells.Length; i++)
                if (_cells[i]) return false;
            return true;
        }
    }

    public BoardState WithGeneration(int generation) => new(Width, Height, (bool[])_cells.Clone(), generation);

    public static BoardState FromLiveCells(int width, int height, IEnumerable<Cell> liveCells, int generation = 0)
    {
        var arr = new bool[width * height];
        foreach (var c in liveCells)
        {
            if (c.X < 0 || c.Y < 0 || c.X >= width || c.Y >= height)
                throw new ArgumentOutOfRangeException($"Cell out of bounds: ({c.X},{c.Y}) within {width}x{height}.");
            arr[c.Y * width + c.X] = true;
        }
        return new BoardState(width, height, arr, generation);
    }

    
    public string Signature()
    {
        
        var bitCount = _cells.Length;
        var byteCount = (bitCount + 7) / 8;
        Span<byte> bytes = byteCount <= 1024 ? stackalloc byte[byteCount] : new byte[byteCount];

        for (int i = 0; i < bitCount; i++)
        {
            if (_cells[i])
            {
                int bIndex = i >> 3; 
                int bOffset = i & 7; 
                bytes[bIndex] |= (byte)(1 << bOffset);
            }
        }

        
        return $"{Width}x{Height}:{Convert.ToBase64String(bytes)}";
    }

    
    internal bool GetAtIndex(int index) => _cells[index];

    internal bool[] CloneCells() => (bool[])_cells.Clone();
}