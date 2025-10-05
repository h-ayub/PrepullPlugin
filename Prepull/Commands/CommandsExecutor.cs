using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using KamiLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Prepull.Commands
{
    [SupportedOSPlatform("windows")]
    public class CommandsExecutor(Configuration configuration, Dictionary<uint, (string, DutyType)> territoryNames, IChatGui chatGui, IBuddyList buddyList, IClientState clientState)
    {
        private BaseCommands baseCommands { get; set; } = new BaseCommands(configuration, territoryNames, chatGui, clientState, buddyList);
        private PetCommands petCommands { get; set; } = new PetCommands(configuration, territoryNames, chatGui, clientState, buddyList);
        private TankCommands tankCommands { get; set; } = new TankCommands(configuration, territoryNames, chatGui, clientState, buddyList);

        public unsafe void ExecuteAll(ushort territoryId, byte jobId, ActionManager* am)
        {
            baseCommands.ExecuteBasics(territoryId);
            petCommands.ExecutePetProtocol(jobId, am, territoryId);
            tankCommands.ExecuteTankProtocol(jobId, am, territoryId);
        }
    }
}
