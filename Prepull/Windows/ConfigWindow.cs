using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using System.Runtime.Versioning;

namespace Prepull.Windows;

[SupportedOSPlatform("windows")]
public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Prepull plugin) : base(strings.ConfigWindowTitle)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
    }

    public override void Draw()
    {
        var defaultMainTank = Configuration.DefaultMainTank;
        if (ImGui.Checkbox(strings.DefaultMainTank, ref defaultMainTank))
        {
            Configuration.DefaultMainTank = defaultMainTank;
            Configuration.Save();
        }

        var gearRepairBreakpoint = Configuration.GearRepairBreakpoint / 300;
        ImGui.SetNextItemWidth(100f * ImGuiHelpers.GlobalScale);
        if (ImGui.InputInt(strings.GearRepairBreakpoint, ref gearRepairBreakpoint, 1))
        {
            if (gearRepairBreakpoint < 1)
                gearRepairBreakpoint = 1;
            if (gearRepairBreakpoint > 99)
                gearRepairBreakpoint = 99;

            Configuration.GearRepairBreakpoint = gearRepairBreakpoint * 300;
            Configuration.Save();
        }
    }
}
