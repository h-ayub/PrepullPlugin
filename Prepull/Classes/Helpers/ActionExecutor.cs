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
        public unsafe void ExecuteActionByActionId(uint actionId, ActionManager* am)
        {
            if (am->GetActionStatus(ActionType.Action, actionId) == 0)
            {
                am->UseAction(ActionType.Action, actionId);
            }
        }

        public unsafe void ExecuteActionByJobId(byte jobId, ActionManager* am)
        {
            var actionId = GetActionIdForJob(jobId);
            ExecuteActionByActionId(actionId, am);
        }
    }
}
