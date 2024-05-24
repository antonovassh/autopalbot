using AutoPalBot.Models.User;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace AutoPalBot.Repositories.Users;

public class UsersRepository : IUsersRepository
{
    private readonly IMemoryCache _memoryCache;

    public UsersRepository(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public UserModel? GetUser(long id)
    {
        var user = _memoryCache.Get(id);

        if (user != null)
        {
            return (UserModel?)user;
        }

        return null;
    }

    public void SetUser(long id, UserModel user)
    {
        //TODO: 10 should be passed with IOptions to be configured.
        _memoryCache.Set(id, user, DateTimeOffset.Now.AddDays(10));
    }
}
