namespace CoreBanking.API.Models
{
    public class PaginationResponse<TEntity>(int pageIndex, int pageSize, long totalCount, IEnumerable<TEntity> items) where TEntity : class
    {
        public int PageIndex => pageIndex;
        public int PageSize => pageSize;
        public long TotalCount => totalCount;
        public IEnumerable<TEntity> Items => items;
    }
}
