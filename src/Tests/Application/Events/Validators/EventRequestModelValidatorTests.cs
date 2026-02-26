using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Events.Validators;
using Epic5Task.Domain.Enums;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Epic5Task.Tests.Application.Events.Validators;

[TestFixture]
public class EventRequestModelValidatorTests
{
    private EventRequestModelValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new EventRequestModelValidator();
    }

    [Test]
    public void Should_Have_Error_When_EventTitle_Is_Empty()
    {
        var model = new EventRequestModel { EventTitle = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.EventTitle);
    }

    [Test]
    public void Should_Have_Error_When_EventDate_Is_In_Past()
    {
        var model = new EventRequestModel
        {
            EventTitle = "Title",
            EventDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            EventTime = TimeOnly.FromDateTime(DateTime.Now)
        };
        var result = _validator.TestValidate(model);
        // Let's check for empty string or see what it actually is in the failure if it still fails
        result.ShouldHaveValidationErrorFor("");
    }

    [Test]
    public void Should_Not_Have_Error_When_EventDate_Is_In_Future()
    {
        var model = new EventRequestModel
        {
            EventTitle = "Title",
            EventDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            EventTime = TimeOnly.FromDateTime(DateTime.Now),
            EventType = EventTypesEnum.Concert
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor("");
    }

    [Test]
    public void Should_Have_Error_When_EventType_Is_Invalid()
    {
        var model = new EventRequestModel { EventType = (EventTypesEnum)999 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.EventType);
    }

    [Test]
    public void Should_Have_Error_When_TicketTypes_Are_Not_Unique()
    {
        var model = new EventRequestModel
        {
            EventCapacities = new[]
            {
                new EventCapacityRequestModel { TicketType = TicketTypesEnum.Regular, TicketPrice = 10, TicketCapacityLimit = 100 },
                new EventCapacityRequestModel { TicketType = TicketTypesEnum.Regular, TicketPrice = 20, TicketCapacityLimit = 50 }
            }
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.EventCapacities)
            .WithErrorMessage("TicketType should be unique");
    }

    [Test]
    public void Should_Have_Errors_When_Capacity_Properties_Are_Invalid()
    {
        var model = new EventRequestModel
        {
            EventCapacities = new[]
            {
                new EventCapacityRequestModel { TicketType = (TicketTypesEnum)999, TicketPrice = 0, TicketCapacityLimit = 0 }
            }
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor("EventCapacities[0].TicketType");
        result.ShouldHaveValidationErrorFor("EventCapacities[0].TicketPrice");
        result.ShouldHaveValidationErrorFor("EventCapacities[0].TicketCapacityLimit");
    }

    [Test]
    public void Should_Not_Have_Errors_When_Model_Is_Valid()
    {
        var model = new EventRequestModel
        {
            EventTitle = "Valid Event",
            EventDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Workshop,
            EventCapacities = new[]
            {
                new EventCapacityRequestModel { TicketType = TicketTypesEnum.Regular, TicketPrice = 100, TicketCapacityLimit = 100 },
                new EventCapacityRequestModel { TicketType = TicketTypesEnum.VIP, TicketPrice = 200, TicketCapacityLimit = 20 }
            }
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
