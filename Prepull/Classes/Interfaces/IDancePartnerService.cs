using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.Interop;
using System.Collections.Generic;

namespace Prepull.Classes.Interfaces
{
    public interface IDancePartnerService
    {
        public void ExecuteDancePartnerCheck(byte jobId, ushort territoryId);
    }
}
