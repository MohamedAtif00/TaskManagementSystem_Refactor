namespace AutomatedTaskSystem.Helper
{
    public class PageList<T>
    {
        public PageList(List<T> items, int page, int pageSize, int totalCount)
        {
            this.items = items;
            this.page = page;
            this.pageSize = pageSize;
            this.totalCount = totalCount;
            this.totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        }
        public List<T> items { get; }
        public int page { get; }
        public int pageSize { get; }
        public int totalCount { get; }
        public int totalPages { get; }

        public bool hasNextPage => page * pageSize <= totalCount;
        public bool hasPreviousPage => page > 1;


        public List<int> NextPages
        {
            get
            {
                var nextPages = new List<int>();
                for (int i = 1; i <= 5; i++)
                {
                    if (page + i <= totalPages)
                    {
                        nextPages.Add(page + i);
                    }
                    else
                    {
                        break;
                    }
                }
                return nextPages;
            }
        }

        public List<int> LastPages
        {
            get
            {
                var lastPages = new List<int>();
                // Start from the last page and move backwards
                for (int i = page; i > 0; i--)
                {
                    if (i != page)
                        lastPages.Add(i);
                }
                lastPages.Reverse();
                return lastPages;
            }
        }

        public static async Task<PageList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize)
        {
            try
            {
                int totalCount = await query.CountAsync();
                var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                return new(items, page, pageSize, totalCount);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
    }
}
