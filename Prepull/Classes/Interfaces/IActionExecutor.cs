using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Interfaces
{
    public interface IActionExecutor
    {
       public uint GetActionIdForJob(byte jobId);
       public unsafe void ExecuteActionByActionId(uint actionId, ActionManager* am, ulong targetId=3758096384uL);
       public unsafe void ExecuteActionByJobId(byte jobId, ActionManager* am, ulong targetId=3758096384uL);
    }
}
