using FFXIVClientStructs.FFXIV.Client.UI;
using KamiLib.Extensions;
using Prepull.Classes.Interfaces;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]
    public class BaseService
    {
        public BaseService() 
        { 
        }

        protected TerritoryConfig GetTerritoryConfig(ushort territoryId)
        {
            if (!PrepullSystem.Configuration.TerritoryConditions.TryGetValue(territoryId, out var value))
            {
                value = new TerritoryConfig(PrepullSystem.Configuration.DefaultMainTank, PrepullSystem.Configuration.FoodBuffRefreshTime);
                PrepullSystem.Configuration.TerritoryConditions[territoryId] = value;
            }

            return value;
        }

        protected bool IsNormalDungeon(ushort territoryId)
        {
            var type = PrepullSystem.TerritoryNames[territoryId].Item2;
            return type == DutyType.Dungeon;
        }

        protected bool IsOtherNormalContent(ushort territoryId)
        {
            var type = PrepullSystem.TerritoryNames[territoryId].Item2;
            return type == DutyType.NormalRaid || type == DutyType.Alliance || type == DutyType.Trial || type == DutyType.Unknown;
        }

        public bool IsNormalContent(ushort territoryId)
        {
            return IsNormalDungeon(territoryId) || IsOtherNormalContent(territoryId);
        }

        protected void DisplayAndNotifyError(string message)
        {
            PrepullPluginServices.ChatGui.PrintError(message);
            UIGlobals.PlayChatSoundEffect(1);
        }
    }
}
