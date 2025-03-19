using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace Prepull;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public Dictionary<ushort, TerritoryConfig> TerritoryConditions = [];

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Prepull.PluginInterface.SavePluginConfig(this);
    }

    [Serializable]
    public class TerritoryConfig
    {
        public bool IsWarMainTank { get; set; } = false;
        public bool IsPldMainTank { get; set; } = false;
        public bool IsDrkMainTank { get; set; } = false;
        public bool IsGnbMainTank { get; set; } = false;
        public bool IsSchSummonPet { get; set; } = true;
        public bool IsSmnSummonPet { get; set; } = true;
    }
}
