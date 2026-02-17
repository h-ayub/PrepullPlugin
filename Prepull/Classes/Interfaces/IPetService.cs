using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Interfaces
{
    public interface IPetService : IBaseService
    {
        public unsafe void ExecutePetCheck(byte jobId, FFXIVClientStructs.FFXIV.Client.Game.ActionManager* am, ushort territoryId);
    }
}
