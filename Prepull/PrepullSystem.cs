using Dalamud.Interface.Windowing;
using KamiLib.Extensions;
using Prepull.Windows;
using System.Collections.Generic;

namespace Prepull;

#pragma warning disable CS8618
internal class PrepullSystem
{
    public static Configuration Configuration { get; set; }

    public static WindowSystem WindowSystem = new("Prepull");
    public static ConfigWindow ConfigWindow { get; set; }
    public static MainWindow MainWindow { get; set; }

    public static Dictionary<uint, (string, DutyType)> TerritoryNames = [];
}
