using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace PdfFillPdfFields
{
    public class PdfCellChecker
    {
        public static bool IsEmptyCell(PdfPage page, Rectangle rect)
        {
            // 1. 限定区域
            var filter = new TextRegionEventFilter(rect);

            // 2. 文本提取策略
            var strategy = new FilteredTextEventListener(
                new LocationTextExtractionStrategy(),
                filter
            );

            // 3. 提取文本
            string text = PdfTextExtractor.GetTextFromPage(page, strategy);

            // 4. 判断是否为空
            return string.IsNullOrWhiteSpace(text);
        }
    }
}