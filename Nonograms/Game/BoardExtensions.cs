using System.Collections.Generic;
using System.Linq;

namespace Nonograms.Game;

public static class BoardExtensions
{
    public static void RevealAll(this Board board)
    {
        foreach (var cell in board.cells.ToList())
        {
            cell.contents = cell.isSolid ? CellContents.Filled : CellContents.Cross;
        }
    }

    public static bool IsVictory(this Board board) =>
        board.cells.ToList()
            .Where(c => c.isSolid)
            .All(c => c.contents is CellContents.Filled)
        && board.cells.ToList()
            .Where(c => !c.isSolid)
            .All(c => c.contents is not CellContents.Filled);

    public static bool[] GetHintSatisfaction(this Board board, Constraint constraint, int? x, int? y)
    {
        CellContents[] line = [];
        if (x is not null) // column constraint
        {
            line = GetColumnContents(board, x.Value);
        }
        if (y is not null) // row constraint
        {
            line = GetRowContents(board, y.Value);
        }

        return HintSatisfactionAnalyser.CheckHintSatisfaction(line, constraint.value);
    }

    private static CellContents[] GetRowContents(this Board board, int y)
    {
        List<CellContents> contents = [];
        for (int x = 0; x < board.width; x++)
        {
            contents.Add(board.cells[x, y].contents);
        }
        return contents.ToArray();
    }

    private static CellContents[] GetColumnContents(this Board board, int x)
    {
        List<CellContents> contents = [];
        for (int y = 0; y < board.height; y++)
        {
            contents.Add(board.cells[x, y].contents);
        }
        
        return contents.ToArray();
    }
}