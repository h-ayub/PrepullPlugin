using KamiLib.Extensions;
using System.Runtime.Versioning;

namespace Prepull.Classes.Repositories
{
    [SupportedOSPlatform("windows")]
    public abstract class BaseRepository
    {
        protected BaseRepository() 
        { 
        }

        public bool IsNormalDungeon(ushort territoryId)
        {
            var type = PrepullSystem.TerritoryNames[territoryId].Item2;
            return type == DutyType.Dungeon;
        }

        public bool IsNormalContent(ushort territoryId)
        {
            var type = PrepullSystem.TerritoryNames[territoryId].Item2;
            return type == DutyType.NormalRaid || type == DutyType.Alliance || type == DutyType.Trial || type == DutyType.Unknown;
        }
    }
}
