using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.Interop;
using Prepull.Classes.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

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
                    if (chara->ObjectKind is not ObjectKind.Pc && chara->ObjectKind is not ObjectKind.BattleNpc) continue;
                    if (!chara->GetIsTargetable()) continue;

                    var battleChara = *chara;

                    if (battleChara.ObjectKind == ObjectKind.Pc && battleChara.IsPartyMember)
                        partyMembers.Add(battleChara);

                    if (battleChara.ObjectKind == ObjectKind.BattleNpc) // temp for trusts
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
