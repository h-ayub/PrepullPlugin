using Dalamud.Game.ClientState.Party;
using Dalamud.Game.ClientState.Statuses;
using Prepull.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace Prepull.Classes.Helpers
{
    [SupportedOSPlatform("windows")]

    public class StatusListScanner : IStatusListScanner
    {
        private IStatus? GetStatusById(uint statusId, IEnumerable<IStatus> statuses)
        {
            foreach (var status in statuses)
            {
                if (status.StatusId == statusId)
                {
                    return status;
                }
            }

            return null;
        }

        public IStatus? GetPartyMemberStatusById(uint statusId, IPartyMember partyMember)
        {
            return GetStatusById(statusId, partyMember.Statuses);
        }

        public IStatus? GetPlayerStatusById(uint statusId)
        {
            if (PrepullPluginServices.ObjectTable.LocalPlayer == null)
            {
                throw new InvalidOperationException("Local player is not available.");
            }

            return GetStatusById(statusId, PrepullPluginServices.ObjectTable.LocalPlayer.StatusList);
        }
    }
}
