using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferType
{
    TransferTo,
    TransferFrom
}