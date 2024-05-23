using AutoPalBot.Models.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Services.DocumentGenerator;

public interface IDocumentService
{
    Stream GenerateDocument(string insuranse);
}
