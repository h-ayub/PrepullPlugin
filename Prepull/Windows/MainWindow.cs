using System;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;

namespace Prepull.Windows;

public class MainWindow : Window, IDisposable
{
    private Prepull Plugin;
    private unsafe PlayerState* playerStatePtr = PlayerState.Instance();

    public MainWindow(Prepull plugin)
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
        if (!Prepull.Condition[ConditionFlag.BoundByDuty])
        {
            ImGui.Text(strings.NotInInstance);
            return;
        }

        var territoryId = Prepull.ClientState.TerritoryType;   // get territory id
        
        if (!Plugin.Configuration.TerritoryConditions.ContainsKey(territoryId)) // check if we do not have data on this territory
        {
            Plugin.Configuration.TerritoryConditions[territoryId] = new Configuration.TerritoryConfig(Plugin.Configuration.DefaultMainTank);
        }

        var jobId = playerStatePtr->CurrentClassJobId;
        ImGui.Text(ReturnTerritoryName(territoryId));
        ImGui.Spacing();

        if (jobId == 19 || jobId == 21 || jobId == 32 || jobId == 37)   // tanks: warrior, paladin, dark knight, gunbreaker
        {
            var config = Plugin.Configuration.TerritoryConditions[territoryId];
            var isMainTank = jobId switch
            {
                19 => config.IsWarMainTank,
                21 => config.IsPldMainTank,
                32 => config.IsDrkMainTank,
                37 => config.IsGnbMainTank
            };
            if (ImGui.Checkbox(strings.ToggleMainTank, ref isMainTank))
            {
                switch (jobId)
                {
                    case 19:
                        config.IsWarMainTank = isMainTank;
                        break;
                    case 21:
                        config.IsPldMainTank = isMainTank;
                        break;
                    case 32:
                        config.IsDrkMainTank = isMainTank;
                        break;
                    case 37:
                        config.IsGnbMainTank = isMainTank;
                        break;
                }
                Plugin.Configuration.Save();
            }
        } else if (jobId == 28 || jobId == 27)  // scholar, summoner
        {
            var config = Plugin.Configuration.TerritoryConditions[territoryId];
            var summonPet = jobId switch {
                27 => config.IsSchSummonPet,
                28 => config.IsSmnSummonPet
            };
            if (ImGui.Checkbox(strings.SummonPet, ref summonPet))
            {
                switch (jobId)
                {
                    case 27:
                        config.IsSchSummonPet = summonPet;
                        break;
                    case 28:
                        config.IsSmnSummonPet = summonPet;
                        break;
                }
                Plugin.Configuration.Save();
            }
        }
    }

    private unsafe string ReturnTerritoryName(uint territoryId)
    {
        return Plugin.TerritoryNames[territoryId].Item1;
    }
}
