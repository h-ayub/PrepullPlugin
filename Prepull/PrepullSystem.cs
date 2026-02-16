using KamiLib.Extensions;
using System.Collections.Generic;

namespace Prepull;

#pragma warning disable CS8618
internal class PrepullSystem
{
    public static Configuration Configuration { get; set; }
    public static Dictionary<uint, (string, DutyType)> TerritoryNames = [];
}
