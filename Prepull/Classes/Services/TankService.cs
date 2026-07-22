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

        private bool IsTank(byte jobId)
        {
            var job = (FfxivJob)jobId;
            return job == FfxivJob.Paladin || job == FfxivJob.Warrior || job == FfxivJob.DarkKnight || job == FfxivJob.Gunbreaker;
        }

        private bool IsMainTank(byte jobId, ushort territoryId)
        {
            actionExecutor.ExecuteActionByJobId(jobId);
        }

        public void ExecuteTankCheck(byte jobId, ushort territoryId)
        {
            if (!IsTank(jobId)) return;

            if (IsOtherNormalContent(territoryId)) return;

            var actionId = actionExecutor.GetActionIdForJob(jobId);

            if (PrepullPluginServices.ObjectTable.LocalPlayer == null)
                return;

            var stanceActive = PrepullPluginServices.ObjectTable.LocalPlayer.StatusList.Any(x => x.StatusId == actionId);
            var mainTankStanceIsOff = !stanceActive && IsMainTank(jobId, territoryId);
            var offTankStanceIsOn = stanceActive && !IsMainTank(jobId, territoryId);

            if (!IsNormalDungeon(territoryId) && (mainTankStanceIsOff || offTankStanceIsOn))
            {
                ActivateTankStance(jobId);
                return;
            }
            else if (IsNormalDungeon(territoryId) && !stanceActive)
            {
                ActivateTankStance(jobId);
            }
        }
    }
}
