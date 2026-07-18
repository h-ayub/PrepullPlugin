using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using MediatR;
using Prepull.Classes.Interfaces;
using Prepull.Classes.Services;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Prepull.Classes.Executors
{
    public class PrepullExecutor : IRequest
    {
    }

    [SupportedOSPlatform("windows")]
    public class PrepullExecutorHandler : IRequestHandler<PrepullExecutor>
    {
        private readonly IGearAndFoodService gearAndFoodService;
        private readonly IPetService petService;
        private readonly ITankService tankService;

        public PrepullExecutorHandler(IGearAndFoodService gearAndFoodService, IPetService petService, ITankService tankService) 
        {
            this.gearAndFoodService = gearAndFoodService;
            this.petService = petService;
            this.tankService = tankService;
        }


        public unsafe Task Handle(PrepullExecutor request, CancellationToken cancellationToken)
        {
            var am = ActionManager.Instance();
            var playerStatePtr = PlayerState.Instance();
            var territoryId = (ushort)PrepullPluginServices.ClientState.TerritoryType;
            var jobId = playerStatePtr->CurrentClassJobId;

            gearAndFoodService.ExecuteGearAndFoodCheck(territoryId);
            petService.ExecutePetCheck(jobId, am, territoryId);
            tankService.ExecuteTankCheck(jobId, am, territoryId);

            return Task.CompletedTask;
        }
    }
}
