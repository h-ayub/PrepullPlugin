using FFXIVClientStructs.FFXIV.Client.UI;
using Prepull.Classes.Interfaces;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]
    public class GearAndFoodService : BaseService, IGearAndFoodService
    {
        IEquipmentScanner equipmentScanner;
        public GearAndFoodService(IEquipmentScanner equipmentScanner) : base() 
        {
            this.equipmentScanner = equipmentScanner;
        }

        private void CheckFoodForRefresh(ushort territoryId)
        {
            if (PrepullPluginServices.ObjectTable.LocalPlayer == null) return;
            if (IsNormalContent(territoryId) || IsNormalDungeon(territoryId)) return;

            var hasFoodBuff = PrepullPluginServices.ObjectTable.LocalPlayer.StatusList.Any(x => x.StatusId == 48);
            var timeRemaining = PrepullPluginServices.ObjectTable.LocalPlayer.StatusList.FirstOrDefault(x => x.StatusId == 48)?.RemainingTime;
            var refreshTime = PrepullSystem.Configuration.TerritoryConditions.TryGetValue(territoryId, out var value) ? value.FoodBuffRefreshTime : PrepullSystem.Configuration.FoodBuffRefreshTime;

            if (!hasFoodBuff || timeRemaining < refreshTime)
            {
                DisplayAndNotifyError(PrepullStrings.RefreshFood);
            }
        }

        private void CheckGearForRepairs(ushort territoryId)
        {
            if (PrepullPluginServices.ObjectTable.LocalPlayer == null) return;

            if (equipmentScanner.GearNeedsRepairing())
            {
                DisplayAndNotifyError(PrepullStrings.RepairGear);
            }
        }

        public void ExecuteGearAndFoodCheck(ushort territoryId)
        {
            CheckFoodForRefresh(territoryId);
            CheckGearForRepairs(territoryId);
        }
    }
}
