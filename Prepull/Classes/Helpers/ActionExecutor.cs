using FFXIVClientStructs.FFXIV.Client.Game;
using Prepull.Classes.Enums;
using Prepull.Classes.Interfaces;

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
        public unsafe void ExecuteActionByActionId(uint actionId, ulong targetId=3758096384uL)
        {
            var am = ActionManager.Instance();
            if (am->GetActionStatus(ActionType.Action, actionId) == 0)
            {
                am->UseAction(ActionType.Action, actionId, targetId);
            }
        }

        public void ExecuteActionByJobId(byte jobId, ulong targetId=3758096384uL)
        {
            var actionId = GetActionIdForJob(jobId);
            ExecuteActionByActionId(actionId, targetId);
        }
    }
}
