using System.Runtime.Serialization;

namespace Epic5Task.Domain.Enums;

public enum TicketTypesEnum
{
    [EnumMember(Value = "Regular")]Regular = 0,
    [EnumMember(Value = "VIP")]VIP = 1,
    [EnumMember(Value = "Student")]Student = 2
}