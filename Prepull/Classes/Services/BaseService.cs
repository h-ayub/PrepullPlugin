using FFXIVClientStructs.FFXIV.Client.UI;
using KamiLib.Extensions;
using Prepull.Classes.Interfaces;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]
    public abstract class BaseService
    {
        protected BaseService() 
        { 
        }

        protected bool IsNormalDungeon(ushort territoryId)
        {
            var type = PrepullSystem.TerritoryNames[territoryId].Item2;
            return type == DutyType.Dungeon;
        }

        protected bool IsNormalContent(ushort territoryId)
        {
            var type = PrepullSystem.TerritoryNames[territoryId].Item2;
            return type == DutyType.NormalRaid || type == DutyType.Alliance || type == DutyType.Trial || type == DutyType.Unknown;
        }

        protected void DisplayAndNotifyError(string message)
        {
            PrepullPluginServices.ChatGui.PrintError(message);
            UIGlobals.PlayChatSoundEffect(1);
        }
    }
}
