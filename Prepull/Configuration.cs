using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Prepull;

[Serializable]
[SupportedOSPlatform("windows")]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool DefaultMainTank { get; set; } = false;
    public int FoodBuffRefreshTime { get; set; } = 600;
    public int GearRepairBreakpoint { get; set; } = 3000; // 300 = 1%
    public Dictionary<ushort, TerritoryConfig> TerritoryConditions = [];

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        PrepullPlugin.PluginInterface.SavePluginConfig(this);
    }

    [Serializable]
    public class TerritoryConfig(bool defaultMainTank, int foodBuffRefreshTime)
    {
        public bool IsWarMainTank { get; set; } = defaultMainTank;
        public bool IsPldMainTank { get; set; } = defaultMainTank;
        public bool IsDrkMainTank { get; set; } = defaultMainTank;
        public bool IsGnbMainTank { get; set; } = defaultMainTank;
        public bool IsSchSummonPet { get; set; } = true;
        public bool IsSmnSummonPet { get; set; } = true;
        public int FoodBuffRefreshTime { get; set; } = foodBuffRefreshTime;
    }
}
