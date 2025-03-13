using System;
using System.Linq.Expressions;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Nonograms.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration _configuration;

    public ConfigWindow(Plugin plugin) : base(
        "Nonograms Settings",
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(395, 460);
        SizeCondition = ImGuiCond.Appearing;

        _configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        int zoom = _configuration.Zoom;
        ImGui.Text("Zoom");
        ImGui.RadioButton("tiny", ref zoom, 50); ImGui.SameLine();
        ImGui.RadioButton("small", ref zoom, 75); ImGui.SameLine();
        ImGui.RadioButton("normal", ref zoom, 100); ImGui.SameLine();
        ImGui.RadioButton("large", ref zoom, 130); ImGui.SameLine();
        ImGui.RadioButton("huge", ref zoom, 200);
        _configuration.Zoom = zoom;
        
        if (ImGui.Button("Save and Close"))
        {
            _configuration.Save();
            IsOpen = false;
        }
    }
}
