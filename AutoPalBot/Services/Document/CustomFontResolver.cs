using PdfSharp.Fonts;
using PdfSharp.Quality;

namespace AutoPalBot.Services.Document;

public class CustomFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
    {
        string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts\\" + faceName + ".ttf");

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
