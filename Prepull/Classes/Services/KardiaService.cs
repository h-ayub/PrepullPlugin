using Dalamud.Game.ClientState.Objects.Types;
using MediatR;
using Prepull.Classes.Enums;
using Prepull.Classes.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Prepull.Classes.Services
{
    [SupportedOSPlatform("windows")]
    public class KardiaService : BaseService, IKardiaService
    {
        private readonly IActionExecutor actionExecutor;
        private readonly IPartyListGenerator partyListGenerator;
        private readonly ICheckJob checkJob;
        private readonly IStatusListScanner statusListScanner;
        private readonly uint kardiaStatusCode = 2604;
        private readonly uint kardionStatusCode = 2605;
        public KardiaService(IActionExecutor actionExecutor, IPartyListGenerator partyListGenerator, ICheckJob checkJob, IStatusListScanner statusListScanner)
        {
            this.actionExecutor = actionExecutor;
            this.partyListGenerator = partyListGenerator;
            this.checkJob = checkJob;
            this.statusListScanner = statusListScanner;
        }

        private bool IsKardiaActive()
        {
            return statusListScanner.PlayerHasStatus(kardiaStatusCode);
        }

        private bool IsKardionActive(IBattleChara partyMember)
        {
            return statusListScanner.PartyMemberHasStatus(kardionStatusCode, partyMember);
        }

        private void NormalContentKardiaCheck(byte jobId, ushort territoryId, List<IBattleChara> partyList)
        {
            if (IsKardiaActive())
                return;

            DisplayAndNotifyError(PrepullStrings.KardiaNotActive);

            if (IsNormalDungeon(territoryId))
            {
                foreach (var chara in partyList) 
                { 
                    if (checkJob.IsTank((byte)chara.ClassJob.RowId))
                    {
                        actionExecutor.ExecuteActionByJobId((byte)FfxivJob.Sage, chara.EntityId);
                    }
                }
            }
        }

        private void HighEndContentKardiaCheck(byte jobId, ushort territoryId, List<IBattleChara> partyList)
        {
            var territoryConfig = GetTerritoryConfig(territoryId);
            string targetKardiaPartner = territoryConfig.DesignatedKardiaPartner;
            bool hasDesignatedKardiaPartner = !string.IsNullOrEmpty(targetKardiaPartner);

            if (hasDesignatedKardiaPartner)
            {
                var partner = partyList.FirstOrDefault(x => x.Name.TextValue == targetKardiaPartner);
                if (partner != null && !IsKardionActive(partner))
                {
                    actionExecutor.ExecuteActionByJobId((byte)FfxivJob.Sage, partner.EntityId);
                    return;
                }
            } else if (!IsKardiaActive())
            {
                DisplayAndNotifyError(PrepullStrings.KardiaNotActive);
            }
        }

        public void ExecuteKardiaCheck(byte jobId, ushort territoryId)
        {
            if (!checkJob.IsExpectedJob(jobId, FfxivJob.Sage))
                return;

            var partyList = partyListGenerator.GenerateBattleCharaList();

            if (IsNormalContent(territoryId))
                NormalContentKardiaCheck(jobId, territoryId, partyList);
            else
                HighEndContentKardiaCheck(jobId, territoryId, partyList);
        }
    }
}
