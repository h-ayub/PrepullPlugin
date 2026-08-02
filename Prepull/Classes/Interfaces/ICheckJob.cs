using Prepull.Classes.Enums;

namespace Prepull.Classes.Interfaces
{
    public interface ICheckJob
    {
        public bool IsTank(byte jobId);
        public bool HasSummonablePet(byte jobId);
        public bool IsExpectedJob(byte jobId, FfxivJob expectedJob);
    }
}
