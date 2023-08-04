namespace ISZR.Web
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        // Aktuális lista megjelenítése
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            this.AddRange(items);
        }

        // Elöző oldal létezésének ellenőrzése
        public bool HasPreviousPage => PageIndex > 1;

        // Következő oldal létezésének ellenőrzése
        public bool HasNextPage => PageIndex < TotalPages;

        // Aktuális lapon szereplők hozzáadása a megjelenítési listához
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize = 10)
        {
            if (pageIndex < 1) { pageIndex = 1; }
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
