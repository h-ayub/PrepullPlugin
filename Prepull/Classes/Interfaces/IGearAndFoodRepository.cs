namespace Prepull.Classes.Interfaces
{
    public interface IGearAndFoodRepository : IBaseRepository
    {
        public unsafe void ExecuteGearAndFoodCheck(ushort territoryId);
    }
}
