using Dalamud.Game.Command;
using Dalamud.Plugin;
using Prepull.Windows;
using Lumina.Excel.Sheets;
using System.Linq;
using KamiLib.Extensions;
using System.Runtime.Versioning;
using Prepull.Classes.Executors;

namespace Prepull;

[SupportedOSPlatform("windows")]
public sealed class PrepullPlugin : IDalamudPlugin
{
    private const string OpenMainWindow = "/ppp";
    private const string OpenConfigWindow = "/ppc";

    public PrepullPlugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PrepullPluginServices>();
        PrepullSystem.Configuration = PrepullPluginServices.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        PrepullSystem.ConfigWindow = new ConfigWindow(this);
        PrepullSystem.MainWindow = new MainWindow(this);

        PrepullSystem.WindowSystem.AddWindow(PrepullSystem.ConfigWindow);
        PrepullSystem.WindowSystem.AddWindow(PrepullSystem.MainWindow);

        PrepullPluginServices.CommandManager.AddHandler(OpenMainWindow, new CommandInfo(OnMainUICommand)
        {
            HelpMessage = strings.HelpMessageMainWindow
        });

        PrepullPluginServices.CommandManager.AddHandler(OpenConfigWindow, new CommandInfo(OnConfigUICommand)
        {
            HelpMessage = strings.HelpMessageConfigWindow
        });

        PrepullPluginServices.PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PrepullPluginServices.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PrepullPluginServices.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // This event is triggered when the player starts a duty
        PrepullPluginServices.DutyState.DutyStarted += ActivatePrepull;
        PrepullPluginServices.DutyState.DutyRecommenced += ActivatePrepull;

        // This fetches the territory names from excel sheet in dalamud repository
        PrepullSystem.TerritoryNames = PrepullPluginServices.DataManager.GetExcelSheet<TerritoryType>().Where(x => x.PlaceName.ValueNullable?.Name.ToString().Length > 0)
            .ToDictionary(x => x.RowId,
                x => ($"{x.PlaceName.ValueNullable?.Name} {(x.ContentFinderCondition.ValueNullable?.Name.ToString().Length > 0 ? $" ({x.ContentFinderCondition.ValueNullable?.Name})" : string.Empty)}",
                        PrepullPluginServices.DataManager.GetDutyType(x.ContentFinderCondition.Value)));
    }

    public void Dispose()
    {
        PrepullSystem.WindowSystem.RemoveAllWindows();

        PrepullSystem.ConfigWindow.Dispose();
        PrepullSystem.MainWindow.Dispose();

        PrepullPluginServices.DutyState.DutyStarted -= ActivatePrepull;
        PrepullPluginServices.DutyState.DutyRecommenced -= ActivatePrepull;

        PrepullPluginServices.CommandManager.RemoveHandler(OpenMainWindow);
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
        var executor = new PrepullExecutor();
        executor.ExecuteAllChecks();
    }
}
