using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Nonograms.Game;
using ImGuiNET;
using Nonograms.FileSystem;

namespace Nonograms.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Action _drawConfig;
    private GameBoard? _gameBoard;
    private readonly IFontAtlas _fontAtlas;
    private readonly Configuration _configuration;
    private readonly PuzzleLoader _puzzleLoader;
    private Dictionary<string, string> _files = new();
    private int _selectedPuzzlePack;
    private List<LoadedPuzzle> _loadedPuzzlePack = new();
    private LoadedPuzzle? _selectedPuzzle;
    private int _selectedPuzzleIndex;
    
    public MainWindow(Plugin plugin, IFontAtlas fontAtlas, Configuration configuration, PuzzleLoader puzzleLoader)
        : base("Nonograms", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize)
    {
        _fontAtlas = fontAtlas;
        _configuration = configuration;
        _puzzleLoader = puzzleLoader;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        // _gameBoard = new GameBoard(new NonogramsGame(), fontAtlas, configuration);
        _drawConfig = plugin.DrawConfigUI;
        
        _files = _puzzleLoader.GetFileNames();
        _loadedPuzzlePack = _puzzleLoader.GetPuzzles(_files.Keys.ToArray()[0]);
    }

    public override void Draw()
    {
        if (_selectedPuzzle is not null)
        {
            DrawPuzzle();
            return;
        }
        
        // Select a puzzle pack
        var fullFileNames = _files.Keys.ToArray();
        var puzzlePackNames = _files.Values.ToArray();
        if (ImGui.Combo("Puzzle pack", ref _selectedPuzzlePack, puzzlePackNames.ToArray(), puzzlePackNames.Length))
        {
            _loadedPuzzlePack = _puzzleLoader.GetPuzzles(fullFileNames[_selectedPuzzlePack]);
        }
        
        // Select a puzzle
        var buttonSize = new Vector2(50, 50);
        ImGui.SetWindowSize(buttonSize * new Vector2(6, 2 + _loadedPuzzlePack.Count / 5f));
        if (_loadedPuzzlePack.Count > 0)
        {
            for (var i = 0; i < _loadedPuzzlePack.Count; i++)
            {
                if (ImGui.Button((i+1).ToString(), buttonSize))
                {
                    _selectedPuzzle = _loadedPuzzlePack[i];
                    _gameBoard = new GameBoard(new NonogramsGame(_selectedPuzzle), _fontAtlas, _configuration);
                }
                if ((i + 1) % 5 > 0) ImGui.SameLine();
            }
        }
    }

    private void DrawPuzzle()
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);

        // Draw nonogram
        _gameBoard?.Draw();
        
        // Draw a back button
        ImGui.SetCursorPos(ImGui.GetWindowContentRegionMin());
        if (ImGui.ArrowButton("back", ImGuiDir.Left))
        {
            _selectedPuzzle = null;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Back");
        }
        
        // Draw settings button
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
        {
            _drawConfig();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Settings");
        }
        
        // Draw puzzle name
        if (_gameBoard.Game.GameState is GameState.Victorious)
            ImGui.TextColored(new Vector4(1f, 1f, 1f, 1f), _selectedPuzzle.title);
    }

    public void Dispose() { }
}
