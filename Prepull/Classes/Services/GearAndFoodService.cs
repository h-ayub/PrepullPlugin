using FFXIVClientStructs.FFXIV.Client.UI;
using Prepull.Classes.Helpers;
using Prepull.Classes.Interfaces;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Repositories
{
    [SupportedOSPlatform("windows")]
    public class GearAndFoodService : BaseService, IGearAndFoodService
    {
        public GearAndFoodService() : base() { }
        private void CheckRemainingFoodBuff(ushort territoryId)
        {
            if (PrepullPluginServices.ClientState.LocalPlayer == null) return;
            if (IsNormalContent(territoryId) || IsNormalDungeon(territoryId)) return;

            var food = PrepullPluginServices.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == 48);
            var timeRemaining = PrepullPluginServices.ClientState.LocalPlayer.StatusList.FirstOrDefault(x => x.StatusId == 48)?.RemainingTime;
            var refreshTime = PrepullSystem.Configuration.TerritoryConditions.TryGetValue(territoryId, out var value) ? value.FoodBuffRefreshTime : PrepullSystem.Configuration.FoodBuffRefreshTime;

            if (!food || timeRemaining < refreshTime)
            {
                PrepullPluginServices.ChatGui.PrintError(PrepullStrings.RefreshFood);
                UIGlobals.PlayChatSoundEffect(1);
            }
        }

        private void CheckGear(ushort territoryId)
        {
            if (PrepullPluginServices.ClientState.LocalPlayer == null) return;
            if (IsNormalContent(territoryId) || IsNormalDungeon(territoryId)) return;

            var equipmentScanner = new EquipmentScanner();

            if (equipmentScanner.GearNeedsRepairing(PrepullSystem.Configuration.GearRepairBreakpoint))
            {
                PrepullPluginServices.ChatGui.PrintError(PrepullStrings.RepairGear);
                UIGlobals.PlayChatSoundEffect(1);
            }
        }
        public void ExecuteGearAndFoodCheck(ushort territoryId)
        {
            CheckRemainingFoodBuff(territoryId);
            CheckGear(territoryId);
        }
    }
}
