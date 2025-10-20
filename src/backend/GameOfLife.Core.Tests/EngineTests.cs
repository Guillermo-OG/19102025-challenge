using FluentAssertions;
using GameOfLife.Core.Models;
using GameOfLife.Core.Rules;
using GameOfLife.Core.Services;
using Xunit;

namespace GameOfLife.Core.Tests;

public class EngineTests
{
    private readonly GameOfLifeEngine _engine = new();
    private readonly BsRule _conway = BsRule.Conway();

    [Fact]
    public void StillLife_Block_RemainsStable()
    {
        
        var state = BoardState.FromLiveCells(4, 4, new[]
        {
            new Cell(1,1), new Cell(2,1),
            new Cell(1,2), new Cell(2,2),
        });
        var next = _engine.Next(state, _conway);

        next.Generation.Should().Be(state.Generation + 1);
        next.Signature().Should().Be(state.Signature());
    }

    [Fact]
    public void Oscillator_Blinker_Period2()
    {
        
        var state = BoardState.FromLiveCells(5, 5, new[]
        {
            new Cell(1,2), new Cell(2,2), new Cell(3,2)
        });

        var next = _engine.Next(state, _conway);            
        var next2 = _engine.Next(next, _conway);            

        next2.Signature().Should().Be(state.Signature());
    }

    [Fact]
    public void EmptyBoard_IsExtinct()
    {
        var state = BoardState.FromLiveCells(3, 3, Array.Empty<Cell>());
        state.IsEmpty.Should().BeTrue();

        var result = SequenceRunner.FindConclusion(state, _engine, _conway, 10);
        result.Status.Should().Be(ConclusionStatus.Extinct);
        result.Period.Should().Be(1);
    }
}