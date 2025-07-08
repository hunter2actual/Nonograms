using System;
using System.Collections.Generic;
using System.Linq;

namespace Nonograms.Game;

public static class MoveExtensions
{
    public static string ToString(this Move move)
    {
        return move switch
        {
            CellMove c => $"{c.X},{c.Y},{c.PreviousContents},{c.NewContents}",
            MultiMove m => string.Join(';', m.Moves.Select(ToString)),
            _ => string.Empty
        };
    }

    public static Move ToMove(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            throw new Exception($"Unexpected string {str} when processing Nonogram Undo/Redo state");
        
        if (str.Contains(';'))
        {
            return new MultiMove(str.Split(';').Select(ToCellMove));
        }

        return ToCellMove(str);
    }

    private static CellMove ToCellMove(string str)
    {
        var split = str.Split(',');
        if (split.Length != 4)
            throw new Exception($"Unexpected string {str} when processing Nonogram Undo/Redo state");

        return new CellMove(
            int.Parse(split[0]),
            int.Parse(split[1]),
            Enum.Parse<CellContents>(split[2]),
            Enum.Parse<CellContents>(split[3]));
    }
}