using Dalamud.IoC;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using KamiLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Prepull.Commands
{
    [SupportedOSPlatform("windows")]
    public class BaseCommands
    {
        protected static IClientState ClientState { get; private set; }
        protected static IBuddyList BuddyList { get; private set; }

        protected static IChatGui ChatGui { get; private set; }
        protected Configuration Configuration { get; set; }
        protected Dictionary<uint, (string, DutyType)> TerritoryNames = [];
        public BaseCommands(Configuration configuration, Dictionary<uint, (string, DutyType)> territoryNames, IChatGui chatGui, IClientState clientState, IBuddyList buddyList)
        {
            Configuration = configuration;
            TerritoryNames = territoryNames;
            ChatGui = chatGui;
            BuddyList = buddyList;
            ClientState = clientState;
        }

        protected bool IsNormalDungeon(ushort territoryId)
        {
            var type = TerritoryNames[territoryId].Item2;
            return type == DutyType.Dungeon;
        }

        protected bool IsNormalContent(ushort territoryId)
        {
            var type = TerritoryNames[territoryId].Item2;
            return type == DutyType.NormalRaid || type == DutyType.Alliance || type == DutyType.Trial || type == DutyType.Unknown;
        }

        private unsafe void CheckRemainingFoodBuff(ushort territoryId)
        {
            if (ClientState.LocalPlayer == null) return;
            if (IsNormalContent(territoryId) || IsNormalDungeon(territoryId)) return;

            var food = ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == 48);
            var timeRemaining = ClientState.LocalPlayer.StatusList.FirstOrDefault(x => x.StatusId == 48)?.RemainingTime;
            var refreshTime = Configuration.TerritoryConditions.TryGetValue(territoryId, out var value) ? value.FoodBuffRefreshTime : Configuration.FoodBuffRefreshTime;

            if (!food || timeRemaining < refreshTime)
            {
                ChatGui.PrintError(strings.RefreshFood);
                UIGlobals.PlayChatSoundEffect(1);
            }
        }

        private unsafe void CheckGear(ushort territoryId)
        {
            if (ClientState.LocalPlayer == null) return;
            if (IsNormalContent(territoryId) || IsNormalDungeon(territoryId)) return;

            var equipmentScanner = new EquipmentScanner();

            if (equipmentScanner.GearNeedsRepairing(Configuration.GearRepairBreakpoint))
            {
                ChatGui.PrintError(strings.RepairGear);
                UIGlobals.PlayChatSoundEffect(1);
            }
        }

        public unsafe void ExecuteBasics(ushort territoryId)
        { 
            CheckRemainingFoodBuff(territoryId);
            CheckGear(territoryId);
        }
    }
}
