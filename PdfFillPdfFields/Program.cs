using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Path = System.IO.Path;

namespace PdfFillPdfFields
{
    internal class Program
    {
        private const string DocxResourceName = "PdfFillPdfFields.Template.Table.docx";
        private const string PdfResourceName = "PdfFillPdfFields.Template.Table.pdf";

        public static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            
            if (IsOfficeInstalled())
            {
                Console.WriteLine("检测到Office，正在处理...");
                var inputDocxPath = ExtractResource(DocxResourceName, "input.docx");
                var inputPdfPath = ConvertDocxToPdf(inputDocxPath, "input.pdf");
                var outputPdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output.pdf");
                FillPdf(inputPdfPath, outputPdfPath);
                Console.WriteLine($"完成！输出文件: {outputPdfPath}");
            }
            else
            {
                Console.WriteLine("未检测到Office，使用模板PDF...");
                var inputPdfPath = ExtractResource(PdfResourceName, "demo_input.pdf");
                var outputPdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "demo_output.pdf");
                FillPdf(inputPdfPath, outputPdfPath);
                Console.WriteLine($"完成！输出文件: {outputPdfPath}");
            }
            
            stopwatch.Stop();
            Console.WriteLine($"总耗时: {stopwatch.ElapsedMilliseconds}ms");
        }

        private static string ExtractResource(string resourceName, string outputFileName)
        {
            var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFileName);
            
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                resourceStream?.CopyTo(fileStream);
            }
            
            Console.WriteLine($"已导出: {outputPath}");
            return outputPath;
        }

        private static bool IsOfficeInstalled()
        {
            try
            {
                var wordType = Type.GetTypeFromProgID("Word.Application");
                if (wordType != null)
                {
                    dynamic word = Activator.CreateInstance(wordType);
                    word.Quit();
                    return true;
                }
            }
            catch
            {
                // ignored
            }

            return false;
        }

        private static string ConvertDocxToPdf(string docxPath, string outputPdfName)
        {
            var pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputPdfName);
            
            try
            {
                var wordType = Type.GetTypeFromProgID("Word.Application");
                if (wordType == null)
                    throw new Exception("无法创建Word应用程序");

                dynamic word = Activator.CreateInstance(wordType);
                word.Visible = false;
                word.DisplayAlerts = false;

                dynamic doc = word.Documents.Open(docxPath);
                doc.SaveAs2(pdfPath, 17);
                doc.Close(false);
                word.Quit();

                Console.WriteLine($"DOCX转换PDF成功: {pdfPath}");
                return pdfPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"转换失败: {ex.Message}");
                throw;
            }
            finally
            {
                if (File.Exists(docxPath))
                    File.Delete(docxPath);
            }
        }

        private static void FillPdf(string inputPdfPath, string outputPdfPath)
        {
            using (var originWriter = new StreamWriter("origin_output.txt"))
            using (var pdfDoc = new PdfDocument(new PdfReader(inputPdfPath), new PdfWriter(outputPdfPath)))
            {
                var form = PdfAcroForm.GetAcroForm(pdfDoc, true);

                var index = -1;
                var numberOfPages = pdfDoc.GetNumberOfPages();
                for (var i = 1; i <= numberOfPages; i++)
                {
                    var page = pdfDoc.GetPage(i);
                
                    var contentStream = page.GetContentStream(0);

                    var bytes = contentStream.GetBytes();
                    var content = Encoding.GetEncoding("ISO-8859-1").GetString(bytes); // 兼容 PDF 内容编码
                
                    originWriter.WriteLine(content);
                
                    var listener = new RectListener();
                    var processor = new PdfCanvasProcessor(listener);
                
                    processor.ProcessPageContent(page);
                
                    var cleaned = CleanRects(listener.Rectangles);
                    foreach (var rect in cleaned)
                    {
                        if (!PdfCellChecker.IsEmptyCell(page, rect))
                        {
                            Console.WriteLine($"({index + 1:D6}:{i:D4}/{numberOfPages:D4})跳过非空单元格：{rect.GetX()} {rect.GetY()} {rect.GetWidth()} {rect.GetHeight()}");
                            continue;
                        }

                        index++;
                        
                        Console.WriteLine($"({index + 1:D6}:{i:D4}/{numberOfPages:D4})处理空白单元格：{rect.GetX()} {rect.GetY()} {rect.GetWidth()} {rect.GetHeight()}");

                        var textField = new TextFormFieldBuilder(pdfDoc, "00" + (index == 0 ? "" : "_" + (index + 1)))
                            .SetPage(i)
                            .SetWidgetRectangle(rect)
                            .CreateText();

                        textField.SetMultiline(true);

                        textField.SetFontSize(0);

                        form.AddField(textField);
                    }
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