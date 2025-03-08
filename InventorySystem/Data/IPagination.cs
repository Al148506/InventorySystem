namespace InventorySystem.Data
{
    public interface IPagination
    {
        int InitialPage { get; }
        int TotalPages { get; }
    }
}
