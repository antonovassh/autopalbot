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
    public void GenerateDocument(Insuranse insuranse)
    {
       
        PdfDocument document = new PdfDocument();
        PdfPage page = document.AddPage();

        XGraphics gfx = XGraphics.FromPdfPage(page);

        GlobalFontSettings.FontResolver = new CustomFontResolver();
        XFont font = new XFont("Arial", 20);

        //gfx.DrawString(text, font, XBrushes.Black,
        //        new XRect(0, 0, page.Width, page.Height),
        //        XStringFormats.Center);

        string filePath = @"C:\Users\Олександра\Desktop\AutoPalBot\HelloWorld.pdf";

        document.Save(filePath);
        document.Close();
    }
}
