using Dalamud.Game.ClientState.Party;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Prepull.Classes.Interfaces;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Prepull.Classes.Helpers
{
    [SupportedOSPlatform("windows")]
    public class PartyListGenerator : IPartyListGenerator
    {

        public List<BattleChara> GenerateBattleCharaList()
        {
            List<BattleChara> partyMembers = [];

            unsafe
            {
                foreach (var characterEntry in CharacterManager.Instance()->BattleCharas)
                {
                    BattleChara* chara = characterEntry.Value;
                    if (chara == null) continue;
                    if (chara->ObjectKind is not ObjectKind.Pc) continue;
                    if (!chara->GetIsTargetable()) continue;

                    var battleChara = *chara;
                    if (battleChara.IsPartyMember || battleChara.EntityId == PrepullPluginServices.ObjectTable.LocalPlayer?.EntityId)
                        partyMembers.Add(battleChara);
                }
            }

            return partyMembers;
        }

        public List<IPartyMember> GeneratePartyList()
        {
            var partyList = new List<IPartyMember>();

            for (var i = 0; i < PrepullPluginServices.PartyList.Length; i++)
            {
                var partyMember = PrepullPluginServices.PartyList[i];
                if (partyMember != null)
                {
                    partyList.Add(partyMember);
                }
            }

            return partyList;
        }
    }
}
