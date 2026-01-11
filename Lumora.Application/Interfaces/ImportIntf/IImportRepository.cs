namespace Lumora.Application.Interfaces.ImportIntf
{
    public interface IImportRepository : IDisposable
    {
        // للتعامل مع الكيانات العامة T
        IQueryable<TEntity> GetQueryable<TEntity>() where TEntity : class;

        // للتعامل مع الأنواع الديناميكية (Runtime Types) المستخرجة من الـ Attributes
        IQueryable<object> GetDynamicQueryable(Type type);

        // إضافة وتحديث الكيانات
        Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        void Update<TEntity>(TEntity entity) where TEntity : class;

        // التحكم في حالة الاستيراد (مثلاً لتعطيل بعض الـ Triggers أو التتبع)
        void SetImportMode(bool isImport);

        // حفظ التغييرات نهائياً
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
