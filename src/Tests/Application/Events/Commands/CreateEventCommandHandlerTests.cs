using AwesomeAssertions;
using Epic5Task.Application.Events.Commands;
using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Interfaces;
using Epic5Task.Domain.Enums;
using Epic5Task.Infrastructure.DataContext;
using Epic5Task.Infrastructure.DataProviders;
using Moq;
using NUnit.Framework;

namespace Epic5Task.Tests.Application.Events.Commands;

[TestFixture]
public class CreateEventCommandHandlerTests
{
    private Mock<IUserContextProvider> _userContextProviderMock;
    private IEventProvider _eventProvider;
    private IUserProvider _userProvider;
    private CreateEventCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        // Clear in-memory data before each test
        Data.Events.Clear();
        Data.Users.Clear();
        Data.EventCapacities.Clear();
        Data.Tickets.Clear();
        Data.TicketStatusHistory.Clear();

        _userContextProviderMock = new Mock<IUserContextProvider>();

        // We use the real UserProvider and EventProvider for integration testing
        _userProvider = new UserProvider();
        _eventProvider = new EventProvider(_userProvider);

        _handler = new CreateEventCommandHandler(_userContextProviderMock.Object, _eventProvider);
    }

    [Test]
    public async Task Handle_ShouldCreateEventAndCapacities_WhenRequestIsValid()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var model = new EventRequestModel
        {
            EventTitle = "Integration Test Event",
            EventDate = new DateOnly(2026, 10, 10),
            EventTime = new TimeOnly(19, 0),
            EventType = EventTypesEnum.Concert,
            EventCapacities = new[]
            {
                new EventCapacityRequestModel
                {
                    TicketType = TicketTypesEnum.Regular,
                    TicketPrice = 100,
                    TicketCapacityLimit = 50
                },
                new EventCapacityRequestModel
                {
                    TicketType = TicketTypesEnum.VIP,
                    TicketPrice = 250,
                    TicketCapacityLimit = 10
                }
            }
        };

        var command = new CreateEventCommand(model);

        // Act
        var eventId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        eventId.Should().BeGreaterThan(0);

        // Verify data in DataContext
        Data.Events.Should().HaveCount(1);
        var createdEvent = Data.Events.Single();
        createdEvent.EventId.Should().Be(eventId);
        createdEvent.EventTitle.Should().Be(model.EventTitle);
        createdEvent.EventDate.Should().Be(model.EventDate);
        createdEvent.EventTime.Should().Be(model.EventTime);
        createdEvent.EventType.Should().Be(model.EventType);

        // Verify owner (UserProvider should have created the user)
        Data.Users.Should().HaveCount(1);
        var user = Data.Users.Single();
        user.PhoneNumber.Should().Be(phoneNumber);
        createdEvent.EventOwner.Should().Be(user.UserId);

        // Verify capacities
        Data.EventCapacities.Should().HaveCount(2);
        Data.EventCapacities.Should().Contain(x =>
            x.EventId == eventId && x.TicketType == TicketTypesEnum.Regular && x.TicketPrice == 100 &&
            x.TicketCapacityLimit == 50);
        Data.EventCapacities.Should().Contain(x =>
            x.EventId == eventId && x.TicketType == TicketTypesEnum.VIP && x.TicketPrice == 250 &&
            x.TicketCapacityLimit == 10);
    }

    [Test]
    public async Task Handle_ShouldUseExistingUser_WhenUserAlreadyExists()
    {
        // Arrange
        var phoneNumber = "+380991112233";
        var existingUserId = Data.AddUser(new Epic5Task.Domain.Entities.UserEntity { PhoneNumber = phoneNumber });

        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var model = new EventRequestModel
        {
            EventTitle = "Event for existing user",
            EventDate = new DateOnly(2026, 12, 12),
            EventTime = new TimeOnly(20, 0),
            EventType = EventTypesEnum.Workshop,
            EventCapacities = []
        };

        var command = new CreateEventCommand(model);

        // Act
        var eventId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Data.Events.Should().HaveCount(1);
        var createdEvent = Data.Events.Single();
        createdEvent.EventOwner.Should().Be(existingUserId);
        Data.Users.Should().HaveCount(1); // No new user created
    }
}