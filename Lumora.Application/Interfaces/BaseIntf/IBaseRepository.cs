namespace Lumora.Application.Interfaces.BaseIntf
{
    public interface IBaseRepository
    {
        // للتحقق من وجود الكيان مع شرط عدم الحذف
        Task<bool> AnyAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : class;

        // لإضافة كيان جديد وحفظ التغييرات
        Task AddAsync<TEntity>(TEntity entity)
            where TEntity : class;

        // حفظ التغييرات (سيتم استدعاؤه داخلياً أو عبر UoW)
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
