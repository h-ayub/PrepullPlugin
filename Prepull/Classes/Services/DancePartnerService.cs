using Prepull.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Services
{
    public class DancePartnerService : IDancePartnerService
    {
        // What do we want
        // 1. Check if dancer is in party
        // 2. Savage/Ultimate check if desired dance partner is in party
        // 3. Check for dance partner status

        public DancePartnerService() { }

        public void ExecuteDancePartnerCheck(byte jobId, ushort territoryId)
        {
            throw new NotImplementedException();
        }
    }
}
