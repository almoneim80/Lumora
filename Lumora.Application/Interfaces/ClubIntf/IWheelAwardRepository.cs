namespace Lumora.Application.Interfaces.ClubIntf
{
    public interface IWheelAwardRepository
    {
        Task<WheelAward?> GetByIdAsync(int id, CancellationToken ct);
        Task<PagedResult<WheelAward>> GetAllPagedAsync(PaginationRequestDto pagination, CancellationToken ct);
        Task AddAsync(WheelAward award, CancellationToken ct);
        void Update(WheelAward award);
    }
}
