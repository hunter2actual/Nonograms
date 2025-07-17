using System;
using System.Collections.Generic;
using System.Linq;
using Nonograms.FileSystem;

namespace Nonograms.Game;

public class NonogramsGame
{
    private readonly PuzzleStateManager _puzzleStateManager;
    private Stack<Move> _undoStack;
    private Stack<Move> _redoStack;
    private BoardBuilder _boardBuilder;
    public Board Board;
    public GameState GameState { get; private set; }
    
    public NonogramsGame(PuzzleStateManager puzzleStateManager, LoadedPuzzle puzzle)
    {
        _puzzleStateManager = puzzleStateManager;
        _undoStack = new Stack<Move>();
        _redoStack = new Stack<Move>();
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
        var prevContents = cell.contents;
        cell.contents = prevContents switch
        {
            CellContents.Filled => CellContents.Nothing,
            _ => CellContents.Filled
        };
        Board.cells[x, y] = cell;

        _undoStack.Push(new CellMove(x, y, prevContents, cell.contents));
        _redoStack.Clear();

        UpdateHintSatisfaction();
        AutoCross(x, y);
        CheckWin();
        SaveProgress();
    }

    public void RightClick(int x, int y)
    {
        if (GameState is not GameState.Playing) return;
        var cell = Board.cells[x, y];
        var prevContents = cell.contents;
        cell.contents = prevContents switch
        {
            CellContents.Filled => CellContents.Cross,
            CellContents.Cross => CellContents.Circle,
            CellContents.Circle => CellContents.Nothing,
            CellContents.Nothing => CellContents.Cross,
            _ => throw new NotSupportedException()
        };
        Board.cells[x, y] = cell;
        
        _undoStack.Push(new CellMove(x, y, prevContents, cell.contents));
        _redoStack.Clear();

        UpdateHintSatisfaction();
        AutoCross(x, y);
        CheckWin();
        SaveProgress();
    }

    public void Fill(IEnumerable<(int x, int y)> cells, CellContents newContents)
    {
        if (GameState is not GameState.Playing) return;

        var moves = new List<CellMove>();
        
        foreach (var coords in cells)
        {
            var cell = Board.cells[coords.x, coords.y];
            moves.Add(new CellMove(coords.x, coords.y, cell.contents, newContents));
            cell.contents = newContents;
            Board.cells[coords.x, coords.y] = cell;
        }
        
        _undoStack.Push(new MultiMove(moves));
        _redoStack.Clear();
        
        UpdateHintSatisfaction();
        AutoCross(cells);
        CheckWin();
        SaveProgress();
    }

    private void UpdateHintSatisfaction()
    {
        for (var x = 0; x < Board.columnConstraints.Count; x++)
        {
            var constraint = Board.columnConstraints[x];
            constraint.satisfied = Board.GetHintSatisfaction(constraint, x, null);
        }

        for (var y = 0; y < Board.rowConstraints.Count; y++)
        {
            var constraint = Board.rowConstraints[y];
            constraint.satisfied = Board.GetHintSatisfaction(constraint, null, y);
        }
    }

