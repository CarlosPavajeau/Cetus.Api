using System.Text.Json.Serialization;

namespace Infrastructure.Wompi;

public sealed record TransactionData(
    string Id,
    [property: JsonPropertyName("created_at")]
    DateTime? CreatedAt,
    [property: JsonPropertyName("finalized_at")]
    DateTime? FinalizedAt,
    [property: JsonPropertyName("amount_in_cents")]
    long AmountInCents,
    string Status,
    [property: JsonPropertyName("payment_method_type")]
    string PaymentMethodType
);

public record Transaction(TransactionData Data);
