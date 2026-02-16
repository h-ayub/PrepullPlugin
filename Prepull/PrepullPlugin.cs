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
using System.Runtime.Versioning;
using Prepull.Commands;

namespace Prepull;

[SupportedOSPlatform("windows")]
public sealed class PrepullPlugin : IDalamudPlugin
{
    private const string OpenMainWindow = "/ppp";
    private const string OpenConfigWindow = "/ppc";

    public PrepullPlugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PrepullServices>();
        PrepullSystem.Configuration = PrepullServices.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        PrepullSystem.ConfigWindow = new ConfigWindow(this);
        PrepullSystem.MainWindow = new MainWindow(this);

        PrepullSystem.WindowSystem.AddWindow(PrepullSystem.ConfigWindow);
        PrepullSystem.WindowSystem.AddWindow(PrepullSystem.MainWindow);

        PrepullServices.CommandManager.AddHandler(OpenMainWindow, new CommandInfo(OnMainUICommand)
        {
            HelpMessage = strings.HelpMessageMainWindow
        });

        PrepullServices.CommandManager.AddHandler(OpenConfigWindow, new CommandInfo(OnConfigUICommand)
        {
            HelpMessage = strings.HelpMessageConfigWindow
        });

        PrepullServices.PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PrepullServices.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PrepullServices.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // This event is triggered when the player starts a duty
        PrepullServices.DutyState.DutyStarted += ActivatePrepull;
        PrepullServices.DutyState.DutyRecommenced += ActivatePrepull;

        // This fetches the territory names from excel sheet in dalamud repository
        PrepullSystem.TerritoryNames = PrepullServices.DataManager.GetExcelSheet<TerritoryType>().Where(x => x.PlaceName.ValueNullable?.Name.ToString().Length > 0)
            .ToDictionary(x => x.RowId,
                x => ($"{x.PlaceName.ValueNullable?.Name} {(x.ContentFinderCondition.ValueNullable?.Name.ToString().Length > 0 ? $" ({x.ContentFinderCondition.ValueNullable?.Name})" : string.Empty)}",
                        PrepullServices.DataManager.GetDutyType(x.ContentFinderCondition.Value)));
    }

    public void Dispose()
    {
        PrepullSystem.WindowSystem.RemoveAllWindows();

        PrepullSystem.ConfigWindow.Dispose();
        PrepullSystem.MainWindow.Dispose();

        PrepullServices.DutyState.DutyStarted -= ActivatePrepull;
        PrepullServices.DutyState.DutyRecommenced -= ActivatePrepull;

        PrepullServices.CommandManager.RemoveHandler(OpenMainWindow);
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

    private void DrawUI() => PrepullSystem.WindowSystem.Draw();

    public void ToggleConfigUI() => PrepullSystem.ConfigWindow.Toggle();
    public void ToggleMainUI() => PrepullSystem.MainWindow.Toggle();

    private unsafe void ActivatePrepull(object? sender, ushort e)
    {   
        var am = ActionManager.Instance();
        var playerStatePtr = PlayerState.Instance();
        var territoryId = PrepullServices.ClientState.TerritoryType;
        var jobId = playerStatePtr->CurrentClassJobId;

        var executor = new CommandsExecutor(PrepullSystem.Configuration, PrepullSystem.TerritoryNames, PrepullServices.ChatGui, PrepullServices.ClientState, PrepullServices.BuddyList);
        executor.ExecuteAllChecks(territoryId, jobId, am);
    }
}
