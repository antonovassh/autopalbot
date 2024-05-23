using AutoPalBot.Models.OpenAI;
using AutoPalBot.Services.Document;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Services.DocumentGenerator;

public class DocumentService : IDocumentService
{
    public Stream GenerateDocument(string insuranse)
    {
       
        PdfDocument document = new PdfDocument();
        document.Info.Title = "Car insuranse";

        PdfPage page = document.AddPage();
        XGraphics gfx = XGraphics.FromPdfPage(page);

        GlobalFontSettings.FontResolver = new CustomFontResolver();
        XFont font = new XFont("Verdana", 10);

        double margin = 40;
        double widthLimit = page.Width - 2 * margin;
        double yPosition = margin;

        string[] paragraphs = insuranse.Split('\n');

        // Iterate through each paragraph
        foreach (string paragraphText in paragraphs)
        {
            // Split the paragraph text into lines
            string[] lines = SplitTextIntoLines(gfx, paragraphText, font, widthLimit);

            // Add each line of the paragraph to the document
            foreach (string line in lines)
            {
                gfx.DrawString(line, font, XBrushes.Black,
                    new XRect(margin, yPosition, page.Width - 2 * margin, page.Height),
                    XStringFormats.TopLeft);
                yPosition += font.Height + 5; // Move to the next line with a small margin
            }

            // Add a newline after the paragraph
            yPosition += font.Height + 5;
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
