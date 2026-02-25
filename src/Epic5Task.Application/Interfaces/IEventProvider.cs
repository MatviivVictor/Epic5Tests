using Epic5Task.Application.Events.Models;
using Epic5Task.Domain.AggregateRoot;

namespace Epic5Task.Application.Interfaces;

public interface IEventProvider
{
    int CreateEvent(EventRequestModel model, string owner);
    List<@Event> GetEvents();
    void UpdateEvent(int requestEventId, EventRequestModel requestModel, int userId);
    @Event GetEvent(int eventId);
}