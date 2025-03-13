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

    public static CellContents GetCellContents(this Board board, int x, int y)
    {
        return board.cells[x, y].contents;
    }

    public static void SetCellContents(this Board board, int x, int y, CellContents contents)
    {
        board.cells[x, y].contents = contents;
    }
    
    public static bool IsConstraintFulfilled(this Board board, Constraint constraint, int? x, int? y)
    {
        if (x is not null) // column constraint
        {
            return constraint.value.SequenceEqual(GetColumnContents(board, x.Value));
        }
        if (y is not null) // row constraint
        {
            return constraint.value.SequenceEqual(GetRowContents(board, y.Value));
        }

        return false;
    }

    public static int[] GetRowContents(this Board board, int y)
    {
        List<int> contents = [];
        int run = 0;
        for (int x = 0; x < board.width; x++)
        {
            if (board.cells[x, y].contents is CellContents.Filled)
                run++;
            else if (run > 0)
            {
                contents.Add(run);
                run = 0;
            }
        }
        if (run > 0)
            contents.Add(run);
        
        if (contents.Count == 0)
            contents.Add(0);
        
        return contents.ToArray();
    }
    
    public static int[] GetColumnContents(this Board board, int x)
    {
        List<int> contents = [];
        int run = 0;
        for (int y = 0; y < board.height; y++)
        {
            if (board.cells[x, y].contents is CellContents.Filled)
                run++;
            else if (run > 0)
            {
                contents.Add(run);
                run = 0;
            }
        }
        if (run > 0)
            contents.Add(run);

        if (contents.Count == 0)
            contents.Add(0);
        
        return contents.ToArray();
    }
}