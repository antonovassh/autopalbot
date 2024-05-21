using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Services
{
    public static class OpenAIService
    {
        public static async Task<string> GeneratePolicyDocument(string _userPassportNumber)
        {
            // Simulate generating a policy document using OpenAI
            await Task.Delay(1000); // Simulate API call delay

            return $"This is your car insurance policy {_userPassportNumber}.";
        }
    }
}
