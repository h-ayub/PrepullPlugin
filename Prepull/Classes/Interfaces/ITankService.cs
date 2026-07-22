using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Interfaces
{
    public interface ITankService
    {
        public void ExecuteTankCheck(byte jobId, ushort territoryId);
    }
}
