using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace Prepull;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool DefaultMainTank { get; set; } = false;
    public Dictionary<ushort, TerritoryConfig> TerritoryConditions = [];

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Prepull.PluginInterface.SavePluginConfig(this);
    }

    [Serializable]
    public class TerritoryConfig(bool defaultMainTank)
    {
        public bool IsWarMainTank { get; set; } = defaultMainTank;
        public bool IsPldMainTank { get; set; } = defaultMainTank;
        public bool IsDrkMainTank { get; set; } = defaultMainTank;
        public bool IsGnbMainTank { get; set; } = defaultMainTank;
        public bool IsSchSummonPet { get; set; } = true;
        public bool IsSmnSummonPet { get; set; } = true;
    }
}
