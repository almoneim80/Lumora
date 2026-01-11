namespace Lumora.Application.Interfaces.UserIntf
{
    public interface IUserRepository
    {
        //  عمليات البحث الأساسية 
        Task<User?> GetByIdAsync(string userId, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);

        //  التحقق من الوجود
        Task<bool> ExistsByPhoneAsync(string phoneNumber, CancellationToken ct = default);
        Task<bool> ExistsByIdActiveAsync(string userId, CancellationToken ct = default);

        //  عمليات القائمة والترشيح
        Task<PagedResult<ListUsersDto>> GetAllPagedAsync(PaginationRequestDto pagination, bool? isActive, CancellationToken ct = default);

        //  عمليات الحفظ والإنشاء
        Task<bool> CreateAsync(User user);
    }
}
