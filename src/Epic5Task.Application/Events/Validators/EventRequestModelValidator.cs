using Epic5Task.Application.Events.Models;
using FluentValidation;

namespace Epic5Task.Application.Events.Validators;

public class EventRequestModelValidator : AbstractValidator<EventRequestModel>
{
    public EventRequestModelValidator()
    {
        RuleFor(x => x.EventTitle).NotEmpty();

        RuleFor(x => x.EventDate.ToDateTime(x.EventTime))
            .GreaterThan(DateTime.Now);

        RuleFor(x => x.EventType).IsInEnum();

        When(x => x.EventCapacities.Length > 0, () =>
        {
            RuleFor(x => x.EventCapacities)
                .Must(items => items.Select(i => i.TicketType).Distinct().Count() == items.Length)
                .WithMessage("TicketType should be unique");

            RuleForEach(x => x.EventCapacities)
                .ChildRules(cap =>
                {
                    cap.RuleFor(x => x.TicketType).IsInEnum();
                    cap.RuleFor(x => x.TicketPrice).GreaterThan(0m);
                    cap.RuleFor(x => x.TicketCapacityLimit).GreaterThan(0);
                });
        });
    }
}