    private void CheckWin()
    {
        if(Board.cells.ToList().All(c =>
               (c.isSolid && c.contents == CellContents.Filled)
               || (!c.isSolid && c.contents != CellContents.Filled)))
        {
            GameState = GameState.Victorious;
            _undoStack.Clear();
            _redoStack.Clear();
            
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
        var prevCellContents = Board.cells.Select2D(c => c.contents);
        
        if (Board.rowConstraints[y].satisfied.All(s => s)) CrossRow(y);
        if (Board.columnConstraints[x].satisfied.All(s => s)) CrossColumn(x);

        var newCellContents = Board.cells.Select2D(c => c.contents);
        var moves = new List<CellMove>();
        for (int X = 0; X < Board.width; X++)
        {
            for (int Y = 0; Y < Board.height; Y++)
            {
                if (prevCellContents[X,Y] != newCellContents[X,Y])
                    moves.Add(new CellMove(X, Y, prevCellContents[X,Y], newCellContents[X,Y]));
            }
        }
        if (moves.Any())
        {
            _undoStack.Push(new MultiMove(moves));
            _redoStack.Clear();
        }
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
        var prevCellContents = Board.cells.Select2D(c => c.contents);
        
        for (int x = 0; x < Board.width; x++)
        {
            if (Board.columnConstraints[x].satisfied.All(s => s)) CrossColumn(x);
        }
        
        for (int y = 0; y < Board.height; y++)
        {
            if (Board.rowConstraints[y].satisfied.All(s => s)) CrossRow(y);
        }
        
        var newCellContents = Board.cells.Select2D(c => c.contents);
        var moves = new List<CellMove>();
        for (int X = 0; X < Board.width; X++)
        {
            for (int Y = 0; Y < Board.height; Y++)
            {
                if (prevCellContents[X,Y] != newCellContents[X,Y])
                    moves.Add(new CellMove(X, Y, prevCellContents[X,Y], newCellContents[X,Y]));
            }
        }
        if (moves.Any())
        {
            _undoStack.Push(new MultiMove(moves));
            _redoStack.Clear();
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

    public void InitialiseFromState(PuzzleState savedState)
    {
        if (savedState.BoardState is null || savedState.BoardState.Length == 0) return;
        
        for (int x = 0; x < Board.width; x++)
        {
            for (int y = 0; y < Board.height; y++)
            {
                if (savedState.Completed)
                {
                    Board.cells[x, y].contents = Board.cells[x, y].isSolid 
                        ? CellContents.Filled 
                        : CellContents.Nothing;
                }
                else
                {
                    Board.cells[x, y].contents = (CellContents)savedState.BoardState[x, y];
                }
            }
        }

        _undoStack = new Stack<Move>(savedState.UndoStack?.Select(x => x.ToMove()).Reverse() ?? Array.Empty<Move>());
        _redoStack = new Stack<Move>(savedState.RedoStack?.Select(x => x.ToMove()).Reverse()  ?? Array.Empty<Move>());
        
        UpdateHintSatisfaction();
        AutoCrossAll();
        CheckWin();
        SaveProgress();
    }

    public void SaveProgress()
    {
        var puzzleState = _puzzleStateManager.GetCurrentPuzzleState();
        puzzleState.Completed = GameState is GameState.Victorious;

        var boardState = new int[Board.width, Board.height];
        for (int x = 0; x < Board.width; x++)
        {
            for (int y = 0; y < Board.height; y++)
            {
                boardState[x, y] = (int)Board.cells[x, y].contents;
            }
        }
        puzzleState.BoardState = boardState;

        puzzleState.UndoStack = _undoStack.Select(MoveExtensions.ToString).ToArray();
        puzzleState.RedoStack = _redoStack.Select(MoveExtensions.ToString).ToArray();

        _puzzleStateManager.SetCurrentPuzzleState(puzzleState);
    }

    public bool CanUndo => _undoStack.Count != 0;
    public bool CanRedo => _redoStack.Count != 0;

    public void Undo()
    {
        if (_undoStack.Count == 0) return;

        var move = _undoStack.Pop();
        _redoStack.Push(move);

        ApplyMove(move, isUndo: true);
        UpdateHintSatisfaction();
        CheckWin();
        SaveProgress();
    }
    
    public void Redo()
    {
        if (_redoStack.Count == 0) return;

        var move = _redoStack.Pop();
        _undoStack.Push(move);

        ApplyMove(move, isUndo: false);
        UpdateHintSatisfaction();
        CheckWin();
        SaveProgress();
    }

    private void ApplyMove(Move move, bool isUndo)
    {
        switch (move)
        {
            case CellMove cellMove:
                Board.cells[cellMove.X, cellMove.Y].contents = isUndo ? cellMove.PreviousContents : cellMove.NewContents;
                break;
            case MultiMove multiMove:
                for (int i = 0; i < multiMove.Moves.Count(); i++)
                {
                    ApplyMove(multiMove.Moves.ToArray()[i], isUndo);
                }
                break;
        }
    }

    public void Reset()
    {
        var moves = new List<CellMove>();
        for (int x = 0; x < Board.width; x++)
        {
            for (int y = 0; y < Board.height; y++)
            {
                moves.Add(new CellMove(x, y, Board.cells[x, y].contents, CellContents.Nothing));
                Board.cells[x, y].contents = CellContents.Nothing;
            }
        }
        if (moves.Any())
        {
            _undoStack.Push(new MultiMove(moves));
            _redoStack.Clear();
            GameState = GameState.Playing;
        }
            
        UpdateHintSatisfaction();
        AutoCrossAll();
        CheckWin();
        SaveProgress();
    }
}