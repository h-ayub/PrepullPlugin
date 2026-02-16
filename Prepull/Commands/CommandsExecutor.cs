using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using KamiLib.Extensions;
using Prepull.Classes.Interfaces;
using Prepull.Classes.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Prepull.Commands
{
    [SupportedOSPlatform("windows")]
    public class CommandsExecutor
    {
        private readonly IGearAndFoodRepository gearAndFoodRepository;
        private readonly IPetRepository petRepository;
        private readonly ITankRepository tankRepository;

        public CommandsExecutor(Configuration configuration, Dictionary<uint, (string, DutyType)> territoryNames, IChatGui chatGui, IClientState clientState, IBuddyList buddyList)
        {
            this.gearAndFoodRepository = new GearAndFoodRepository(configuration, territoryNames, chatGui, clientState, buddyList);
            this.petRepository = new PetRepository(configuration, territoryNames, chatGui, clientState, buddyList);
            this.tankRepository = new TankRepository(configuration, territoryNames, chatGui, clientState, buddyList);
        }

        public unsafe void ExecuteAllChecks(ushort territoryId, byte jobId, ActionManager* am)
        {
            gearAndFoodRepository.ExecuteGearAndFoodCheck(territoryId);
            petRepository.ExecutePetCheck(jobId, am, territoryId);
            tankRepository.ExecuteTankCheck(jobId, am, territoryId);
        }
    }
}
