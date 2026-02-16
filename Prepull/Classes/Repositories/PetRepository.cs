using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using KamiLib.Extensions;
using Prepull.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Prepull.Classes.Repositories
{
    [SupportedOSPlatform("windows")]
    public class PetRepository : BaseRepository, IPetRepository
    {
        public PetRepository(Configuration configuration, Dictionary<uint, (string, DutyType)> territoryNames, IChatGui chatGui, IClientState clientState, IBuddyList buddyList) : base(configuration, territoryNames, chatGui, clientState, buddyList)
        {
        }

        private bool IsSummonPet(byte jobId, ushort territoryId)
        {
            if (!Configuration.TerritoryConditions.TryGetValue(territoryId, out var value))
            {
                value = new TerritoryConfig(Configuration.DefaultMainTank, Configuration.FoodBuffRefreshTime);
                Configuration.TerritoryConditions[territoryId] = value;
            }
            return jobId switch
            {
                27 => value.IsSchSummonPet,
                28 => value.IsSmnSummonPet,
                _ => false,
            };
        }
        public unsafe void ExecutePetCheck(byte jobId, ActionManager* am, ushort territoryId)
        {
            var summonPet = BuddyList.PetBuddy == null && (IsNormalDungeon(territoryId) || IsSummonPet(jobId, territoryId));

            if (jobId == 27 && summonPet) // scholar
            {
                if (am->GetActionStatus(ActionType.Action, 25798) != 0) return;
                am->UseAction(ActionType.Action, 25798);
            }

            if (jobId == 28 && summonPet) // summoner
            {
                if (am->GetActionStatus(ActionType.Action, 17215) != 0) return;
                am->UseAction(ActionType.Action, 17215);
            }
        }
    }
}
