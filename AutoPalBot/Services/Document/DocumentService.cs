using AutoPalBot.Services.Document;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace AutoPalBot.Services.DocumentGenerator;

public class DocumentService : IDocumentService
{
    public DocumentService()
    {
        GlobalFontSettings.FontResolver = new CustomFontResolver();
    }

    public Stream GenerateDocument(string insuranse)
    {
       
        PdfDocument document = new();
        document.Info.Title = "Car insuranse";

        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);
       
        //TODO: Magic numbers to consts
        XFont font = new("Arial", 8);

        //TODO: Magic numbers to consts
        double margin = 20;
        double widthLimit = page.Width - 2 * margin;
        double yPosition = margin;

        string[] paragraphs = insuranse.Split('\n');

        foreach (string paragraphText in paragraphs)
        {
            string[] lines = SplitTextIntoLines(gfx, paragraphText, font, widthLimit);

            foreach (string line in lines)
            {
                gfx.DrawString(line, font, XBrushes.Black,
                    new XRect(margin, yPosition, page.Width - 2 * margin, page.Height),
                    XStringFormats.TopLeft);
                //TODO: Magic numbers to consts
                yPosition += font.Height + 2; 
            }

            //TODO: Magic numbers to consts
            yPosition += font.Height + 2;
        }
        var pdfAsStream = new MemoryStream();

        document.Save(pdfAsStream, false);
        
        return pdfAsStream;
    }

    private string[] SplitTextIntoLines(XGraphics gfx, string text, XFont font, double widthLimit)
    {
        var lines = new List<string>();
        var words = text.Split(' ');

        string currentLine = "";
        foreach (var word in words)
        {
            var testLine = currentLine + (currentLine.Length > 0 ? " " : "") + word;
            var size = gfx.MeasureString(testLine, font);

            if (size.Width < widthLimit)
            {
                currentLine = testLine;
            }
            else
            {
                lines.Add(currentLine);
                currentLine = word;
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine);
        }

        return lines.ToArray();
    }
}
