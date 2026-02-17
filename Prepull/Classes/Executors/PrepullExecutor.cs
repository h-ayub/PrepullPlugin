using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Prepull.Classes.Repositories;
using System.Runtime.Versioning;

namespace Prepull.Classes.Executors
{
    [SupportedOSPlatform("windows")]
    public class PrepullExecutor
    {
        private readonly GearAndFoodService gearAndFoodRepository;
        private readonly PetService petRepository;
        private readonly TankService tankRepository;

        public PrepullExecutor()
        {
            this.gearAndFoodRepository = new GearAndFoodService();
            this.petRepository = new PetService();
            this.tankRepository = new TankService();
        }

        public unsafe void ExecuteAllChecks()
        {
            var am = ActionManager.Instance();
            var playerStatePtr = PlayerState.Instance();
            var territoryId = PrepullPluginServices.ClientState.TerritoryType;
            var jobId = playerStatePtr->CurrentClassJobId;
            gearAndFoodRepository.ExecuteGearAndFoodCheck(territoryId);
            petRepository.ExecutePetCheck(jobId, am, territoryId);
            tankRepository.ExecuteTankCheck(jobId, am, territoryId);
        }
    }
}
