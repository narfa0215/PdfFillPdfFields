using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using iText.Forms;
using iText.Forms.Fields;
using iText.Forms.Xfdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Data;

namespace PdfFillPdfFields
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PdfFillPdfFields.Template.Table.pdf"))
            using (var originWriter = new StreamWriter("origin_output.txt"))
            using (var pdfDoc = new PdfDocument(new PdfReader(resourceStream), new PdfWriter("output.pdf")))
            {
                var form = PdfAcroForm.GetAcroForm(pdfDoc, true);

                var page = pdfDoc.GetPage(1);
                
                var contentStream = page.GetContentStream(0);

                var bytes = contentStream.GetBytes();
                var content = Encoding.GetEncoding("ISO-8859-1").GetString(bytes); // 兼容 PDF 内容编码
                
                originWriter.WriteLine(content);
                
                var listener = new RectListener();
                var processor = new PdfCanvasProcessor(listener);
                
                processor.ProcessPageContent(page);
                
                var cleaned = CleanRects(listener.Rectangles);
                
                Console.WriteLine("=== 真实表格单元格 ===");
                foreach (var r in cleaned)
                {
                    Console.WriteLine($"{r.GetX()} {r.GetY()} {r.GetWidth()} {r.GetHeight()}");

                    float x = r.GetX();
                    float y = r.GetY();
                    float width = r.GetWidth();
                    float height = r.GetHeight();

                    var textField = new TextFormFieldBuilder(pdfDoc, "CellText")
                        .SetPage(1)
                        .SetWidgetRectangle(new Rectangle(x, y, width, height))
                        .CreateText();
                
                    form.AddField(textField);
                }
            }
        }
        
        static List<Rectangle> CleanRects(List<Rectangle> input, float pageWidth = 595f, float pageHeight = 842f)
        {
            return input
                .Where(r =>
                {
                    float w = r.GetWidth();
                    float h = r.GetHeight();
                    float x = r.GetX();
                    float y = r.GetY();

                    // 1. 去掉极小线条（边框）
                    if (w < 5 || h < 5)
                        return false;

                    // 2. 去掉整页背景（关键）
                    if (w > pageWidth * 0.9f && h > pageHeight * 0.9f)
                        return false;

                    // 3. 去掉贴边页面矩形（常见 artifact）
                    if (x < 1 && y < 1 && w > 500 && h > 500)
                        return false;

                    return true;
                })
                // 去重
                .GroupBy(r => new
                {
                    X = Math.Round(r.GetX(), 1),
                    Y = Math.Round(r.GetY(), 1),
                    W = Math.Round(r.GetWidth(), 1),
                    H = Math.Round(r.GetHeight(), 1)
                })
                .Select(g => g.First())
                .ToList();
        }
    }
}