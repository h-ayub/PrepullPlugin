using Dalamud.Game.ClientState.Objects.Types;
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

        public List<IBattleChara> GenerateBattleCharaList()
        {
            List<IBattleChara> partyMembers = [];

            unsafe
            {
                var charaManager = CharacterManager.Instance();
                if (charaManager == null) return partyMembers;

                foreach (var characterEntry in charaManager->BattleCharas)
                {
                    BattleChara* chara = characterEntry.Value;
                    if (chara == null) continue;

                    // Un-comment filters as needed:
                    //if (chara->Character.GameObject.ObjectKind is not ObjectKind.Pc) continue;
                    if (!chara->GetIsTargetable()) continue;
                    //if (!chara->IsPartyMember || chara->NameString != PrepullPluginServices.ObjectTable.LocalPlayer?.Name.TextValue) continue;

                    // Convert the unmanaged pointer address into a Dalamud IBattleChara interface
                    var managedChara = PrepullPluginServices.ObjectTable.CreateObjectReference((nint)chara) as IBattleChara;

                    if (managedChara != null)
                    {
                        partyMembers.Add(managedChara);
                    }
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
