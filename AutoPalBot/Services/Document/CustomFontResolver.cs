using PdfSharp.Fonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Services.Document
{
    public class CustomFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            
            string fontPath = Path.Combine("C:/Windows/Fonts", faceName + ".ttf"); // Adjust the font directory as necessary
            if (File.Exists(fontPath))
                return File.ReadAllBytes(fontPath);
            else
                throw new FileNotFoundException($"Font file not found: {fontPath}");
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo(familyName);
        }
    }
}
