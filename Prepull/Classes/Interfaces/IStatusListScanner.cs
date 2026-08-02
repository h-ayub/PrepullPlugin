using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.ClientState.Statuses;

namespace Prepull.Classes.Interfaces
{
    public interface IStatusListScanner
    {
        public IStatus? GetPartyMemberStatusById(uint statusId, IBattleChara partyMember);
        public IStatus? GetPlayerStatusById(uint statusId);
        public bool PartyMemberHasStatus(uint statusId, IBattleChara partyMember);
        public bool PlayerHasStatus(uint statusId);
    }
}
