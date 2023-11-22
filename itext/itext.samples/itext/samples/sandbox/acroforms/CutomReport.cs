using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace itext.samples.itext.samples.sandbox.acroforms
{
    public class CutomReport
    {
        public static readonly String DEST = "results/sandbox/fonts/report.pdf";
        public static readonly String SRC = "../../../resources/custom/custom.pdf";


        public static readonly String FONT = "../../../resources/font/NotoSansCJKsc-Regular.otf";


        public static void Main(String[] args)
        {
            FileInfo file = new FileInfo(DEST);
            file.Directory.Create();

            new CutomReport().ManipulatePdf(DEST);
        }

        protected void ManipulatePdf(String dest)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(SRC), new PdfWriter(dest));
            Document doc = new Document(pdfDoc);

            PdfFont font = PdfFontFactory.CreateFont(FONT, PdfEncodings.IDENTITY_H);

            // 1ページ目を取得
            PdfPage page = pdfDoc.GetPage(1);

            // ページのサイズを取得
            Rectangle pageSize = page.GetPageSize();

            // 幅と高さを取得
            float width = pageSize.GetWidth();
            float height = pageSize.GetHeight();

            // 幅と高さを表示
            Console.WriteLine($"Width: {width}");
            Console.WriteLine($"Height: {height}");

            doc.SetFont(font);

            Paragraph p = new Paragraph("ああああああああああああ").SetFontSize(8).SetFixedPosition(3, 65, 400, 100); // ページ番号, x, y, 幅を指定
            doc.Add(p);
            doc.Close();
        }

    }
}
