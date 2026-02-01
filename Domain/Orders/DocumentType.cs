using NpgsqlTypes;

namespace Domain.Orders;

public enum DocumentType
{
    [PgName("CC")] CC,
    [PgName("CE")] CE,
    [PgName("NIT")] NIT,
    [PgName("PP")] PP,
    [PgName("OTHER")] OTHER
}
