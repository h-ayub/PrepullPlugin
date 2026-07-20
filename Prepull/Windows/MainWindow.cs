using System;
using System.Numerics;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Dalamud.Bindings.ImGui;
using System.Runtime.Versioning;
using Prepull.Classes.Enums;
using Prepull.Classes.Services;

namespace Prepull.Windows;

[SupportedOSPlatform("windows")]
public class MainWindow : Window, IDisposable
{
    private readonly PrepullPlugin Plugin;
    private readonly unsafe PlayerState* PlayerStatePtr = PlayerState.Instance();
    private readonly BaseService baseService = new BaseService();

    public MainWindow(PrepullPlugin plugin)
        : base(PrepullStrings.MainWindowTitle, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
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
        if (!PrepullPluginServices.Condition[ConditionFlag.BoundByDuty])
        {
            ImGui.Text(PrepullStrings.NotInInstance);
            return;
        }

        ushort territoryId = (ushort)PrepullPluginServices.ClientState.TerritoryType;   // get territory id

        // who cares if this is normal content...
        if (baseService.IsNormalContent(territoryId))
            return;
        
        if (!PrepullSystem.Configuration.TerritoryConditions.ContainsKey(territoryId)) // check if we do not have data on this territory
        {
            PrepullSystem.Configuration.TerritoryConditions[territoryId] = new TerritoryConfig(PrepullSystem.Configuration.DefaultMainTank, PrepullSystem.Configuration.FoodBuffRefreshTime);
        }

        var jobId = (FfxivJob)PlayerStatePtr->CurrentClassJobId;
        ImGui.Text(ReturnTerritoryName(territoryId));
        ImGui.Spacing();

        if (jobId == FfxivJob.Paladin || jobId == FfxivJob.Warrior || jobId == FfxivJob.DarkKnight || jobId == FfxivJob.Gunbreaker)   // tanks: warrior, paladin, dark knight, gunbreaker
        {
            DrawTankUi(jobId, territoryId);
        } else if (jobId == FfxivJob.Scholar || jobId == FfxivJob.Summoner)  // scholar, summoner
        {
            DrawPetSummonUi(jobId, territoryId);
        }

        DrawFoodBuffRefreshTimeUi(territoryId);
        DrawDesignatedDancePartnerUi(territoryId);
    }

    private void DrawTankUi(FfxivJob jobId, ushort territoryId)
    {
        var config = PrepullSystem.Configuration.TerritoryConditions[territoryId];
        var isMainTank = jobId switch
        {
            FfxivJob.Paladin => config.IsPldMainTank,
            FfxivJob.Warrior => config.IsWarMainTank,
            FfxivJob.DarkKnight => config.IsDrkMainTank,
            FfxivJob.Gunbreaker => config.IsGnbMainTank,
        };
        if (ImGui.Checkbox(PrepullStrings.ToggleMainTank, ref isMainTank))
        {
            switch (jobId)
            {
                case FfxivJob.Warrior:
                    config.IsWarMainTank = isMainTank;
                    break;
                case FfxivJob.Paladin:
                    config.IsPldMainTank = isMainTank;
                    break;
                case FfxivJob.DarkKnight:
                    config.IsDrkMainTank = isMainTank;
                    break;
                case FfxivJob.Gunbreaker:
                    config.IsGnbMainTank = isMainTank;
                    break;
            }
            PrepullSystem.Configuration.Save();
        }
    }

    private void DrawPetSummonUi(FfxivJob jobId, ushort territoryId)
    {
        var config = PrepullSystem.Configuration.TerritoryConditions[territoryId];
        var summonPet = jobId switch
        {
            FfxivJob.Scholar => config.IsSchSummonPet,
            FfxivJob.Summoner => config.IsSmnSummonPet
        };
        if (ImGui.Checkbox(PrepullStrings.SummonPet, ref summonPet))
        {
            switch (jobId)
            {
                case FfxivJob.Scholar:
                    config.IsSchSummonPet = summonPet;
                    break;
                case FfxivJob.Summoner:
                    config.IsSmnSummonPet = summonPet;
                    break;
            }
            PrepullSystem.Configuration.Save();
        }
    }

    private void DrawFoodBuffRefreshTimeUi(ushort territoryId)
    {
        var foodRefreshTime = PrepullSystem.Configuration.TerritoryConditions[territoryId].FoodBuffRefreshTime / 60;
        ImGui.SetNextItemWidth(100f * ImGuiHelpers.GlobalScale);
        if (ImGui.InputInt(PrepullStrings.RefreshFoodTimer, ref foodRefreshTime, 1))
        {
            if (foodRefreshTime < 1)
                foodRefreshTime = 1;
            if (foodRefreshTime > 20)
                foodRefreshTime = 20;
            PrepullSystem.Configuration.TerritoryConditions[territoryId].FoodBuffRefreshTime = foodRefreshTime * 60;
            PrepullSystem.Configuration.Save();
        }
    }

    private void DrawDesignatedDancePartnerUi(ushort territoryId)
    {
        var designatedDancePartner = PrepullSystem.Configuration.TerritoryConditions[territoryId].DesignatedDancePartner;
        ImGui.SetNextItemWidth(200f * ImGuiHelpers.GlobalScale);
        if (ImGui.InputText(PrepullStrings.DancePartner, ref designatedDancePartner, 50))
        {
            PrepullSystem.Configuration.TerritoryConditions[territoryId].DesignatedDancePartner = designatedDancePartner;
            PrepullSystem.Configuration.Save();
        }
    }

    private unsafe string ReturnTerritoryName(uint territoryId)
    {
        return PrepullSystem.TerritoryNames[territoryId].Item1;
    }
}
