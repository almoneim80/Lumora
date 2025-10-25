namespace Lumora.Extensions
{
    public static class PaginationExtensions
    {
        public static async Task<PagedResult<T>> ApplyPaginationAsync<T>(
            this IQueryable<T> query,
            PaginationRequestDto pagination,
            CancellationToken cancellationToken = default)
        {
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(pagination.Skip)
                .Take(pagination.PageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };
        }
    }
}
