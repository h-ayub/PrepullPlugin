using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.Interop;
using MediatR;
using Prepull.Classes.Interfaces;
using Prepull.Classes.Services;
using System.Collections.Generic;
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
        private readonly IDancePartnerService dancePartnerService;

        public PrepullCommandHandler(IGearAndFoodService gearAndFoodService, IPetService petService, ITankService tankService, IDancePartnerService dancePartnerService) 
        {
            this.gearAndFoodService = gearAndFoodService;
            this.petService = petService;
            this.tankService = tankService;
            this.dancePartnerService = dancePartnerService;
        }


        public unsafe Task Handle(PrepullCommand request, CancellationToken cancellationToken)
        {
            var am = ActionManager.Instance();
            var playerStatePtr = PlayerState.Instance();
            var territoryId = (ushort)PrepullPluginServices.ClientState.TerritoryType;
            var jobId = playerStatePtr->CurrentClassJobId;

            gearAndFoodService.ExecuteGearAndFoodCheck(territoryId);
            petService.ExecutePetCheck(jobId, territoryId);
            tankService.ExecuteTankCheck(jobId, territoryId);
            dancePartnerService.ExecuteDancePartnerCheck(jobId, territoryId);

            return Task.CompletedTask;
        }
    }
}
