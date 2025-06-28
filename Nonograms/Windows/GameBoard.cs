using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Nonograms.Game;

namespace Nonograms.Windows;

public class GameBoard(NonogramsGame game, IFontAtlas fontAtlas, Configuration configuration)
{
    public NonogramsGame Game { get; set; } = game;
    private int _cellSizePx;
    private Vector2 _cellSizePxVec2;
    private Vector2 _boardBottomRight = Vector2.Zero;
    
    // For the MultiSelectionBrush
    private (int x, int y) _startCell; 
    private (int x, int y) _endCell;

    public void Draw()
    {
        // Check if config zoom has changed
        _cellSizePxVec2.X = _cellSizePxVec2.Y = _cellSizePx = (int) (40f * configuration.Zoom / 100f);
        var fontSize = configuration.Zoom switch
        {
            < 75 => GameFontFamilyAndSize.MiedingerMid12,
            < 99 => GameFontFamilyAndSize.MiedingerMid14,
            < 200 => GameFontFamilyAndSize.MiedingerMid18,
            200 => GameFontFamilyAndSize.MiedingerMid36,
            _ => throw new ArgumentOutOfRangeException(nameof(configuration.Zoom), "Unable to set font size for zoom")
        };
        var font = fontAtlas.NewGameFontHandle(new GameFontStyle(fontSize));
        font.Push();

        Vector2 currentCellCursorPos = Vector2.Zero;
        Vector2 tlCursorPos = Vector2.Zero;
        var drawList = ImGui.GetWindowDrawList();
        var tableWidth = Game.Board.longestRowConstraint + Game.Board.width;

        using var cellStyle = ImRaii.PushStyle(ImGuiStyleVar.CellPadding, Vector2.Zero);
        using var itemStyle = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

        // background
        drawList.AddRectFilledMultiColor(
            ImGui.GetWindowContentRegionMin(),
            _boardBottomRight,
            Colours.NavyBlue, Colours.Black, Colours.Black, Colours.Black);
        DrawHintBackgrounds(_boardBottomRight, drawList, Colours.LighterNavyBlueVariant);
        
        if (ImGui.BeginTable("nonogram", tableWidth + 1, ImGuiTableFlags.SizingFixedSame))
        {
            // draw column constraints
            for (int y = 0; y < Game.Board.longestColumnConstraint; y++)
            {
                // empty space in the top left corner
                for (int x = 0; x < Game.Board.longestRowConstraint; x++)
                {
                    ImGui.TableNextColumn();
                }

                for (int x = 0; x < Game.Board.width; x++)
                {
                    ImGui.TableNextColumn();
                    
                    var constraint = Game.Board.columnConstraints[x];
                    var pos = constraint.value.Length - Game.Board.longestColumnConstraint + y;
                    if (pos >= 0)
                    {
                        CenteredText(constraint.value[pos].ToString(), constraint.satisfied[pos]);
                    }
                }

                ImGui.TableNextColumn();
                ImGui.Dummy(_cellSizePxVec2); // dummy column for spacing purposes
                ImGui.TableNextRow();
            }

            for (int y = 0; y < Game.Board.height; y++)
            {
                // draw row constraints
                for (int x = 0; x < Game.Board.longestRowConstraint; x++)
                {
                    ImGui.TableNextColumn();
                    var constraint = Game.Board.rowConstraints[y];
                    
                    var pos = constraint.value.Length - Game.Board.longestRowConstraint + x;
                    if (pos >= 0)
                    {
                        CenteredText(constraint.value[pos].ToString(), constraint.satisfied[pos]);
                    }
                }

                // draw board
                for (int x = 0; x < Game.Board.width; x++)
                {
                    ImGui.TableNextColumn();
                    var currentCursorPos = ImGui.GetCursorPos();

                    if (x == 0 && y == 0) tlCursorPos = ImGui.GetCursorPos();
                    
                    DrawBoardCell(x, y, drawList);
                    ImGui.SetCursorPos(currentCursorPos);

                    if (MouseInSquare(ImGui.GetMousePos(), _cellSizePx))
                    {
                        currentCellCursorPos = GetWindowCursorPos();
                        var currentCell = (x, y);

                        // multi selection line
                        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            _endCell = _startCell = (x, y);
                        }
                        if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                        {
                            if (currentCell != _startCell)
                            {
                                var isHorizontal = Math.Abs(currentCell.x - _startCell.x) >=
                                                   Math.Abs(currentCell.y - _startCell.y);

                                if (isHorizontal) _endCell = _startCell with { x = currentCell.x };
                                else _endCell = _startCell with { y = currentCell.y };
                            }
                            else
                            {
                                _endCell = _startCell;
                            }
                        }
                        
                        // standard
                        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _startCell == _endCell)
                        {
                            Game.Click(x, y);
                        }
                        else if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            Game.RightClick(x, y);
                        }
                    }
                }

