
namespace Prepull.Classes.Interfaces
{
    public interface IBaseRepository
    {
        bool IsNormalDungeon(ushort territoryId);
        bool IsNormalContent(ushort territoryId);
    }
}
