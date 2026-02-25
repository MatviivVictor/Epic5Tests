namespace Epic5Task.Application.Interfaces;

public interface IUserProvider
{
    int GetUserId(string phoneNumber);
}