using Lumora.Application.Services;
using Microsoft.EntityFrameworkCore.Storage;

namespace Lumora.Infrastructure.Data
{
    public class EfUnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
    {
        public async Task CommitAsync(CancellationToken ct) => await transaction.CommitAsync(ct);
        public async Task RollbackAsync(CancellationToken ct) => await transaction.RollbackAsync(ct);
        public void Dispose() => transaction.Dispose();
        public async ValueTask DisposeAsync() => await transaction.DisposeAsync();
    }
}
