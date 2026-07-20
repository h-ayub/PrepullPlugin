using FFXIVClientStructs.FFXIV.Client.Game;
using Prepull.Classes.Enums;
using Prepull.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Helpers
{
    public class ActionExecutor : IActionExecutor
    {
        public ActionExecutor()
        {
        }

        public uint GetActionIdForJob(byte jobId)
        {
            return (FfxivJob)jobId switch
            {
                FfxivJob.Paladin => 28,
                FfxivJob.Warrior => 48,
                FfxivJob.DarkKnight => 3629,
                FfxivJob.Gunbreaker => 16142,
                FfxivJob.Dancer => 16006,   // Dance Partner
                FfxivJob.Scholar => 25798,  // Summon Eos
                FfxivJob.Summoner => 17215, // Summon carbuncle
                _ => throw new System.NotImplementedException(),
            };
        }
        public unsafe void ExecuteActionByActionId(uint actionId, ActionManager* am, ulong targetId=3758096384uL)
        {
            if (am->GetActionStatus(ActionType.Action, actionId) == 0)
            {
                am->UseAction(ActionType.Action, actionId, targetId);
            }
        }

        public unsafe void ExecuteActionByJobId(byte jobId, ActionManager* am, ulong targetId=3758096384uL)
        {
            var actionId = GetActionIdForJob(jobId);
            ExecuteActionByActionId(actionId, am, targetId);
        }
    }
}
