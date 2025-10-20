using System.ComponentModel.DataAnnotations;

namespace GameOfLife.API.DTOs;

public record CellDto(int X, int Y);

public record CreateBoardRequest
{
    [Range(1, 10_000)]
    public int Width { get; init; }

    [Range(1, 10_000)]
    public int Height { get; init; }

    public List<CellDto> LiveCells { get; init; } = new();

    public string? Rule { get; init; }
}

public record BoardResponse
{
    public string Id { get; init; } = default!;
    public int Width { get; init; }
    public int Height { get; init; }
    public int Generation { get; init; }
    public string Rule { get; init; } = default!;
    public IReadOnlyList<CellDto> LiveCells { get; init; } = Array.Empty<CellDto>();
    public int LiveCount { get; init; }
}

public record NextRequest
{
    public bool Persist { get; init; } = true;
    public string? Rule { get; init; }
}

public record AdvanceRequest
{
    [Range(1, int.MaxValue)]
    public int Steps { get; init; }

    public bool Persist { get; init; } = true;

    public string? Rule { get; init; }
}

public record FinalRequest
{
    [Range(1, 2_000_000)]
    public int MaxAttempts { get; init; } = 50_000;

    public bool Persist { get; init; } = false;

    public string? Rule { get; init; }
}


public record UpdateBoardRequest
{
    public List<CellDto> LiveCells { get; init; } = new();
    public string? Rule { get; init; }
    public int? Generation { get; init; } 
}

public record ErrorResponse(string Code, string Message, object? Details = null);