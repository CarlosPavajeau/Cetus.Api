using System.Text.Json.Serialization;

namespace Cetus.Api.Requests;

public sealed record WompiTransaction(
    string Id,
    string Reference,
    string Status,
    [property: JsonPropertyName("amount_in_cents")]
    decimal AmountInCents);

public sealed record WompiData(WompiTransaction Transaction);

public sealed record WompiSignature(IEnumerable<string> Properties, string Checksum);

public sealed record WompiRequest(
    string Event,
    WompiData Data,
    string Environment,
    WompiSignature Signature,
    long Timestamp);
