namespace Lumora.Application.Interfaces.WebDomainIntf
{
    public interface IWebCheckerService
    {
        // لقراءة ملفات free_domains.txt و disposable_domains.txt
        void VerifyFreeAndDisposable(WebDomain domain);

        // لتغليف مكتبة DnsClient
        Task VerifyDns(WebDomain domain);

        // لتغليف مكتبة HtmlAgilityPack و HttpClient
        Task VerifyHttp(WebDomain domain);

        Task<IEnumerable<string>> GetMxRecordsAsync(string domainName);
    }
}
