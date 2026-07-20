using FFXIVClientStructs.FFXIV.Client.Game;
using Prepull.Classes.Enums;
using Prepull.Classes.Interfaces;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]
    public class PetService : BaseService, IPetService
    {
        private readonly IActionExecutor actionExecutor;
        public PetService(IActionExecutor actionExecutor) : base() 
        {
            this.actionExecutor = actionExecutor;
        }

        private bool IsSummonPet(byte jobId, ushort territoryId)
        {
            var territoryConfig = GetTerritoryConfig(territoryId);
            return (FfxivJob)jobId switch
            {
                FfxivJob.Scholar => territoryConfig.IsSchSummonPet,
                FfxivJob.Summoner => territoryConfig.IsSmnSummonPet,
                _ => false,
            };
        }
        public void ExecutePetCheck(byte jobId, ushort territoryId)
        {
            var summonPet = PrepullPluginServices.BuddyList.PetBuddy == null && (IsNormalDungeon(territoryId) || IsSummonPet(jobId, territoryId));

            if (jobId == (byte)FfxivJob.Scholar && summonPet) // scholar
            {
                actionExecutor.ExecuteActionByJobId(jobId); // Summon Eos
            }

            if (jobId == (byte)FfxivJob.Summoner && summonPet) // summoner
            {
                actionExecutor.ExecuteActionByJobId(jobId); // Summon Carbuncle
            }
        }
    }
}
