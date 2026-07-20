using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Prepull.Classes.Enums;
using Prepull.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]

    public class DancePartnerService : BaseService, IDancePartnerService
    {
        private readonly IActionExecutor actionExecutor;
        private readonly IPartyListGenerator partyListGenerator;
        public DancePartnerService(IActionExecutor actionExecutor, IPartyListGenerator partyListGenerator) 
        { 
            this.actionExecutor = actionExecutor;
            this.partyListGenerator = partyListGenerator;
        }

        private bool IsPlayerDancer(byte jobId)
        {
            return (FfxivJob)jobId == FfxivJob.Dancer;
        }

        private bool IsDancerInParty(List<BattleChara> partyList)
        {
            foreach (var partyMember in partyList)
            {
                var jobId = partyMember.ClassJob;
                if (IsPlayerDancer(jobId))
                    return true;
            }

            return false;
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

        private unsafe void AssignDancePartner(ActionManager* am, List<BattleChara> partyList, string partnerName="")
        {
            ulong playerId = 0;
            var priority = int.MaxValue;

            foreach (var player in partyList)
            {
                if (player.NameString.Equals(partnerName, StringComparison.OrdinalIgnoreCase))  // assign to designated partner if specified
                {
                    actionExecutor.ExecuteActionByJobId((byte)FfxivJob.Dancer, am, player.EntityId);
                    return;
                }

                var jobId = player.ClassJob;
                var currentPriority = GetDancePartnerPriority(jobId);
                if (currentPriority < priority)
                {
                    priority = currentPriority;
                    playerId = player.EntityId;
                }
            }

            if (playerId != 0)
            {
                actionExecutor.ExecuteActionByJobId((byte)FfxivJob.Dancer, am, playerId);
            }
        }

        private bool TryIsDancePartnerAssigned(List<BattleChara> partyList, [NotNullWhen(true)] out BattleChara? partner)
        {
            foreach (var player in partyList)
            {
                if (player.HasStatus(1822)) // dance partner status on other players
                {
                    partner = player;
                    return true;
                }
            }
            partner = null;
            return false;
        }

        private unsafe void NormalContentDancePartnerCheck(byte jobId, ActionManager* am, ushort territoryId, List<BattleChara> partyList)
        {
            if (TryIsDancePartnerAssigned(partyList, out _))
                return;

            if (IsPlayerDancer(jobId))
            {
                AssignDancePartner(am, partyList);
                return;
            }

            DisplayAndNotifyError(PrepullStrings.DancePartnerUnassigned);
        }

        private unsafe void HighEndContentDancePartnerCheck(byte jobId, ActionManager* am, ushort territoryId, List<BattleChara> partyList)
        {
            var territoryConfig = GetTerritoryConfig(territoryId);
            string targetPartner = territoryConfig.DesignatedDancePartner;
            bool hasTargetConfig = !string.IsNullOrEmpty(targetPartner);

            bool isPartnerAssigned = TryIsDancePartnerAssigned(partyList, out var currentPartner);

            bool needsPartnerAssignment = !isPartnerAssigned || 
                (hasTargetConfig && currentPartner.Value.NameString != targetPartner);

            if (!needsPartnerAssignment)
                return;

            if (IsPlayerDancer(jobId))
            {
                AssignDancePartner(am, partyList, targetPartner);
                return;
            }

            string errorMessage = hasTargetConfig ? string.Format(PrepullStrings.DesignatedDancePartnerUnassigned, targetPartner) : PrepullStrings.DancePartnerUnassigned;

            DisplayAndNotifyError(errorMessage);
        }

        public unsafe void ExecuteDancePartnerCheck(byte jobId, ActionManager* am, ushort territoryId)
        {
            var partyList = partyListGenerator.GenerateBattleCharaList();
            if (!IsPlayerDancer(jobId) && !IsDancerInParty(partyList))
                return;

            if (IsNormalContent(territoryId))
                NormalContentDancePartnerCheck(jobId, am, territoryId, partyList);
            else
                HighEndContentDancePartnerCheck(jobId, am, territoryId, partyList);
        }
    }
}
