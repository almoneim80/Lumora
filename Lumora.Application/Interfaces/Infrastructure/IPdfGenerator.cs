namespace Lumora.Application.Interfaces.Infrastructure
{
    public interface IPdfGenerator
    {
        byte[] GenerateFromHtml(string html);
    }
}
