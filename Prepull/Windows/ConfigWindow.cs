using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Prepull;

namespace Prepull.Windows;

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
    }
}
