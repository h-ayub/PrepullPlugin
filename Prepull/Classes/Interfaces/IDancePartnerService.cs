using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Interfaces
{
    public interface IDancePartnerService
    {
        public void ExecuteDancePartnerCheck(byte jobId, ushort territoryId);
    }
}
