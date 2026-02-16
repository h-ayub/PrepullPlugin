using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using KamiLib.Extensions;
using Prepull.Classes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Repositories
{
    [SupportedOSPlatform("windows")]
    public class GearAndFoodRepository : BaseRepository, IGearAndFoodRepository
    {
        public GearAndFoodRepository(Configuration configuration, Dictionary<uint, (string, DutyType)> territoryNames, IChatGui chatGui, IClientState clientState, IBuddyList buddyList) : base(configuration, territoryNames, chatGui, clientState, buddyList)
        {
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
        public unsafe void ExecuteGearAndFoodCheck(ushort territoryId)
        {
            CheckRemainingFoodBuff(territoryId);
            CheckGear(territoryId);
        }
    }
}
