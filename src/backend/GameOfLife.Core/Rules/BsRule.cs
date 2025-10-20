using GameOfLife.Core.Abstractions;
using System.Text;

namespace GameOfLife.Core.Rules;

public sealed class BsRule : IGameOfLifeRule
{
    
    private readonly bool[] _birth = new bool[9];
    private readonly bool[] _survival = new bool[9];

    public string Name { get; }
    private readonly string _notation;

    public BsRule(IEnumerable<int> birth, IEnumerable<int> survival, string name, string? notation = null)
    {
        foreach (var b in birth)
        {
            if (b is < 0 or > 8) throw new ArgumentOutOfRangeException(nameof(birth), "Birth values must be between 0 and 8.");
            _birth[b] = true;
        }
        foreach (var s in survival)
        {
            if (s is < 0 or > 8) throw new ArgumentOutOfRangeException(nameof(survival), "Survival values must be between 0 and 8.");
            _survival[s] = true;
        }

        Name = name;
        _notation = notation ?? ToNotation();
    }

    public bool Next(bool isAlive, int liveNeighbors)
    {
        if (liveNeighbors is < 0 or > 8) return false;
        return isAlive ? _survival[liveNeighbors] : _birth[liveNeighbors];
    }

    public string ToNotation()
    {
        static string Part(string prefix, bool[] flags)
        {
            var sb = new StringBuilder(prefix);
            for (int i = 0; i <= 8; i++)
            {
                if (flags[i]) sb.Append(i);
            }
            return sb.ToString();
        }

        return $"{Part("B", _birth)}/{Part("S", _survival)}";
    }

    public static bool TryParse(string notation, out BsRule? rule, out string? error)
    {
        rule = null;
        error = null;

        if (string.IsNullOrWhiteSpace(notation))
        {
            error = "Rule notation is empty.";
            return false;
        }

        string norm = notation.Trim().ToUpperInvariant().Replace(" ", "");
        
        string[] parts = norm.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2) { error = "Invalid rule format. Expect 'B.../S...'"; return false; }

        string? bPart = parts.FirstOrDefault(p => p.StartsWith("B"));
        string? sPart = parts.FirstOrDefault(p => p.StartsWith("S"));
        if (bPart is null || sPart is null) { error = "Rule must contain 'B' and 'S' sections."; return false; }

        bool[] birth = new bool[9];
        bool[] survival = new bool[9];

        static bool ParseDigits(string part, char prefix, bool[] target, out string? err)
        {
            err = null;
            if (part.Length < 1 || part[0] != prefix)
            {
                err = $"Part must start with '{prefix}'.";
                return false;
            }
            for (int i = 1; i < part.Length; i++)
            {
                if (!char.IsDigit(part[i])) { err = "Non-digit in rule counts."; return false; }
                int val = part[i] - '0';
                if (val < 0 || val > 8) { err = "Counts must be between 0 and 8."; return false; }
                target[val] = true;
            }
            return true;
        }

        if (!ParseDigits(bPart, 'B', birth, out error)) return false;
        if (!ParseDigits(sPart, 'S', survival, out error)) return false;

        var bList = Enumerable.Range(0, 9).Where(i => birth[i]);
        var sList = Enumerable.Range(0, 9).Where(i => survival[i]);
        rule = new BsRule(bList, sList, $"Rule {norm}", norm);
        return true;
    }

    public static BsRule Conway() => new(new[] { 3 }, new[] { 2, 3 }, "Conway B3/S23", "B3/S23");
}