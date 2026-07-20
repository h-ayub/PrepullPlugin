using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Interfaces
{
    public interface IActionExecutor
    {
       public uint GetActionIdForJob(byte jobId);
       public void ExecuteActionByActionId(uint actionId, ulong targetId=3758096384uL);
       public void ExecuteActionByJobId(byte jobId, ulong targetId=3758096384uL);
    }
}
