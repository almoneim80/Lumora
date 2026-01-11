using Lumora.Application.Interfaces.Infrastructure;

namespace Lumora.Infrastructure.Printing
{
    public class IronPdfGenerator : IPdfGenerator
    {
        public byte[] GenerateFromHtml(string html)
        {
            var renderer = new ChromePdfRenderer();

            renderer.RenderingOptions.MarginTop = 10;
            renderer.RenderingOptions.MarginBottom = 10;

            renderer.RenderingOptions.WaitFor.RenderDelay(50); // للانتظار حتى اكتمال ريندر الـ JS

            using var pdfDocument = renderer.RenderHtmlAsPdf(html);
            return pdfDocument.BinaryData;
        }
    }
}
