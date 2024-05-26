namespace AutoPalBot.Services.Bot;

public enum BotState
{
    StartingConversation,
    AwaitingPassportNumber,
    AwaitingVehicleNumber,
    AwaitingDataConfirmation,
    AwaitingPriceConfirmation,
    AwaitingForQuestion
}
