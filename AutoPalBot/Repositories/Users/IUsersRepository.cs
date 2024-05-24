using AutoPalBot.Models.User;

namespace AutoPalBot.Repositories.Users;

public interface IUsersRepository
{
    UserModel? GetUser(long id);
    void SetUser(long id, UserModel user);
}