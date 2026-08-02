using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Interfaces
{
    public interface IKardiaService
    {
        public void ExecuteKardiaCheck(byte jobId, ushort territoryId);
    }
}
