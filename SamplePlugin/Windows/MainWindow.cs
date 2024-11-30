using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private string GoatImagePath;
    private Plugin Plugin;
    private unsafe PlayerState* playerStatePtr = PlayerState.Instance();

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath)
        : base(strings.MainWindowTitle, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        GoatImagePath = goatImagePath;
        Plugin = plugin;
    }

    public void Dispose() { }

    public unsafe override void Draw()
    {
        if (playerStatePtr->CurrentClassJobRow == 2529515684192)
        {
            var isMainTank = Plugin.Configuration.IsMainTank;
            if (ImGui.Checkbox(strings.ToggleMainTank, ref isMainTank))
            {
                Plugin.Configuration.IsMainTank = isMainTank;
                Plugin.Configuration.Save();
            }
        }

        ImGui.Spacing();
        ImGui.Text(ReturnPlayerName());
    }

    private unsafe string ReturnPlayerName()
    {
        return playerStatePtr->CurrentClassJobRow.ToString();
    }
}
