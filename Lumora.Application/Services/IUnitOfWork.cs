namespace Lumora.Application.Services
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct);
    }

    public interface IUnitOfWorkTransaction : IDisposable, IAsyncDisposable
    {
        Task CommitAsync(CancellationToken ct);
        Task RollbackAsync(CancellationToken ct);
    }
}
