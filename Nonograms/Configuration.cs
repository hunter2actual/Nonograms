using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace Nonograms;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public int Zoom { get; set; } = 100;

    public string? CurrentPuzzleStateKey { get; set; }
    public Dictionary<string, PuzzleState> AllPuzzlesState { get; set; } = new();
    
    [NonSerialized]
    private IDalamudPluginInterface? _pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }

    public void Save()
    {
        _pluginInterface!.SavePluginConfig(this);
    }
}

public class PuzzleState
{
    public string Pack { get; set; }
    public string Title { get; set; }
    public string PackTitleKey => Pack + Title;
    public int[,]? BoardState { get; set; }
    public bool Completed { get; set; }
    public string[] UndoStack { get; set; }
    public string[] RedoStack { get; set; }
}