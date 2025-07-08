using System.Collections.Generic;

namespace Nonograms.Game;

public record Board
{
    public int width;
    public int height;
    public Cell[,] cells = new Cell[,]{};
    public List<Constraint> columnConstraints = [];
    public List<Constraint> rowConstraints = [];
    public int longestColumnConstraint;
    public int longestRowConstraint;
}

public record Cell
{
    public bool isSolid; // actual nonogram truth
    public CellContents contents; // gameplay state
}

public record Constraint
{
    public int[] value;
    public bool[] satisfied;
}

public enum CellContents { Nothing, Filled, Cross, Circle }

public enum GameState { Playing, Victorious }

public abstract record Move;
public record CellMove(int X, int Y, CellContents PreviousContents, CellContents NewContents) : Move;
public record MultiMove(IEnumerable<CellMove> Moves) : Move;