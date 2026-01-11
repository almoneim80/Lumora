using Lumora.Application.Interfaces.ImportIntf;
using System.Collections;

namespace Lumora.Infrastructure.Repositories
{
    public class ImportRepository : IImportRepository
    {
        private readonly PgDbContext _context;
        public ImportRepository(PgDbContext context)
        {
            _context = context;
        }

        public IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class
        {
            return _context.Set<TEntity>().AsNoTracking();
        }

        public IQueryable<object> GetDynamicQueryable(Type type)
        {
            // 1. الحصول على النوع الخاص بـ DbContext.Set<T>() للنوع المطلوب
            // واستخدام Reflection لاستدعاء النسخة الـ Generic من Set
            var method = typeof(DbContext)
                .GetMethods()
                .First(m => m.Name == nameof(DbContext.Set) && m.IsGenericMethod && m.GetParameters().Length == 0)
                .MakeGenericMethod(type);

            // 2. استدعاء الدالة والحصول على IQueryable<T>
            var dbSet = method.Invoke(_context, null);

            // 3. استدعاء AsNoTracking عبر الـ Reflection أيضاً لأنها Generic Extension Method
            var asNoTrackingMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking))!
                .MakeGenericMethod(type);

            var queryable = asNoTrackingMethod.Invoke(null, new[] { dbSet });

            // 4. الآن يمكننا تحويله إلى IQueryable<object> بأمان
            return ((IEnumerable)queryable!).Cast<object>().AsQueryable();
        }

        public async Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            await _context.Set<TEntity>().AddRangeAsync(entities);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            _context.Set<TEntity>().Update(entity);
        }

        public void SetImportMode(bool isImport)
        {
            _context.IsImportRequest = isImport;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
