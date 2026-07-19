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
    public class PrepullCommand : IRequest
    {
    }

    [SupportedOSPlatform("windows")]
    public class PrepullCommandHandler : IRequestHandler<PrepullCommand>
    {
        private readonly IGearAndFoodService gearAndFoodService;
        private readonly IPetService petService;
        private readonly ITankService tankService;

        public PrepullCommandHandler(IGearAndFoodService gearAndFoodService, IPetService petService, ITankService tankService) 
        {
            this.gearAndFoodService = gearAndFoodService;
            this.petService = petService;
            this.tankService = tankService;
        }


        public unsafe Task Handle(PrepullCommand request, CancellationToken cancellationToken)
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
