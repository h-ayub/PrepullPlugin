using Dalamud.Game.ClientState.Party;
using Dalamud.Game.ClientState.Statuses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prepull.Classes.Interfaces
{
    public interface IStatusListScanner
    {
        public IStatus? GetPartyMemberStatusById(uint statusId, IPartyMember partyMember);
        public IStatus? GetPlayerStatusById(uint statusId);
    }
}
