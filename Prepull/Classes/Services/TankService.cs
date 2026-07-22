using FFXIVClientStructs.FFXIV.Client.Game;
using Prepull.Classes.Interfaces;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]
    public class TankService : BaseService, ITankService
    {
        public TankService() : base() { }
        private bool IsMainTank(byte jobId, ushort territoryId)
        {
            if (!PrepullSystem.Configuration.TerritoryConditions.TryGetValue(territoryId, out var value))
            {
                value = new TerritoryConfig(PrepullSystem.Configuration.DefaultMainTank, PrepullSystem.Configuration.FoodBuffRefreshTime);
                PrepullSystem.Configuration.TerritoryConditions[territoryId] = value;
            }
            return jobId switch
            {
                19 => value.IsWarMainTank,
                21 => value.IsPldMainTank,
                32 => value.IsDrkMainTank,
                37 => value.IsGnbMainTank,
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
            uint actionId = jobId switch
            {
                19 => 28,       // warrior
                21 => 48,       // paladin
                32 => 3629,     // dark knight
                37 => 16142,    // gunbreaker
                _ => throw new System.NotImplementedException(),
            };

            if (am->GetActionStatus(ActionType.Action, actionId) == 0)
            {
                am->UseAction(ActionType.Action, actionId);
            }
        }

        public unsafe void ExecuteTankCheck(byte jobId, ActionManager* am, ushort territoryId)
        {
            if (!IsTank(jobId)) return;

            if (IsOtherNormalContent(territoryId)) return;

            ushort stanceId = jobId switch
            {
                19 => 79,   // warrior
                21 => 91,   // paladin
                32 => 743,  // dark knight
                37 => 1833, // gunbreaker
                _ => throw new System.NotImplementedException(),
            };

            if (PrepullPluginServices.ObjectTable.LocalPlayer == null)
                return;

            var stanceActive = PrepullPluginServices.ObjectTable.LocalPlayer.StatusList.Any(x => x.StatusId == stanceId);
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
