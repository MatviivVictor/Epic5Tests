using Epic5Task.Application.Interfaces;
using Epic5Task.Domain.Entities;

namespace Epic5Task.Infrastructure.DataProviders;

public class UserProvider: IUserProvider
{
    public int GetUserId(string phoneNumber)
    {
        var user = DataContext.Data.Users.FirstOrDefault(x => x.PhoneNumber == phoneNumber);
        return user?.UserId ?? DataContext.Data.AddUser(new UserEntity { PhoneNumber = phoneNumber });
    }
}