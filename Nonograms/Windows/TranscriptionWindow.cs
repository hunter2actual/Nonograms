using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;

namespace Nonograms.Windows;

public class TranscriptionWindow : Window, IDisposable
{
    private int _inputWidth;
    private int _inputHeight;
    private int _height;
    private int _width;
    private readonly Vector2 _cellSizePxVec2 = new(40, 40);
    private Vector2 _tlCursorPos = Vector2.Zero;
    private bool[,] _board;
    
    public TranscriptionWindow() : base("Transcription")
    {
        _board = new bool[10,10];
        _inputHeight = 10;
        _inputWidth = 10;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw()
    {
        ImGui.InputInt("Width", ref _inputWidth);
        ImGui.InputInt("Height", ref _inputHeight);
        if (ImGui.Button("Clear"))
        {
            _board = new bool[_width, _height];
        }
        ImGui.SameLine();
        if (ImGui.Button("Fill"))
        {
            _board = new bool[_width, _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _board[x, y] = true;
                }
            }
        }

        if (_inputHeight != _height || _inputWidth != _width)
        {
            _height = _inputHeight;
            _width = _inputWidth;
            _board = new bool[_width, _height];
        }
        
        DrawGrid();
        
        ImGui.NewLine();
        if (ImGui.Button("Copy to clipboard"))
        {
            ImGui.SetClipboardText(BoardToString());   
        }
        
        ImGui.SetCursorPos(_tlCursorPos);
        DrawAntiClickField(new Vector2(_width, _height) * _cellSizePxVec2.X * 1.2f);
    }

    private void DrawGrid()
    {
        if (ImGui.BeginTable("transcribe", _width + 1, ImGuiTableFlags.SizingFixedSame))
        {
            var drawList = ImGui.GetWindowDrawList();
            for (int y = 0; y < _height; y++)
            {
                ImGui.TableNextRow();
                for (int x = 0; x < _width; x++)
                {
                    ImGui.TableNextColumn();
                    if (x == 0 && y == 0) _tlCursorPos = ImGui.GetCursorPos();
                    DrawBoardCell(x, y, drawList);

                    if (MouseInSquare(ImGui.GetMousePos(), (int)_cellSizePxVec2.X))
                    {
                        if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                        {
                            _board[x, y] = true;
                        }
                        else if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
                        {
                            _board[x, y] = false;
                        }
                    }
                }
                ImGui.Dummy(_cellSizePxVec2); // dummy column for spacing purposes
            }
        }
        ImGui.EndTable();
    }

    private void DrawBoardCell(int x, int y, ImDrawListPtr drawList)
    {
        var cell = _board[x, y];
        var correctedCursor = GetWindowCursorPos();

        bool evenBlockX = x / 5 % 2 == 0;
        bool evenBlockY = y / 5 % 2 == 0;

        var colour = cell 
            ? Colours.SkyBlue 
            : evenBlockX ^ evenBlockY ? Colours.MidGrey : Colours.White;
        drawList.AddRectFilled(correctedCursor, correctedCursor + _cellSizePxVec2, colour);
    }
    
    private bool MouseInSquare(Vector2 mousePos, int squareSize)
    {
        var cursorPos = GetWindowCursorPos();
        
        return mousePos.X > cursorPos.X
               && mousePos.X <= cursorPos.X + squareSize
               && mousePos.Y > cursorPos.Y
               && mousePos.Y <= cursorPos.Y + squareSize;
    }

    private Vector2 GetWindowCursorPos() => ImGui.GetCursorPos() + ImGui.GetWindowPos();
    
    private void DrawAntiClickField(Vector2 size)
    {
        // Cover the board in an anticlick field to stop window movement
        var cursorPos = ImGui.GetCursorPos();
        ImGui.InvisibleButton("anticlick", size);
        ImGui.SetCursorPos(cursorPos);
    }

    private string BoardToString()
    {
        var text = "";
        
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var cell = _board[x, y];
                text += cell ? '#' : ' ';
            }

            text += "\n";
        }

        return text;
    }

    public void Dispose() { }
}