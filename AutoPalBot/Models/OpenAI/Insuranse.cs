using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Models.OpenAI
{
    public class Insuranse
    {
        public static string Format = "{\"Title\":string,\"Description\":string}";

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
