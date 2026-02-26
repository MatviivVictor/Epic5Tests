using Epic5Task.Application.Events.Commands;
using Epic5Task.Application.Events.Models;
using Epic5Task.Domain.Enums;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Epic5Task.Tests.Application.Events.Validators;

[TestFixture]
public class CreateTicketsCommandValidatorTests
{
    private CreateTicketsCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateTicketsCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_EventId_Is_Zero_Or_Negative()
    {
        var command0 = new CreateTicketsCommand(0, new CreateTicketsRequestModel());
        var result0 = _validator.TestValidate(command0);
        result0.ShouldHaveValidationErrorFor(x => x.EventId);

        var commandNeg = new CreateTicketsCommand(-1, new CreateTicketsRequestModel());
        var resultNeg = _validator.TestValidate(commandNeg);
        resultNeg.ShouldHaveValidationErrorFor(x => x.EventId);
    }

    [Test]
    public void Should_Have_Error_When_Model_Is_Null()
    {
        var command = new CreateTicketsCommand(1, null!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Model);
    }

    [Test]
    public void Should_Have_Errors_When_Tickets_Are_Invalid()
    {
        var command = new CreateTicketsCommand(1, new CreateTicketsRequestModel
        {
            Tickets = new[]
            {
                new BookingTicketsModel { TicketType = (TicketTypesEnum)999, Quantity = 0 }
            }
        });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Model.Tickets[0].TicketType");
        result.ShouldHaveValidationErrorFor("Model.Tickets[0].Quantity");
    }

    [Test]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var command = new CreateTicketsCommand(1, new CreateTicketsRequestModel
        {
            Tickets = new[]
            {
                new BookingTicketsModel { TicketType = TicketTypesEnum.Regular, Quantity = 2 },
                new BookingTicketsModel { TicketType = TicketTypesEnum.VIP, Quantity = 1 }
            }
        });
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
