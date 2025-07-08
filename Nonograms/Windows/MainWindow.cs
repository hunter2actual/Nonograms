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
    private int? _selectedPuzzleContextMenu = null;

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
        if (_selectedPuzzle is null)
            DrawPuzzleSelection();
        else
            DrawPuzzle();
    }

    private void DrawPuzzleSelection()
    {
        // Settings button
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Cog))
        {
            _drawConfig();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Settings");
        }
        ImGui.SameLine();
        
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

                DrawPuzzleSelectionButton(savedState, i, buttonSize);
                DrawPuzzleSelectionContextMenu(i, puzzlePackNames);
                
                if ((i + 1) % 5 > 0) ImGui.SameLine();
            }
        }
    }

    private void DrawPuzzleSelectionButton(PuzzleState? savedState, int index, Vector2 buttonSize)
    {
        string label;
        if (savedState.Completed)
        {
            label = savedState.Title;
            StyleButtonPositive();
        }
        else
        {
            label = $"Puzzle {index + 1}";
        }
        if (ImGui.Button(label, buttonSize))
        {
            _selectedPuzzle = _loadedPuzzlePack[index];
            var game = new NonogramsGame(_puzzleStateManager, _selectedPuzzle);

            _puzzleStateManager.SetCurrentPuzzleState(savedState);
            game.InitialiseFromState(savedState);
            _gameBoard = new GameBoard(game, _fontAtlas, _configuration);
        }
        if (savedState.Completed)
        {
            ClearButtonStyle();
        }
    }

    private void DrawPuzzleSelectionContextMenu(int index, string[] puzzlePackNames)
    {
        if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup($"Puzzlecontext{index}");
            _selectedPuzzleContextMenu = null;
        }
        if (ImGui.BeginPopup($"Puzzlecontext{index}"))
        {
            if (_selectedPuzzleContextMenu == index)
            {
                ImGui.Text("Are you sure?");
                if (ImGui.Button("Reset puzzle"))
                {
                    _selectedPuzzleContextMenu = null;
                    ImGui.CloseCurrentPopup();
                    _puzzleStateManager.ResetPuzzle(puzzlePackNames[_selectedPuzzlePack], _loadedPuzzlePack, index);
                }
            }
            else
            {
                if (ImGui.Button("Reset puzzle"))
                    _selectedPuzzleContextMenu = index;
            }
            ImGui.EndPopup();
        }
    }

    private void DrawPuzzle()
    {
        var puzzleTitle = _selectedPuzzle!.title;
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

        // Draw puzzle name
        if (_gameBoard!.Game.GameState is GameState.Victorious)
            ImGui.TextColored(new Vector4(1f, 1f, 1f, 1f), puzzleTitle);
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
