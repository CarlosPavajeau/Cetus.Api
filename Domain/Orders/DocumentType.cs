using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace Domain.Orders;

public enum DocumentType
{
    [PgName("CC"), JsonStringEnumMemberName("CC")]
    CC,

    [PgName("CE"), JsonStringEnumMemberName("CE")]
    CE,

    [PgName("NIT"), JsonStringEnumMemberName("NIT")]
    NIT,

    [PgName("PP"), JsonStringEnumMemberName("PP")]
    PP,

    [PgName("OTHER"), JsonStringEnumMemberName("OTHER")]
    OTHER
}
