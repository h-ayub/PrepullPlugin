using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Interfaces
{
    public interface ITankService
    {
        public unsafe void ExecuteTankCheck(byte jobId, FFXIVClientStructs.FFXIV.Client.Game.ActionManager* am, ushort territoryId);
    }
}
