using AwesomeAssertions;
using Epic5Task.Domain.Entities;
using Epic5Task.Infrastructure.DataContext;
using Epic5Task.Infrastructure.DataProviders;
using NUnit.Framework;

namespace Epic5Task.Infrastructure.Tests.DataProviders;

[TestFixture]
public class UserProviderTests
{
    private UserProvider _userProvider;

    [SetUp]
    public void SetUp()
    {
        Data.Users.Clear();
        _userProvider = new UserProvider();
    }

    [Test]
    public void GetUserId_WhenUserExists_ShouldReturnExistingUserId()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var existingUserId = Data.AddUser(new UserEntity { PhoneNumber = phoneNumber });

        // Act
        var result = _userProvider.GetUserId(phoneNumber);

        // Assert
        result.Should().Be(existingUserId);
        Data.Users.Should().HaveCount(1);
    }

    [Test]
    public void GetUserId_WhenUserDoesNotExist_ShouldCreateNewUserAndReturnNewId()
    {
        // Arrange
        var phoneNumber = "+0987654321";

        // Act
        var result = _userProvider.GetUserId(phoneNumber);

        // Assert
        result.Should().Be(1); // First user added to cleared Data.Users
        Data.Users.Should().HaveCount(1);
        Data.Users[0].PhoneNumber.Should().Be(phoneNumber);
        Data.Users[0].UserId.Should().Be(result);
    }
}
