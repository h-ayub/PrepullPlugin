using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.Interop;
using System.Collections.Generic;

namespace Prepull.Classes.Interfaces
{
    public interface IDancePartnerService
    {
        public unsafe void ExecuteDancePartnerCheck(byte jobId, ActionManager* am, ushort territoryId);
    }
}
