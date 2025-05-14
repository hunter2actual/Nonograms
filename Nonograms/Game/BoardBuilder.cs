using System;
using System.Collections.Generic;
using System.Linq;

namespace Nonograms.Game;

public class BoardBuilder()
{
    private bool[,] _data;
    private int _width;
    private int _height;
    
    public Board Build()
    {
        var board = new Board {
            width = _width,
            height = _height,
            cells = new Cell[_width, _height]
        };

        board = PopulateCells(board);
        board = PopulateConstraints(board);
        return board;
    }

    public void WithData(bool[,] data)
    {
        _width = data.GetLength(0);
        _height = data.GetLength(1);
        _data = data;
    }

    private Board PopulateCells(Board board)
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                board.cells[x, y] = new Cell
                {
                    isSolid = _data[x, y],
                    contents = CellContents.Nothing
                };
            }
        }

        return board;
    }

    private Board PopulateConstraints(Board board)
    {
        board.columnConstraints = CreateConstraints(board.width, board.height, (x, y) => board.cells[x, y]);
        board.longestColumnConstraint = LongestConstraint(board.columnConstraints);
        board.rowConstraints = CreateConstraints(board.height, board.width, (y, x) => board.cells[x, y]);
        board.longestRowConstraint = LongestConstraint(board.rowConstraints);
        return board;
    }

    private List<Constraint> CreateConstraints(int dimension1, int dimension2, Func<int, int, Cell> cellGetter)
    {
        List<int[]> constraints = new List<int[]>();
        for (int i = 0; i < dimension1; i++)
        {
            var constraint = new List<int>();
            int run = 0;
            for (int j = 0; j < dimension2; j++)
            {
                if (cellGetter(i, j).isSolid)
                {
                    run++;
                }
                else if (run > 0)
                {
                    constraint.Add(run);
                    run = 0;
                }
            }

            if (run > 0)
            {
                constraint.Add(run);
            }

            if (constraint.Count == 0)
            {
                constraint.Add(0);
            }
            constraints.Add(constraint.ToArray());
        }

        return constraints.Select(x => new Constraint { value = x, satisfied = [] }).ToList();
    }

    private int LongestConstraint(List<Constraint> constraints)
    {
        return constraints.MaxBy(x => x.value.Length)!.value.Length;
    }
}