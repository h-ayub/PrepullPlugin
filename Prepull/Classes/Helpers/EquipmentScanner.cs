using FFXIVClientStructs.FFXIV.Client.Game;
using Prepull.Classes.Interfaces;
using System;
using System.Runtime.Versioning;

// Credit to RepairMe plugin
namespace Prepull.Classes.Helpers
{
    [SupportedOSPlatform("windows")]
    public unsafe class EquipmentScanner : IEquipmentScanner
    {
        internal const uint EquipmentContainerSize = 13;

        public Action? NotificationTarget { private get; set; }

        private InventoryManager* inventoryManager;
        private InventoryContainer* equipmentContainer;
        private InventoryItem* equipmentInventoryItem;

        public EquipmentScanner()
        {
            EnableScanning();
        }

        private void EnableScanning()
        {
            inventoryManager = InventoryManager.Instance();
            equipmentContainer = inventoryManager->GetInventoryContainer(InventoryType.EquippedItems);
            equipmentInventoryItem = equipmentContainer->GetInventorySlot(0);
        }

        public bool GearNeedsRepairing()
        {
            var gearRepairBreakpoint = PrepullSystem.Configuration.GearRepairBreakpoint;
            var inventoryItem = equipmentInventoryItem;
            if (inventoryItem == null) return false;

            for (var i = 0; i < EquipmentContainerSize; i++, inventoryItem++)
            {
                if (inventoryItem->Condition <= (ushort)gearRepairBreakpoint)
                    return true;
            }

            return false;
        }
    }
}
