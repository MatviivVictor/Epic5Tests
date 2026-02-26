using Epic5Task.Application.Interfaces;

namespace Epic5Task.Application.Commons.Providers;

public class UserContextProvider: IUserContextProvider
{
    public string UserPhoneNumber { get; set; }
}