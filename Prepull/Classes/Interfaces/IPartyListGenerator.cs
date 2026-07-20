using Dalamud.Game.ClientState.Party;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using System.Collections.Generic;

namespace Prepull.Classes.Interfaces
{
    public interface IPartyListGenerator
    {
        public List<BattleChara> GenerateBattleCharaList();
        public List<IPartyMember> GeneratePartyList();
    }
}
