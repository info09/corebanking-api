namespace CoreBanking.API.Models
{
    public class PaginationResponse<TEntity>(int index, int pageSize, long totalCount, IEnumerable<TEntity> items) where TEntity : class
    {
        public int Index => index;
        public int PageSize => pageSize;
        public long TotalCount => totalCount;
        public IEnumerable<TEntity> Items => items;
    }
}
