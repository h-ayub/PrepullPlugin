using FFXIVClientStructs.FFXIV.Client.Game;
using System;

// Credit to RepairMe plugin
namespace Prepull.Classes.Helpers
{
    public unsafe class EquipmentScanner
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

        public bool GearNeedsRepairing(int gearRepairBreakpoint)
        {
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
