using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Data
{
    public class Pagination<T> :List<T>
    {
        public int InitialPage {  get; private set; }
        public int TotalPages {  get; private set; }

        public Pagination(List<T> items, int counter, int intialPage, int regQuantity )
        {
            InitialPage = intialPage;
            TotalPages = (int)Math.Ceiling(counter / (double)regQuantity);
            this.AddRange(items);
        }

        public bool PreviousPages => InitialPage > 1;
        public bool LaterPages => InitialPage < TotalPages;

        public static async Task<Pagination<T>> CreatePagination(IQueryable<T> source, int initialPage, int regQuantity)
        {
            var counter = await source.CountAsync();
            var items = await source.Skip(initialPage - 1).Take(regQuantity).ToListAsync();
            return new Pagination<T>(items, counter, initialPage, regQuantity);
        }
    }
}
