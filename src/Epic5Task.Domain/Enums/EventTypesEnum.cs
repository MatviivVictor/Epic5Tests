using System.Runtime.Serialization;

namespace Epic5Task.Domain.Enums;

public enum EventTypesEnum
{
    [EnumMember(Value = "Other")]Other = 0,
    [EnumMember(Value = "Concert")]Concert = 1,
    [EnumMember(Value = "Conference")]Conference = 2,
    [EnumMember(Value = "Workshop")]Workshop = 3,
}