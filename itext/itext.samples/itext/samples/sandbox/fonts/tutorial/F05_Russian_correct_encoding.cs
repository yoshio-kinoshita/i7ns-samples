using System;
using System.IO;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace iText.Samples.Sandbox.Fonts.Tutorial
{
    public class F05_Russian_correct_encoding
    {
        public static readonly String DEST = "results/sandbox/fonts/tutorial/f05_russian_encoding.pdf";

        public static readonly String FONT = "../../../resources/font/FreeSans.ttf";

        public static void Main(String[] args)
        {
            FileInfo file = new FileInfo(DEST);
            file.Directory.Create();

            new F05_Russian_correct_encoding().ManipulatePdf(DEST);
        }

        protected void ManipulatePdf(String dest)
        {
            PdfDocument pdfDoc = new PdfDocument(new PdfWriter(dest));
            Document doc = new Document(pdfDoc);

            // CP1251 encoding type supports russian characters
            PdfFont font = PdfFontFactory.CreateFont(FONT, "Cp1251", 
                PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            doc.SetFont(font);

            // The text line is "Откуда ты?"
            doc.Add(new Paragraph("\u041e\u0442\u043a\u0443\u0434\u0430 \u0442\u044b?"));

            // The text line is "Увидимся позже. Увидимся."
            doc.Add(new Paragraph("\u0423\u0432\u0438\u0434\u0438\u043c\u0441\u044f "
                                  + "\u043f\u043E\u0437\u0436\u0435. \u0423\u0432\u0438\u0434\u0438\u043c\u0441\u044f."));

            // The text line is "Позвольте мне представиться."
            doc.Add(new Paragraph("\u041f\u043e\u0437\u0432\u043e\u043b\u044c\u0442\u0435 \u043c\u043d\u0435 "
                                  + "\u043f\u0440\u0435\u0434\u0441\u0442\u0430\u0432\u0438\u0442\u044c\u0441\u044f."));

            // The text line is "Это студент."
            doc.Add(new Paragraph("\u042d\u0442\u043e \u0441\u0442\u0443\u0434\u0435\u043d\u0442."));

            // The text line is "Хорошо?"
            doc.Add(new Paragraph("\u0425\u043e\u0440\u043e\u0448\u043e?"));

            // The text line is "Он инженер. Она доктор."
            doc.Add(new Paragraph("\u041e\u043d \u0438\u043d\u0436\u0435\u043d\u0435\u0440. "
                                  + "\u041e\u043d\u0430 \u0434\u043e\u043a\u0442\u043e\u0440."));

            // The text line is "Это окно."
            doc.Add(new Paragraph("\u042d\u0442\u043e \u043e\u043a\u043d\u043e."));

            // The text line is "Повторите, пожалуйста."
            doc.Add(new Paragraph("\u041f\u043e\u0432\u0442\u043e\u0440\u0438\u0442\u0435, "
                                  + "\u043f\u043e\u0436\u0430\u043b\u0443\u0439\u0441\u0442\u0430."));

            doc.Close();
        }
    }
}