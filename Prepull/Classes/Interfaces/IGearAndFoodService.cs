namespace Prepull.Classes.Interfaces
{
    public interface IGearAndFoodService : IBaseService
    {
        public void ExecuteGearAndFoodCheck(ushort territoryId);
    }
}
