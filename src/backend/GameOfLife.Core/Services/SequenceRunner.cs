using GameOfLife.Core.Abstractions;
using GameOfLife.Core.Models;

namespace GameOfLife.Core.Services;

public enum ConclusionStatus
{
    Stable,          
    Extinct,         
    CycleDetected,   
    AttemptsExceeded 
}

public sealed record ConclusionResult(ConclusionStatus Status, BoardState Last, int StepsTaken, int? Period = null);

public static class SequenceRunner
{
    
    public static BoardState Advance(BoardState start, IGameOfLifeEngine engine, IGameOfLifeRule rule, int steps)
    {
        if (steps < 0) throw new ArgumentOutOfRangeException(nameof(steps));
        var cur = start;
        for (int i = 0; i < steps; i++)
        {
            cur = engine.Next(cur, rule);
        }
        return cur;
    }

    
    
    
    public static ConclusionResult FindConclusion(BoardState start, IGameOfLifeEngine engine, IGameOfLifeRule rule, int maxAttempts)
    {
        if (maxAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(maxAttempts));

        
        var seen = new Dictionary<string, int>(capacity: Math.Min(maxAttempts + 1, 32768));
        var current = start;
        var sig = current.Signature();
        seen[sig] = current.Generation;

        for (int i = 1; i <= maxAttempts; i++)
        {
            current = engine.Next(current, rule);

            if (current.IsEmpty)
                return new ConclusionResult(ConclusionStatus.Extinct, current, i, Period: 1);

            var s = current.Signature();
            if (seen.TryGetValue(s, out int firstGen))
            {
                int period = current.Generation - firstGen;
                if (period == 1)
                    return new ConclusionResult(ConclusionStatus.Stable, current, i, Period: 1);
                else
                    return new ConclusionResult(ConclusionStatus.CycleDetected, current, i, Period: period);
            }

            seen[s] = current.Generation;
        }

        return new ConclusionResult(ConclusionStatus.AttemptsExceeded, current, maxAttempts, null);
    }
}