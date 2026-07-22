using FFXIVClientStructs.FFXIV.Client.UI;
using Prepull.Classes.Interfaces;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]
    public class GearAndFoodService : BaseService, IGearAndFoodService
    {
        private readonly IEquipmentScanner equipmentScanner;
        private readonly IStatusListScanner statusListScanner;
        public GearAndFoodService(IEquipmentScanner equipmentScanner, IStatusListScanner statusListScanner) : base() 
        {
            this.equipmentScanner = equipmentScanner;
            this.statusListScanner = statusListScanner;
        }

        private void CheckFoodForRefresh(ushort territoryId)
        {
            if (PrepullPluginServices.ObjectTable.LocalPlayer == null) return;
            if (IsNormalContent(territoryId)) return;

            var foodBuff = statusListScanner.GetPlayerStatusById(48); // well fed buff id
            var timeRemaining = foodBuff?.RemainingTime;
            var refreshTime = PrepullSystem.Configuration.TerritoryConditions.TryGetValue(territoryId, out var value) ? value.FoodBuffRefreshTime : PrepullSystem.Configuration.FoodBuffRefreshTime;

            if (foodBuff == null || timeRemaining < refreshTime)
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
