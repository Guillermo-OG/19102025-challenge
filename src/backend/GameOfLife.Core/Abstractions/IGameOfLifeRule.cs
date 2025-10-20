namespace GameOfLife.Core.Abstractions;

public interface IGameOfLifeRule
{
    bool Next(bool isAlive, int liveNeighbors);

    string Name { get; }

    string ToNotation();
}