                ImGui.TableNextColumn();
                ImGui.Dummy(_cellSizePxVec2); // dummy column for spacing purposes
                _boardBottomRight = GetWindowCursorPos();
                ImGui.TableNextRow();
            }
            ImGui.EndTable();
        }

        DrawMultiSelectionBrush(drawList);
        DrawGridLines(_boardBottomRight, drawList);
        DrawHighlightCross(currentCellCursorPos, _boardBottomRight, drawList);
        ImGui.SetCursorPos(tlCursorPos);
        DrawAntiClickField(new Vector2(Game.Board.width, Game.Board.height) * _cellSizePx);
        font.Pop();
        DrawUndoRedoButtons();
        ImGui.SetWindowSize(_boardBottomRight
            - ImGui.GetWindowPos()
            + ImGui.GetWindowContentRegionMin()*Vector2.UnitX
            + Vector2.UnitY*35); // footer

    }

    // TODO Maybe extract to footer class
    private void DrawUndoRedoButtons()
    {
        var buttonPos = ImGui.GetWindowContentRegionMin() +
                        Vector2.UnitY * ((Game.Board.height + Game.Board.longestColumnConstraint) * _cellSizePx + 5);
        ImGui.SetCursorPos(buttonPos);
        
        ImGuiComponents.IconButton(FontAwesomeIcon.Undo);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Undo");
        }
        ImGui.SameLine();
        ImGuiComponents.IconButton(FontAwesomeIcon.Redo);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Redo");
        }
    }

    private Vector2 GetWindowCursorPos() => ImGui.GetCursorPos() + ImGui.GetWindowPos();

    private bool MouseInSquare(Vector2 mousePos, int squareSize)
    {
        var cursorPos = GetWindowCursorPos();
        
        return mousePos.X > cursorPos.X
               && mousePos.X <= cursorPos.X + squareSize
               && mousePos.Y > cursorPos.Y
               && mousePos.Y <= cursorPos.Y + squareSize;
    }

    private Vector2 LocateCellCentre((int x, int y) cell)
    {
        var root = ImGui.GetWindowPos() + ImGui.GetWindowContentRegionMin();
        return root + _cellSizePxVec2*0.5f
                    + new Vector2(
                        _cellSizePx * (cell.x + Game.Board.longestRowConstraint),
                        _cellSizePx * (cell.y + Game.Board.longestColumnConstraint));
    }

    private void CenteredText(string text, bool greyedOut = false)
    {
        var textSize = ImGui.CalcTextSize(text);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 0.5f*(_cellSizePx - textSize.X));
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 0.5f*(_cellSizePx - textSize.Y));
        if (greyedOut)
            ImGui.TextColored(new Vector4(0.2f, 0.2f, 0.2f, 1), text);
        else
            ImGui.TextColored(new Vector4(0.9f, 0.9f, 0.9f, 1), text);
    }

    private void DrawBoardCell(int x, int y, ImDrawListPtr drawList)
    {
        var cell = Game.Board.cells[x, y];
        var correctedCursor = GetWindowCursorPos();

        switch (cell.contents)
        {
            case CellContents.Filled:
                drawList.AddRectFilled(correctedCursor, correctedCursor + _cellSizePxVec2, Colours.Black);
                return;
            case CellContents.Nothing:
                drawList.AddRectFilled(correctedCursor, correctedCursor + _cellSizePxVec2, Colours.White);
                return;
            case CellContents.Cross:
                drawList.AddRectFilled(correctedCursor, correctedCursor + _cellSizePxVec2, Colours.White);
                var tl = correctedCursor + _cellSizePxVec2*0.25f;
                var br = correctedCursor + _cellSizePxVec2*0.75f;
                var tr = tl with { X = br.X };
                var bl = tl with { Y = br.Y };
                drawList.AddLine(tl, br, Colours.Black, _cellSizePx*0.1f);
                drawList.AddLine(tr, bl, Colours.Black, _cellSizePx*0.1f);
                return;
            case CellContents.Circle:
                drawList.AddRectFilled(correctedCursor, correctedCursor + _cellSizePxVec2, Colours.White);
                drawList.AddCircleFilled(correctedCursor + _cellSizePxVec2*0.5f, _cellSizePx*0.15f, Colours.Black);
                return;
        }
    }

    private void DrawHintBackgrounds(Vector2 boardBottomRight, ImDrawListPtr drawList, uint colour)
    {
        var windowPos = ImGui.GetWindowPos() + ImGui.GetWindowContentRegionMin();
        
        for (int x = 0; x < Game.Board.width; x++)
        {
            var columnHintsBottom = windowPos.Y + _cellSizePx * Game.Board.longestColumnConstraint;
            if (x % 10 == 0)
            {
                var rectWidthCells = Math.Min(5, Game.Board.width - x);
                var tl = new Vector2(windowPos.X + (x+Game.Board.longestRowConstraint) * _cellSizePx, windowPos.Y);
                var br = new Vector2(windowPos.X + (x+rectWidthCells+Game.Board.longestRowConstraint) * _cellSizePx , columnHintsBottom);
                drawList.AddRectFilledMultiColor(tl, br, Colours.Transparency, Colours.Transparency, colour, colour);
            }
        }
        
        for (int y = 0; y < Game.Board.height; y++)
        {
            var rowHintsRight = windowPos.X + _cellSizePx * Game.Board.longestRowConstraint;
            if (y % 10 == 0)
            {
                var rectHeightCells = Math.Min(5, Game.Board.height - y);
                var tl = new Vector2(windowPos.X, windowPos.Y + (y+Game.Board.longestColumnConstraint) * _cellSizePx);
                var br = new Vector2(rowHintsRight, windowPos.Y + (y+rectHeightCells+Game.Board.longestColumnConstraint) * _cellSizePx);
                drawList.AddRectFilledMultiColor(tl, br, Colours.Transparency, colour, colour, Colours.Transparency);
            }
        }
    }

    private void DrawGridLines(Vector2 boardBottomRight, ImDrawListPtr drawList)
    {
        // if (Game.GameState is not GameState.Playing) return;
        var colour = Colours.SkyBlue;
        var windowPos = ImGui.GetWindowPos() + ImGui.GetWindowContentRegionMin();
        
        for (int x = 0; x < Game.Board.width; x++)
        {
            if (x % 5 == 0)
            {
                var tl = new Vector2(windowPos.X + (x+Game.Board.longestRowConstraint) * _cellSizePx, windowPos.Y);
                var br = new Vector2(windowPos.X + (x+Game.Board.longestRowConstraint) * _cellSizePx + 1, boardBottomRight.Y);
                drawList.AddRectFilled(tl, br, colour);
            }

            if (x == Game.Board.width - 1)
            {
                var tl = new Vector2(windowPos.X + (x+Game.Board.longestRowConstraint+1) * _cellSizePx, windowPos.Y);
                var br = new Vector2(windowPos.X + (x+Game.Board.longestRowConstraint+1) * _cellSizePx + 1, boardBottomRight.Y);
                drawList.AddRectFilled(tl, br, colour);
            }
        }

        for (int y = 0; y < Game.Board.height; y++)
        {
            if (y % 5 == 0)
            {
                var tl = new Vector2(windowPos.X, windowPos.Y + (y+Game.Board.longestColumnConstraint)*_cellSizePx);
                var br = new Vector2(boardBottomRight.X, windowPos.Y + (y+Game.Board.longestColumnConstraint)*_cellSizePx + 1);
                drawList.AddRectFilled(tl, br, colour);
            }

            if (y == Game.Board.height - 1)
            {
                var tl = new Vector2(windowPos.X, windowPos.Y + (y+Game.Board.longestColumnConstraint+1)*_cellSizePx);
                var br = new Vector2(boardBottomRight.X, windowPos.Y + (y+Game.Board.longestColumnConstraint+1)*_cellSizePx + 1);
                drawList.AddRectFilled(tl, br, colour);
            }
        }
    }

    private void DrawHighlightCross(Vector2 cursorPos, Vector2 boardBottomRight, ImDrawListPtr drawList)
    {
        if (Game.GameState is not GameState.Playing) return;
        if (cursorPos == Vector2.Zero) return;
        
        var windowPos = ImGui.GetWindowPos();
        var tl = new Vector2(windowPos.X, cursorPos.Y);
        var br = new Vector2(boardBottomRight.X, cursorPos.Y + _cellSizePx);
        drawList.AddRectFilled(tl, br, Colours.OrangeHighlight);

        tl = new Vector2(cursorPos.X, windowPos.Y);
        br = new Vector2(cursorPos.X + _cellSizePx, boardBottomRight.Y);
        drawList.AddRectFilled(tl, br, Colours.OrangeHighlight);
    }

    private void DrawMultiSelectionBrush(ImDrawListPtr drawList)
    {
        if (Game.GameState is not GameState.Playing) return;
        if (_startCell == _endCell) return;

        var startIsFilled = Game.Board.cells[_startCell.x, _startCell.y].contents == CellContents.Filled;
        var colour = startIsFilled ? Colours.White : Colours.Black;
        
        if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            var start = LocateCellCentre(_startCell);
            var end = LocateCellCentre(_endCell);

            if (_startCell.x < _endCell.x) // Going right
            {
                start.X -= _cellSizePx * 0.5f;
                end.X += _cellSizePx * 0.5f;
            }
            else if (_startCell.x > _endCell.x) // Going left
            {
                start.X += _cellSizePx * 0.5f;
                end.X -= _cellSizePx * 0.5f;
            }
            else if (_startCell.y < _endCell.y) // Going down
            {
                start.Y -= _cellSizePx * 0.5f;
                end.Y += _cellSizePx * 0.5f;
            }
            else if (_startCell.y > _endCell.y) // Going up
            {
                start.Y += _cellSizePx * 0.5f;
                end.Y -= _cellSizePx * 0.5f;
            }
            
            drawList.AddLine(start, end, colour, _cellSizePx*0.4f);
        }

        if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _startCell != _endCell)
        {
            var fill = Game.Board.cells[_startCell.x, _startCell.y].contents is not CellContents.Filled;
            
            var cells = new HashSet<(int x, int y)>();
            
            for (int x = Math.Min(_startCell.x, _endCell.x); x <= Math.Max(_startCell.x, _endCell.x); x++)
            for (int y = Math.Min(_startCell.y, _endCell.y); y <= Math.Max(_startCell.y, _endCell.y); y++)
            {
                cells.Add((x, y));
            }
            
            if (fill) Game.Fill(cells);
            else Game.Clear(cells);

            _startCell = _endCell;
        }
    }
    
    private void DrawAntiClickField(Vector2 size)
    {
        // Cover the board in an anticlick field to stop window movement
        var cursorPos = ImGui.GetCursorPos();
        ImGui.InvisibleButton("anticlick", size);
        ImGui.SetCursorPos(cursorPos);
    }
}