namespace Lumora.Interfaces
{
    public interface IDomainService : IEntityService<Domain>
    {
        public Task Verify(Domain domain);

        public string GetDomainNameByEmail(string email);

        Task<DomainDetailsDto> VerifyDomainAsync(string name, bool force);
    }
}
