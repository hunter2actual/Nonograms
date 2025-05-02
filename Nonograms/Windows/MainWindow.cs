using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private readonly PuzzleStateManager _puzzleStateManager;
    private Dictionary<string, string> _files = new();
    private int _selectedPuzzlePack;
    private List<LoadedPuzzle> _loadedPuzzlePack = new();
    private LoadedPuzzle? _selectedPuzzle;

    public MainWindow(Plugin plugin, IFontAtlas fontAtlas, Configuration configuration, PuzzleLoader puzzleLoader)
        : base("Nonograms", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize)
    {
        _fontAtlas = fontAtlas;
        _configuration = configuration;
        _puzzleLoader = puzzleLoader;
        _puzzleStateManager = new PuzzleStateManager(configuration);
        _drawConfig = plugin.DrawConfigUI;
        _files = _puzzleLoader.GetFileNames();
        _loadedPuzzlePack = _puzzleLoader.GetPuzzles(_files.Keys.ToArray()[0]);
        
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
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
            _puzzleStateManager.InitialisePack(puzzlePackNames[_selectedPuzzlePack], _loadedPuzzlePack);
        }
        
        // Select a puzzle
        var buttonSize = new Vector2(100, 100);
        ImGui.SetWindowSize(buttonSize * new Vector2(6, 2 + _loadedPuzzlePack.Count / 5f));
        if (_loadedPuzzlePack.Count > 0)
        {
            for (var i = 0; i < _loadedPuzzlePack.Count; i++)
            {
                var savedState = _puzzleStateManager.GetPuzzleState(puzzlePackNames[_selectedPuzzlePack], _loadedPuzzlePack[i].title);

                string label;
                if (savedState.Completed)
                {
                    label = savedState.Title;
                    StyleButtonPositive();
                }
                else
                {
                    label = $"Puzzle {i + 1}";
                }

                if (ImGui.Button(label, buttonSize))
                {
                    _selectedPuzzle = _loadedPuzzlePack[i];
                    var game = new NonogramsGame(_puzzleStateManager, _selectedPuzzle);

                    _puzzleStateManager.SetCurrentPuzzleState(savedState);
                    game.InitialiseFromState(savedState);
                    _gameBoard = new GameBoard(game, _fontAtlas, _configuration);
                }

                if (savedState.Completed)
                {
                    ClearButtonStyle();
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
        if (_gameBoard!.Game.GameState is GameState.Victorious)
            ImGui.TextColored(new Vector4(1f, 1f, 1f, 1f), _selectedPuzzle!.title);
    }

    private void StyleButtonPositive()
    {
        ImGui.PushStyleColor(ImGuiCol.Button, Colours.Green);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Colours.GreenHighlight);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, Colours.GreenActive);
    }

    private void ClearButtonStyle()
    {
        ImGui.PopStyleColor(3);
    }

    public void Dispose() { }
}
