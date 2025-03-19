using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Prepull.Windows;
using System.Collections.Generic;
using Lumina.Excel.Sheets;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace Prepull;

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

    private const string CommandName = "/ppp";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Prepull");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    internal Dictionary<uint, string> TerritoryNames = new();

    public Prepull()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        //WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = strings.HelpMessage
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
                x => $"{x.PlaceName.ValueNullable?.Name}{(x.ContentFinderCondition.ValueNullable?.Name.ToString().Length > 0 ? $" ({x.ContentFinderCondition.ValueNullable?.Name})" : string.Empty)}");
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        DutyState.DutyStarted -= ActivatePrepull;
        DutyState.DutyRecommenced -= ActivatePrepull;

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
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

        ActivateTankStance(jobId, am, territoryId);

        SummonPet(jobId, am, territoryId);
    }

    private bool IsMainTank(byte jobId, ushort territoryId)
    {
        if (!Configuration.TerritoryConditions.ContainsKey(territoryId))
        {
            Configuration.TerritoryConditions[territoryId] = new Configuration.TerritoryConfig();
        }
        switch (jobId)
        {
            case 19: return Configuration.TerritoryConditions[territoryId].IsWarMainTank;
            case 21: return Configuration.TerritoryConditions[territoryId].IsPldMainTank;
            case 32: return Configuration.TerritoryConditions[territoryId].IsDrkMainTank;
            case 37: return Configuration.TerritoryConditions[territoryId].IsGnbMainTank;
            default: return false;
        }
    }

    private bool IsSummonPet(byte jobId, ushort territoryId)
    {
        if (!Configuration.TerritoryConditions.ContainsKey(territoryId))
        {
            Configuration.TerritoryConditions[territoryId] = new Configuration.TerritoryConfig();
        }
        switch (jobId)
        {
            case 27: return Configuration.TerritoryConditions[territoryId].IsSchSummonPet;
            case 28: return Configuration.TerritoryConditions[territoryId].IsSmnSummonPet;
            default: return false;
        }
    }

    private unsafe void ActivateTankStance(byte jobId, ActionManager* am, ushort territoryId)
    {
        ushort stanceId = jobId switch
        {
            19 => 79,   // warrior
            21 => 91,   // paladin
            32 => 743,  // dark knight
            37 => 1833, // gunbreaker
            _ => 0
        };

        if (stanceId == 0)
            return;

        var stanceActive = ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == stanceId);

        if (!stanceActive && IsMainTank(jobId, territoryId) || stanceActive && !IsMainTank(jobId, territoryId))
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
    }

    private unsafe void SummonPet(byte jobId, ActionManager* am, ushort territoryId)
    {
        var summonPet = BuddyList.PetBuddy == null && IsSummonPet(jobId, territoryId);

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
