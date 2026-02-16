using FFXIVClientStructs.FFXIV.Client.UI;
using Prepull.Classes.Helpers;
using Prepull.Classes.Interfaces;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Repositories
{
    [SupportedOSPlatform("windows")]
    public class GearAndFoodRepository : BaseRepository, IGearAndFoodRepository
    {
        public GearAndFoodRepository() : base() { }
        private unsafe void CheckRemainingFoodBuff(ushort territoryId)
        {
            if (PrepullServices.ClientState.LocalPlayer == null) return;
            if (IsNormalContent(territoryId) || IsNormalDungeon(territoryId)) return;

            var food = PrepullServices.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == 48);
            var timeRemaining = PrepullServices.ClientState.LocalPlayer.StatusList.FirstOrDefault(x => x.StatusId == 48)?.RemainingTime;
            var refreshTime = PrepullSystem.Configuration.TerritoryConditions.TryGetValue(territoryId, out var value) ? value.FoodBuffRefreshTime : PrepullSystem.Configuration.FoodBuffRefreshTime;

            if (!food || timeRemaining < refreshTime)
            {
                PrepullServices.ChatGui.PrintError(strings.RefreshFood);
                UIGlobals.PlayChatSoundEffect(1);
            }
        }

        private unsafe void CheckGear(ushort territoryId)
        {
            if (PrepullServices.ClientState.LocalPlayer == null) return;
            if (IsNormalContent(territoryId) || IsNormalDungeon(territoryId)) return;

            var equipmentScanner = new EquipmentScanner();

            if (equipmentScanner.GearNeedsRepairing(PrepullSystem.Configuration.GearRepairBreakpoint))
            {
                PrepullServices.ChatGui.PrintError(strings.RepairGear);
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
