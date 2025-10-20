using GameOfLife.Core.Abstractions;
using GameOfLife.Core.Models;

namespace GameOfLife.Core.Services;

public interface IGameOfLifeEngine
{
    BoardState Next(BoardState current, IGameOfLifeRule rule);
}

public sealed class GameOfLifeEngine : IGameOfLifeEngine
{
    public BoardState Next(BoardState current, IGameOfLifeRule rule)
    {
        var width = current.Width;
        var height = current.Height;
        var next = new bool[width * height];

        for (int y = 0; y < height; y++)
        {
            int rowOffset = y * width;
            for (int x = 0; x < width; x++)
            {
                int idx = rowOffset + x;
                int neighbors = 0;

                
                for (int dy = -1; dy <= 1; dy++)
                {
                    int ny = y + dy;
                    if (ny < 0 || ny >= height) continue;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx;
                        if (nx < 0 || nx >= width) continue;

                        if (current.GetAtIndex(ny * width + nx)) neighbors++;
                    }
                }

                bool alive = current.GetAtIndex(idx);
                next[idx] = rule.Next(alive, neighbors);
            }
        }

        return new BoardState(width, height, next, current.Generation + 1);
    }
}