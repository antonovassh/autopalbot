using AutoPalBot.Services.Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPalBot.Models.User;

public class UserModel
{
    public string? PassportNumber { get; set; }

    public string? VehicleNumber { get; set; }

    public BotState BotState { get; set; }
}
