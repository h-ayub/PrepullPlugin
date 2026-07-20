using FFXIVClientStructs.FFXIV.Client.Game;
using Prepull.Classes.Enums;
using Prepull.Classes.Interfaces;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]
    public class TankService : BaseService, ITankService
    {
        private readonly IActionExecutor actionExecutor;
        public TankService(IActionExecutor actionExecutor) : base() 
        {
            this.actionExecutor = actionExecutor;
        }

        private bool IsMainTank(byte jobId, ushort territoryId)
        {
            var territoryConfig = GetTerritoryConfig(territoryId);
            return (FfxivJob)jobId switch
            {
                FfxivJob.Paladin => territoryConfig.IsPldMainTank,
                FfxivJob.Warrior => territoryConfig.IsWarMainTank,
                FfxivJob.DarkKnight => territoryConfig.IsDrkMainTank,
                FfxivJob.Gunbreaker => territoryConfig.IsGnbMainTank,
                _ => false,
            };
        }

        private unsafe void ActivateTankStance(byte jobId, ActionManager* am)
        {
            actionExecutor.ExecuteActionByJobId(jobId, am);
        }

        public unsafe void ExecuteTankCheck(byte jobId, ActionManager* am, ushort territoryId)
        {
            if (IsOtherNormalContent(territoryId)) return;

            var actionId = actionExecutor.GetActionIdForJob(jobId);

            if (PrepullPluginServices.ObjectTable.LocalPlayer == null)
                return;

            var stanceActive = PrepullPluginServices.ObjectTable.LocalPlayer.StatusList.Any(x => x.StatusId == actionId);
            var mainTankStanceIsOff = !stanceActive && IsMainTank(jobId, territoryId);
            var offTankStanceIsOn = stanceActive && !IsMainTank(jobId, territoryId);

            if (!IsNormalDungeon(territoryId) && (mainTankStanceIsOff || offTankStanceIsOn))
            {
                ActivateTankStance(jobId, am);
                return;
            }
            else if (IsNormalDungeon(territoryId) && !stanceActive)
            {
                ActivateTankStance(jobId, am);
            }
        }
    }
}
