using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Prepull.Windows;
using System.Collections.Generic;
using Lumina.Excel.Sheets;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using KamiLib.Extensions;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.SteamApi.Callbacks;
using System;
using System.Runtime.Versioning;

namespace Prepull;

[SupportedOSPlatform("windows")]
public sealed class Prepull : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static ICondition Condition { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IDutyState DutyState { get; private set; } = null!;
    [PluginService] internal static IBuddyList BuddyList { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    private const string OpenMainWindow = "/ppp";
    private const string OpenConfigWindow = "/ppc";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Prepull");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    internal Dictionary<uint, (string, DutyType)> TerritoryNames = [];

    public Prepull()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(OpenMainWindow, new CommandInfo(OnMainUICommand)
        {
            HelpMessage = strings.HelpMessageMainWindow
        });

        CommandManager.AddHandler(OpenConfigWindow, new CommandInfo(OnConfigUICommand)
        {
            HelpMessage = strings.HelpMessageConfigWindow
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // This event is triggered when the player starts a duty
        DutyState.DutyStarted += ActivatePrepull;
        DutyState.DutyRecommenced += ActivatePrepull;

        // This fetches the territory names from excel sheet in dalamud repository
        this.TerritoryNames = DataManager.GetExcelSheet<TerritoryType>().Where(x => x.PlaceName.ValueNullable?.Name.ToString().Length > 0)
            .ToDictionary(x => x.RowId,
                x => ($"{x.PlaceName.ValueNullable?.Name} {(x.ContentFinderCondition.ValueNullable?.Name.ToString().Length > 0 ? $" ({x.ContentFinderCondition.ValueNullable?.Name})" : string.Empty)}",
                        DataManager.GetDutyType(x.ContentFinderCondition.Value)));
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        DutyState.DutyStarted -= ActivatePrepull;
        DutyState.DutyRecommenced -= ActivatePrepull;

        CommandManager.RemoveHandler(OpenMainWindow);
    }

    private void OnMainUICommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void OnConfigUICommand(string command, string args)
    {
        ToggleConfigUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    private unsafe void ActivatePrepull(object? sender, ushort e)
    {   
        var am = ActionManager.Instance();
        var playerStatePtr = PlayerState.Instance();
        var territoryId = ClientState.TerritoryType;
        var jobId = playerStatePtr->CurrentClassJobId;

        ExecuteTankProtocol(jobId, am, territoryId);
        ExecutePetProtocol(jobId, am, territoryId);
        CheckRemainingFoodBuff(territoryId);
        CheckGear(territoryId);
    }

    private bool IsMainTank(byte jobId, ushort territoryId)
    {
        if (!Configuration.TerritoryConditions.TryGetValue(territoryId, out var value))
        {
            value = new Configuration.TerritoryConfig(Configuration.DefaultMainTank, Configuration.FoodBuffRefreshTime);
            Configuration.TerritoryConditions[territoryId] = value;
        }
        return jobId switch
        {
            19 => value.IsWarMainTank,
            21 => value.IsPldMainTank,
            32 => value.IsDrkMainTank,
            37 => value.IsGnbMainTank,
            _ => false,
        };
    }

    private bool IsSummonPet(byte jobId, ushort territoryId)
    {
        if (!Configuration.TerritoryConditions.TryGetValue(territoryId, out var value))
        {
            value = new Configuration.TerritoryConfig(Configuration.DefaultMainTank, Configuration.FoodBuffRefreshTime);
            Configuration.TerritoryConditions[territoryId] = value;
        }
        return jobId switch
        {
            27 => value.IsSchSummonPet,
            28 => value.IsSmnSummonPet,
            _ => false,
        };
    }

    private unsafe void ActivateTankStance(byte jobId, ActionManager* am)
    {
        uint actionId = jobId switch
        {
            19 => 28,       // warrior
            21 => 48,       // paladin
            32 => 3629,     // dark knight
            37 => 16142,    // gunbreaker
            _ => throw new System.NotImplementedException()
        };

        if (am->GetActionStatus(ActionType.Action, actionId) == 0)
        {
            am->UseAction(ActionType.Action, actionId);
        }
    }

    private bool IsNormalDungeon(ushort territoryId)
    {
        var type = TerritoryNames[territoryId].Item2;
        return type == DutyType.Dungeon;
    }

    private bool IsNormalContent(ushort territoryId)
    {
        var type = TerritoryNames[territoryId].Item2;
        return type == DutyType.NormalRaid || type == DutyType.Alliance || type == DutyType.Trial || type == DutyType.Unknown;
    }

    private unsafe void CheckRemainingFoodBuff(ushort territoryId)
    {
        if (ClientState.LocalPlayer == null) return;
        if (IsNormalContent(territoryId) || IsNormalDungeon(territoryId)) return;

        var food = ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == 48);
        var timeRemaining = ClientState.LocalPlayer.StatusList.FirstOrDefault(x => x.StatusId == 48)?.RemainingTime;
        var refreshTime = Configuration.TerritoryConditions[territoryId].FoodBuffRefreshTime;

        if (!food || timeRemaining < refreshTime)
        {
            ChatGui.PrintError(strings.RefreshFood);
            UIGlobals.PlayChatSoundEffect(1);
        }
    }

    private unsafe void CheckGear(ushort territoryId)
    {
        if (ClientState.LocalPlayer == null) return;
        if (IsNormalContent(territoryId) || IsNormalDungeon(territoryId)) return;

        var equipmentScanner = new EquipmentScanner();

        if (equipmentScanner.GearNeedsRepairing(Configuration.GearRepairBreakpoint))
        {
            ChatGui.PrintError(strings.RepairGear);
            UIGlobals.PlayChatSoundEffect(1);
        }
        
    }

    private unsafe void ExecuteTankProtocol(byte jobId, ActionManager* am, ushort territoryId)
    {
        if (IsNormalContent(territoryId)) return;

        ushort stanceId = jobId switch
        {
            19 => 79,   // warrior
            21 => 91,   // paladin
            32 => 743,  // dark knight
            37 => 1833, // gunbreaker
            _ => 0
        };

        if (stanceId == 0 || ClientState.LocalPlayer == null)
            return;

        var stanceActive = ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == stanceId);
        var mainTankStanceIsOff = !stanceActive && IsMainTank(jobId, territoryId);
        var offTankStanceIsOn = stanceActive && !IsMainTank(jobId, territoryId);

        if (!IsNormalDungeon(territoryId) && (mainTankStanceIsOff || offTankStanceIsOn))
        {
            ActivateTankStance(jobId, am);
            return;
        } else if (IsNormalDungeon(territoryId) && !stanceActive)
        {
            ActivateTankStance(jobId, am);
        }
    }

    private unsafe void ExecutePetProtocol(byte jobId, ActionManager* am, ushort territoryId)
    {
        var summonPet = BuddyList.PetBuddy == null &&  (IsNormalDungeon(territoryId) || IsSummonPet(jobId, territoryId));

        if (jobId == 27 && summonPet) // scholar
        {
            if (am->GetActionStatus(ActionType.Action, 25798) != 0) return;
            am->UseAction(ActionType.Action, 25798);
        }

        if (jobId == 28 && summonPet) // summoner
        {
            if (am->GetActionStatus(ActionType.Action, 17215) != 0) return;
            am->UseAction(ActionType.Action, 17215);
        }
    }
}
