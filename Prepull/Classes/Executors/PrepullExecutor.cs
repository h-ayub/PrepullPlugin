using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Prepull.Classes.Repositories;
using System.Runtime.Versioning;

namespace Prepull.Classes.Executors
{
    [SupportedOSPlatform("windows")]
    public class PrepullExecutor
    {
        private readonly GearAndFoodRepository gearAndFoodRepository;
        private readonly PetRepository petRepository;
        private readonly TankRepository tankRepository;

        public PrepullExecutor()
        {
            this.gearAndFoodRepository = new GearAndFoodRepository();
            this.petRepository = new PetRepository();
            this.tankRepository = new TankRepository();
        }

        public unsafe void ExecuteAllChecks()
        {
            var am = ActionManager.Instance();
            var playerStatePtr = PlayerState.Instance();
            var territoryId = PrepullServices.ClientState.TerritoryType;
            var jobId = playerStatePtr->CurrentClassJobId;
            gearAndFoodRepository.ExecuteGearAndFoodCheck(territoryId);
            petRepository.ExecutePetCheck(jobId, am, territoryId);
            tankRepository.ExecuteTankCheck(jobId, am, territoryId);
        }
    }
}
