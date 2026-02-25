using System.Runtime.Serialization;

namespace Epic5Task.Domain.Enums;

public enum TicketStatusesEnum
{
    [EnumMember(Value = "Pending")]Pending = 1,
    [EnumMember(Value = "Confirmed")]Confirmed = 2,
    [EnumMember(Value = "Cancelled")]Cancelled = 3,
    [EnumMember(Value = "Expired")]Expired = 4
    
}