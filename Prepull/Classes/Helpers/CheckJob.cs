using Prepull.Classes.Enums;
using Prepull.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Helpers
{
    public class CheckJob : ICheckJob
    {
        public bool HasSummonablePet(byte jobId)
        {
            var job = (FfxivJob)jobId;
            return job == FfxivJob.Scholar || job == FfxivJob.Summoner;
        }

        public bool IsExpectedJob(byte jobId, FfxivJob expectedJob)
        {
            return (FfxivJob)jobId == expectedJob;
        }

        public bool IsTank(byte jobId)
        {
            var job = (FfxivJob)jobId;
            return job == FfxivJob.Paladin || job == FfxivJob.Warrior || job == FfxivJob.DarkKnight || job == FfxivJob.Gunbreaker;
        }
    }
}
