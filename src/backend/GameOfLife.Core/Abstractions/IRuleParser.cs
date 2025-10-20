namespace GameOfLife.Core.Abstractions;

public interface IRuleParser
{
    
    bool TryParse(string notation, out IGameOfLifeRule? rule, out string? error);

    
    IGameOfLifeRule Default { get; }
}