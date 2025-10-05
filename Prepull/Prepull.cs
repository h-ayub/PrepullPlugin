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
using FFXIVClientStructs.FFXIV.Client.UI;
using System.Runtime.Versioning;
using Prepull.Commands;

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

        var executor = new CommandsExecutor(Configuration, TerritoryNames, ChatGui, BuddyList, ClientState);
        executor.ExecuteAll(territoryId, jobId, am);
    }
}
