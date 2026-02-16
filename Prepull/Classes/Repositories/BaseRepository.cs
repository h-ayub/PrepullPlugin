using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using KamiLib.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Repositories
{
    [SupportedOSPlatform("windows")]
    public abstract class BaseRepository
    {
        protected static IClientState ClientState { get; private set; }
        protected static IBuddyList BuddyList { get; private set; }
        protected static IChatGui ChatGui { get; private set; }
        protected Configuration Configuration { get; set; }
        protected Dictionary<uint, (string, DutyType)> TerritoryNames { get; set; }
        protected BaseRepository(Configuration configuration, Dictionary<uint, (string, DutyType)> territoryNames, IChatGui chatGui, IClientState clientState, IBuddyList buddyList) 
        { 
            Configuration = configuration;
            TerritoryNames = territoryNames;
            ChatGui = chatGui;
            ClientState = clientState;
            BuddyList = buddyList;
        }

        public bool IsNormalDungeon(ushort territoryId)
        {
            var type = TerritoryNames[territoryId].Item2;
            return type == DutyType.Dungeon;
        }

        public bool IsNormalContent(ushort territoryId)
        {
            var type = TerritoryNames[territoryId].Item2;
            return type == DutyType.NormalRaid || type == DutyType.Alliance || type == DutyType.Trial || type == DutyType.Unknown;
        }
    }
}
