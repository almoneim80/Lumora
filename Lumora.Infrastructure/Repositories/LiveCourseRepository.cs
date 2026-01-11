namespace Lumora.Infrastructure.Repositories
{
    public class LiveCourseRepository(PgDbContext dbContext) : ILiveCourseRepository
    {
        private readonly PgDbContext _dbContext = dbContext;

        public async Task<LiveCourse?> GetByIdAsync(int id, CancellationToken ct, bool includeSubscribers = false)
        {
            var query = _dbContext.LiveCourses.AsQueryable();

            if (includeSubscribers)
            {
                query = query.Include(c => c.UserLiveCourses)
                             .ThenInclude(ulc => ulc.User)
                             .Include(c => c.UserLiveCourses)
                             .ThenInclude(ulc => ulc.PaymentItem)
                                .ThenInclude(p => p!.Payment);
            }

            return await query.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        }

        public async Task AddAsync(LiveCourse liveCourse, CancellationToken ct)
        {
            await _dbContext.LiveCourses.AddAsync(liveCourse, ct);
        }

        public IQueryable<LiveCourse> GetQueryable()
        {
            return _dbContext.LiveCourses.AsNoTracking().Where(c => !c.IsDeleted);
        }

        public async Task<bool> AnyAsync(int id, CancellationToken ct)
        {
            return await _dbContext.LiveCourses.AnyAsync(c => c.Id == id && !c.IsDeleted, ct);
        }

        public async Task<bool> IsUserSubscribedAsync(string userId, int courseId, CancellationToken ct)
        {
            return await _dbContext.UserLiveCourses
                .AnyAsync(x => x.UserId == userId && x.LiveCourseId == courseId, ct);
        }

        public void Update(LiveCourse liveCourse)
        {
            _dbContext.LiveCourses.Update(liveCourse);
        }

        public async Task<bool> IsPaymentValidAsync(int courseId, string userId, int paymentItemId, CancellationToken ct)
        {
            return await _dbContext.PaymentItems
                .AsNoTracking()
                .AnyAsync(p =>
                    p.Id == paymentItemId &&
                    p.ItemType == PaymentItemType.LiveCourse &&
                    p.ItemId == courseId &&
                    p.Payment.Status == PaymentStatus.Paid &&
                    p.Payment.UserId == userId,
                    ct);
        }

        public async Task AddSubscriptionAsync(UserLiveCourse subscription, CancellationToken ct)
        {
            await _dbContext.UserLiveCourses.AddAsync(subscription, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _dbContext.SaveChangesAsync(ct);
        }

        public IQueryable<UserLiveCourse> GetUserEnrollmentsQueryable(string userId)
        {
            return _dbContext.UserLiveCourses
                .AsNoTracking()
                .Include(x => x.LiveCourse)
                .Where(x => x.UserId == userId
                            && !x.IsDeleted
                            && x.LiveCourse.IsActive)
                .OrderByDescending(x => x.RegisteredAt);
        }

        public IQueryable<UserLiveCourse> GetSubscribersQueryable(int courseId)
        {
            return _dbContext.UserLiveCourses
                .AsNoTracking()
                .Where(x => x.LiveCourseId == courseId)
                .Include(x => x.User)
                .Include(x => x.PaymentItem)
                    .ThenInclude(p => p!.Payment);
        }

        public async Task<(List<LiveCourse> Items, int TotalCount)> GetPagedListAsync(bool? isActive, string? keyword, int pageNumber, int pageSize, CancellationToken ct)
        {
            var query = _dbContext.LiveCourses.AsNoTracking().Where(c => !c.IsDeleted);
            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                query = query.Where(c => c.Title.Contains(k) || c.Description.Contains(k));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<PagedResult<UserLiveCourse>> GetUserEnrollmentsPagedAsync(string userId, PaginationRequestDto pagination, CancellationToken ct)
        {
            var query = _dbContext.UserLiveCourses
                        .AsNoTracking()
                        .Include(x => x.LiveCourse)
                        .Where(x => x.UserId == userId
                                    && !x.IsDeleted
                                    && x.LiveCourse.IsActive)
                        .OrderByDescending(x => x.RegisteredAt);

            return await query.ApplyPaginationAsync(pagination, ct);
        }

        public async Task<(List<UserLiveCourse> Items, int TotalCount)> GetSubscribersPagedAsync(int courseId, int skip, int pageSize, CancellationToken ct)
        {
            var query = _dbContext.UserLiveCourses
                .AsNoTracking()
                .Where(x => x.LiveCourseId == courseId)
                .Include(x => x.User);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.RegisteredAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
