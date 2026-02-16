using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using KamiLib.Extensions;
using Lumina.Excel.Sheets;
using Prepull.Classes.Executors;
using Prepull.Windows;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull;

[SupportedOSPlatform("windows")]
public sealed class PrepullPlugin : IDalamudPlugin
{
    private const string OpenMainWindow = "/ppp";
    private const string OpenConfigWindow = "/ppc";
    public static ConfigWindow ConfigWindow { get; set; }
    public static MainWindow MainWindow { get; set; }
    private readonly static WindowSystem WindowSystem = new("Prepull");

    public PrepullPlugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<PrepullPluginServices>();
        PrepullSystem.Configuration = PrepullPluginServices.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        PrepullPluginServices.CommandManager.AddHandler(OpenMainWindow, new CommandInfo(OnMainUICommand)
        {
            HelpMessage = PrepullStrings.HelpMessageMainWindow
        });

        PrepullPluginServices.CommandManager.AddHandler(OpenConfigWindow, new CommandInfo(OnConfigUICommand)
        {
            HelpMessage = PrepullStrings.HelpMessageConfigWindow
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
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

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

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    private void ActivatePrepull(object? sender, ushort e)
    {   
        var executor = new PrepullExecutor();
        executor.ExecuteAllChecks();
    }
}
