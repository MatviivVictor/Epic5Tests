using Epic5Task.Application.Events.Commands;
using Epic5Task.Application.Events.Models;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Epic5Task.Tests.Application.Events.Validators;

[TestFixture]
public class UpdateEventCommandValidatorTests
{
    private UpdateEventCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateEventCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_Model_Is_Null()
    {
        var command = new UpdateEventCommand(1, null!);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Model);
    }

    [Test]
    public void Should_Have_Errors_When_Model_Is_Invalid()
    {
        var command = new UpdateEventCommand(1, new EventRequestModel { EventTitle = "" });
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Model.EventTitle");
    }
}
