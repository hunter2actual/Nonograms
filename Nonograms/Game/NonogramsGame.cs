using System;
using System.Collections.Generic;
using System.Linq;
using Nonograms.FileSystem;

namespace Nonograms.Game;

public class NonogramsGame
{
    private BoardBuilder _boardBuilder;
    public Board Board;
    public GameState GameState { get; private set; }
    
    public NonogramsGame()
    {
        _boardBuilder = new BoardBuilder();
        Board = _boardBuilder.Build();
        GameState = GameState.Playing;
        UpdateHintSatisfaction();
        AutoCrossAll();
    }

    public NonogramsGame(LoadedPuzzle puzzle)
    {
        _boardBuilder = new BoardBuilder();
        _boardBuilder.WithData(puzzle.data);
        Board = _boardBuilder.Build();
        GameState = GameState.Playing;
        UpdateHintSatisfaction();
        AutoCrossAll();
    }

    public void Click(int x, int y)
    {
        if (GameState is not GameState.Playing) return;
        var cell = Board.cells[x, y];
        cell.contents = cell.contents switch
        {
            CellContents.Filled => CellContents.Nothing,
            _ => CellContents.Filled
        };
        Board.cells[x, y] = cell;

        UpdateHintSatisfaction();
        AutoCross(x, y);
        CheckWin();
    }
    
    public void Clear(int x, int y)
        {
        if (GameState is not GameState.Playing) return;
        var cell = Board.cells[x, y];
        cell.contents = CellContents.Nothing;
        Board.cells[x, y] = cell;
        UpdateHintSatisfaction();
        AutoCross(x, y);
        CheckWin();
    }
    
    public void Clear(IEnumerable<(int x, int y)> cells)
    {
        if (GameState is not GameState.Playing) return;
        foreach (var coords in cells)
        {
            var cell = Board.cells[coords.x, coords.y];
            cell.contents = CellContents.Nothing;
            Board.cells[coords.x, coords.y] = cell;
        }
        UpdateHintSatisfaction();
        AutoCross(cells);
        CheckWin();
    }
    
    public void Fill(int x, int y)
    {
        if (GameState is not GameState.Playing) return;
        var cell = Board.cells[x, y];
        cell.contents = CellContents.Filled;
        Board.cells[x, y] = cell;
        UpdateHintSatisfaction();
        AutoCross(x, y);
        CheckWin();
    }
    
    public void Fill(IEnumerable<(int x, int y)> cells)
    {
        if (GameState is not GameState.Playing) return;
        foreach (var coords in cells)
        {
            var cell = Board.cells[coords.x, coords.y];
            cell.contents = CellContents.Filled;
            Board.cells[coords.x, coords.y] = cell;
        }
        UpdateHintSatisfaction();
        AutoCross(cells);
        CheckWin();
    }

    public void RightClick(int x, int y)
    {
        if (GameState is not GameState.Playing) return;
        var cell = Board.cells[x, y];
        cell.contents = cell.contents switch
        {
            CellContents.Filled => CellContents.Cross,
            CellContents.Cross => CellContents.Circle,
            CellContents.Circle => CellContents.Nothing,
            CellContents.Nothing => CellContents.Cross,
            _ => throw new NotSupportedException()
        };
        Board.cells[x, y] = cell;
    }

    private void UpdateHintSatisfaction()
    {
        for (var x = 0; x < Board.columnConstraints.Count; x++)
        {
            var constraint = Board.columnConstraints[x];
            constraint.satisfied = Board.IsConstraintFulfilled(constraint, x, null);
        }

        for (var y = 0; y < Board.rowConstraints.Count; y++)
        {
            var constraint = Board.rowConstraints[y];
            constraint.satisfied = Board.IsConstraintFulfilled(constraint, null, y);
        }
    }

    private void CheckWin()
    {
        if(Board.cells.ToList().All(c =>
               (c.isSolid && c.contents == CellContents.Filled)
               || (!c.isSolid && c.contents != CellContents.Filled)))
        {
            GameState = GameState.Victorious;
            
            // clear crosses and dots
            foreach (var cell in Board.cells.ToList())
            {
                cell.contents = cell.contents switch {
                    CellContents.Filled => CellContents.Filled,
                    _ => CellContents.Nothing
                };
            }
        }
    }

    private void AutoCross(int x, int y)
    {
        if (Board.rowConstraints[y].satisfied) CrossRow(y);
        if (Board.columnConstraints[x].satisfied) CrossColumn(x);
    }

    private void AutoCross(IEnumerable<(int x, int y)> cells)
    {
        foreach (var cell in cells)
        {
            AutoCross(cell.x, cell.y);
        }
    }
    
    private void AutoCrossAll()
    {
        for (int x = 0; x < Board.width; x++)
        {
            for (int y = 0; y < Board.height; y++)
            {
                if (Board.rowConstraints[y].satisfied) CrossRow(y);
                if (Board.columnConstraints[x].satisfied) CrossColumn(x);
            }
        }
    }

    private void CrossRow(int y)
    {
        for (int x = 0; x < Board.width; x++)
        {
            Board.cells[x, y].contents = Board.cells[x, y].contents switch
            {
                CellContents.Filled => CellContents.Filled,
                _ => CellContents.Cross
            };
        }
    }
    
    private void CrossColumn(int x)
    {
        for (int y = 0; y < Board.height; y++)
        {
            Board.cells[x, y].contents = Board.cells[x, y].contents switch
            {
                CellContents.Filled => CellContents.Filled,
                _ => CellContents.Cross
            };
        }
    }
}