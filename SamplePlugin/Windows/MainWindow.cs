using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;

namespace SamplePlugin.Windows;

public class MainWindow : Window, IDisposable
{
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

        Plugin = plugin;
    }

    public void Dispose() { }

    public unsafe override void Draw()
    {
        // need to only draw if we are in an instance
        if (!Plugin.Condition[ConditionFlag.BoundByDuty])
        {
            ImGui.Text(strings.NotInInstance);
            return;
        }

        var territoryId = Plugin.ClientState.TerritoryType;   // get territory id
        // check if we do not have data on this territory
        if (!Plugin.Configuration.TerritoryConditions.ContainsKey(territoryId))
        {
            Plugin.Configuration.TerritoryConditions[territoryId] = new Configuration.TerritoryConfig();
        }

        var jobId = playerStatePtr->CurrentClassJobId;  // TODO: figure out a way to save settings per job
        ImGui.Text(ReturnTerritoryName(territoryId));
        ImGui.Spacing();

        if (jobId == 19 || jobId == 21 || jobId == 32 || jobId == 37)   // tanks: warrior, paladin, dark knight, gunbreaker
        {
            var isMainTank = Plugin.Configuration.TerritoryConditions[territoryId].IsMainTank;
            if (ImGui.Checkbox(strings.ToggleMainTank, ref isMainTank))
            {
                Plugin.Configuration.TerritoryConditions[territoryId].IsMainTank = isMainTank;
                Plugin.Configuration.Save();
            }
        } else if (jobId == 28 || jobId == 27)  // scholar, summoner
        {
            var summonPet = Plugin.Configuration.TerritoryConditions[territoryId].SummonPet;
            if (ImGui.Checkbox(strings.SummonPet, ref summonPet))
            {
                Plugin.Configuration.TerritoryConditions[territoryId].SummonPet = summonPet;
                Plugin.Configuration.Save();
            }
        }
    }

    private unsafe string ReturnTerritoryName(uint territoryId)
    {
        return Plugin.TerritoryNames[territoryId];
    }
}
