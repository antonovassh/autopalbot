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
        PdfPage page = document.AddPage();

        XGraphics gfx = XGraphics.FromPdfPage(page);

        GlobalFontSettings.FontResolver = new CustomFontResolver();
        XFont font = new XFont("Arial", 20);

        gfx.DrawString(insuranse, font, XBrushes.Black,
                new XRect(0, 0, page.Width, page.Height),
                XStringFormats.Center);

        //document.Save(filePath);

        var pdfAsStream = new MemoryStream();

        document.Save(pdfAsStream, false);
        
        return pdfAsStream;
    }
}
