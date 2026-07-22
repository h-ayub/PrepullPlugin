using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using System.Collections.Generic;

namespace Prepull.Classes.Interfaces
{
    public interface IPartyListGenerator
    {
        public List<IBattleChara> GenerateBattleCharaList();
        public List<IPartyMember> GeneratePartyList();
    }
}
