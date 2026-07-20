using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Prepull.Classes.Enums;
using Prepull.Classes.Extensions;
using Prepull.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]
    public class DancePartnerService : BaseService, IDancePartnerService
    {
        private readonly IActionExecutor actionExecutor;
        private readonly IPartyListGenerator partyListGenerator;
        private readonly uint closedPositionStatusCode = 1823;
        private readonly uint dancePartnerStatusCode = 1824;
        private List<IBattleChara> dancers = [];
        private List<IBattleChara> dancePartners = [];

        public DancePartnerService(IActionExecutor actionExecutor, IPartyListGenerator partyListGenerator) 
        { 
            this.actionExecutor = actionExecutor;
            this.partyListGenerator = partyListGenerator;
        }

        private bool IsBattleCharaMainPlayer(IBattleChara battleChara)
        {
            return battleChara.EntityId == PrepullPluginServices.ObjectTable.LocalPlayer?.EntityId;
        }

        private bool IsPlayerDancer(byte jobId)
        {
            return (FfxivJob)jobId == FfxivJob.Dancer;
        }

        private bool IsDancerInParty(List<IBattleChara> partyList)
        {
            foreach (var partyMember in partyList)
            {
                var partyMemberJobId = (byte)partyMember.ClassJob.RowId;
                if (IsPlayerDancer(partyMemberJobId) && partyMember.Level >= 60)
                {
                    dancers.Add(partyMember);
                }
            }

            return dancers.Count > 0;
        }

        private int GetDancePartnerPriority(byte jobId)
        {
            return (FfxivJob)jobId switch
            {
                FfxivJob.Samurai => 1, // Highest priority
                FfxivJob.Pictomancer => 2,
                FfxivJob.Reaper => 3,
                FfxivJob.Viper => 4,
                FfxivJob.Monk => 5,
                FfxivJob.Ninja => 6,
                FfxivJob.Dragoon => 7,
                FfxivJob.BlackMage => 8,
                FfxivJob.RedMage => 9,
                FfxivJob.Summoner => 10,
                FfxivJob.Machinist => 11,
                FfxivJob.Bard => 12,
                FfxivJob.Dancer => 13,
                _ => int.MaxValue // Not a dance partner
            };
        }

        private void AssignDancePartner(List<IBattleChara> partyList, string designatedPartnerName="")
        {
            var priority = int.MaxValue;
            IBattleChara? playerToAssign = null;

            foreach (var player in partyList)
            {
                if (player.HasStatus(dancePartnerStatusCode))
                    continue;
                if (IsBattleCharaMainPlayer(player))
                    continue;
                if (player.Name.TextValue.Equals(designatedPartnerName, StringComparison.OrdinalIgnoreCase))  // assign to designated partner if specified
                {
                    playerToAssign = player;
                    break;
                }

                var jobId = (byte)player.ClassJob.RowId;
                var currentPriority = GetDancePartnerPriority(jobId);
                if (currentPriority < priority)
                {
                    priority = currentPriority;
                    playerToAssign = player;
                }
            }

            if (playerToAssign != null)
            {
                actionExecutor.ExecuteActionByJobId((byte)FfxivJob.Dancer, playerToAssign.EntityId);
                _ = IsDancePartnerAssigned(partyList);
            }
        }

        private bool IsDancePartnerAssigned(List<IBattleChara> partyList)
        {
            dancePartners.Clear();
            foreach (var player in partyList)
            {
                if (player.HasStatus(dancePartnerStatusCode)) // dance partner status on other players
                {
                    dancePartners.Add(player);
                }
            }

            return dancers.Count == dancePartners.Count;
        }

        private void NormalContentDancePartnerCheck(byte mainPlayersJobId, ushort territoryId, List<IBattleChara> partyList)
        {
            if (IsDancePartnerAssigned(partyList))
                return;

            if (IsPlayerDancer(mainPlayersJobId))
            {
                AssignDancePartner(partyList);
            }

            if (dancers.Count != dancePartners.Count)
            {
                // Display error for dancers 
                foreach (var player in dancers.Where(x => !x.HasStatus(closedPositionStatusCode))) 
                { 
                    DisplayAndNotifyError(string.Format(PrepullStrings.DancePartnerUnassigned, player.Name.TextValue));
                }
            }
        }

        private void HighEndContentDancePartnerCheck(byte mainPlayersJobId, ushort territoryId, List<IBattleChara> partyList)
        {
            var territoryConfig = GetTerritoryConfig(territoryId);
            string targetPartner = territoryConfig.DesignatedDancePartner;
            bool hasTargetPartnerConfig = !string.IsNullOrEmpty(targetPartner);

            bool isPartnerAssigned = IsDancePartnerAssigned(partyList);
            bool designatedPartnerNeedsAssignment = (hasTargetPartnerConfig && !dancePartners.Any(x => x.Name.TextValue == targetPartner));

            bool needsPartnerAssignment = !isPartnerAssigned || designatedPartnerNeedsAssignment;

            if (!needsPartnerAssignment)
                return;

            if (IsPlayerDancer(mainPlayersJobId))
            {
                AssignDancePartner(partyList, targetPartner);
                return;
            }

            string errorMessage = designatedPartnerNeedsAssignment ? string.Format(PrepullStrings.DesignatedDancePartnerUnassigned, targetPartner) : PrepullStrings.DancePartnerUnassigned;

            DisplayAndNotifyError(errorMessage);
        }

        public void ExecuteDancePartnerCheck(byte jobId, ushort territoryId)
        {
            var partyList = partyListGenerator.GenerateBattleCharaList();
            if (!IsDancerInParty(partyList))
                return;

            if (IsNormalContent(territoryId))
                NormalContentDancePartnerCheck(jobId, territoryId, partyList);
            else
                HighEndContentDancePartnerCheck(jobId, territoryId, partyList);
        }
    }
}
