using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using KamiLib.Extensions;
using Prepull.Classes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Repositories
{
    [SupportedOSPlatform("windows")]
    public class TankRepository : BaseRepository, ITankRepository
    {
        public TankRepository(Configuration configuration, Dictionary<uint, (string, DutyType)> territoryNames, IChatGui chatGui, IClientState clientState, IBuddyList buddyList) : base(configuration, territoryNames, chatGui, clientState, buddyList)
        {
        }
        private bool IsMainTank(byte jobId, ushort territoryId)
        {
            if (!Configuration.TerritoryConditions.TryGetValue(territoryId, out var value))
            {
                value = new Configuration.TerritoryConfig(Configuration.DefaultMainTank, Configuration.FoodBuffRefreshTime);
                Configuration.TerritoryConditions[territoryId] = value;
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

        private unsafe void ActivateTankStance(byte jobId, ActionManager* am)
        {
            uint actionId = jobId switch
            {
                19 => 28,       // warrior
                21 => 48,       // paladin
                32 => 3629,     // dark knight
                37 => 16142,    // gunbreaker
                _ => throw new System.NotImplementedException()
            };

            if (am->GetActionStatus(ActionType.Action, actionId) == 0)
            {
                am->UseAction(ActionType.Action, actionId);
            }
        }

        public unsafe void ExecuteTankCheck(byte jobId, ActionManager* am, ushort territoryId)
        {
            if (IsNormalContent(territoryId)) return;

            ushort stanceId = jobId switch
            {
                19 => 79,   // warrior
                21 => 91,   // paladin
                32 => 743,  // dark knight
                37 => 1833, // gunbreaker
                _ => 0
            };

            if (stanceId == 0 || ClientState.LocalPlayer == null)
                return;

            var stanceActive = ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == stanceId);
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
