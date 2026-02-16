namespace Prepull.Classes.Interfaces
{
    public interface IGearAndFoodService : IBaseService
    {
        public unsafe void ExecuteGearAndFoodCheck(ushort territoryId);
    }
}
