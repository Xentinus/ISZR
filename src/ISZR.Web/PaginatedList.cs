namespace ISZR.Web
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        // Aktuális lista megjelenítése
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            // Aktuális oldalszám
            PageIndex = pageIndex;

            // Megadott adatok alapján lapmennyiség kiszámítása
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            // Amennyieben 0 lenne az oldalak száma 1-et írjon ki
            if (TotalPages < 1) { TotalPages = 1; }

            // Lista megjelenítése
            this.AddRange(items);
        }

        // Elöző oldal létezésének ellenőrzése
        public bool HasPreviousPage => PageIndex > 1;

        // Következő oldal létezésének ellenőrzése
        public bool HasNextPage => PageIndex < TotalPages;

        // Aktuális lapon szereplők hozzáadása a megjelenítési listához
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize = 10)
        {
            // Amennyiben nem adták meg az aktuális oldalszámot, abban az esetben mindig az első oldalon kezdi
            if (pageIndex < 1) { pageIndex = 1; }

            // Lista elemeinek megszámlálása
            var count = await source.CountAsync();

            // Aktuális lapon szereplő elemek kiválasztása
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // Kiválasztott elemek megjelenítése
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
