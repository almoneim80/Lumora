using Lumora.Application.Interfaces.ClubIntf;
namespace Lumora.Infrastructure.Repositories
{
    public class WheelAwardRepository(PgDbContext dbContext) : IWheelAwardRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<WheelAward?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _dbContext.WheelAwards
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
        }

        public async Task<PagedResult<WheelAward>> GetAllPagedAsync(PaginationRequestDto pagination, CancellationToken ct)
        {
            var query = _dbContext.WheelAwards
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt);

            return await query.ApplyPaginationAsync(pagination, ct);
        }

        public async Task AddAsync(WheelAward award, CancellationToken ct)
        {
            await _dbContext.WheelAwards.AddAsync(award, ct);
        }

        public void Update(WheelAward award)
        {
            _dbContext.WheelAwards.Update(award);
        }
    }
}
