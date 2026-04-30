using System.Reflection;
using iText.Forms;
using iText.Forms.Fields;
using iText.Forms.Xfdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;

namespace PdfFillPdfFields
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PdfFillPdfFields.Template.Table.pdf"))
            using (var pdfDoc = new PdfDocument(new PdfReader(resourceStream), new PdfWriter("output.pdf")))
            {
                var form = PdfAcroForm.GetAcroForm(pdfDoc, true);

                var page = pdfDoc.GetPage(1);

                float x = 90.504f;
                float y = 698.98f;
                float width = 219.02f;
                float height = 70.464f;

                var textField = new TextFormFieldBuilder(pdfDoc, "CellText")
                    .SetPage(1)
                    .SetWidgetRectangle(new Rectangle(x, y, width, height))
                    .CreateText();
                
                form.AddField(textField);

                pdfDoc.Close();
            }
        }
    }
}