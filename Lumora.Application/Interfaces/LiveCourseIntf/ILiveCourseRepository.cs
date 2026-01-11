namespace Lumora.Application.Interfaces.LiveCourseIntf
{
    public interface ILiveCourseRepository
    {
        Task<LiveCourse?> GetByIdAsync(int id, CancellationToken ct, bool includeSubscribers = false);
        Task AddAsync(LiveCourse liveCourse, CancellationToken ct);
        IQueryable<LiveCourse> GetQueryable();
        Task<bool> AnyAsync(int id, CancellationToken ct);
        Task<bool> IsUserSubscribedAsync(string userId, int courseId, CancellationToken ct);
        void Update(LiveCourse liveCourse);
        Task<bool> IsPaymentValidAsync(int courseId, string userId, int paymentItemId, CancellationToken ct);
        Task AddSubscriptionAsync(UserLiveCourse subscription, CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
        IQueryable<UserLiveCourse> GetUserEnrollmentsQueryable(string userId);
        IQueryable<UserLiveCourse> GetSubscribersQueryable(int courseId);
        Task<(List<LiveCourse> Items, int TotalCount)> GetPagedListAsync(bool? isActive, string? keyword, int pageNumber, int pageSize, CancellationToken ct);
        Task<PagedResult<UserLiveCourse>> GetUserEnrollmentsPagedAsync(string userId, PaginationRequestDto pagination, CancellationToken ct);
        Task<(List<UserLiveCourse> Items, int TotalCount)> GetSubscribersPagedAsync(int courseId, int skip, int pageSize, CancellationToken ct);
    }
}
