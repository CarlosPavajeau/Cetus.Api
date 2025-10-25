using Application.Abstractions.Messaging;

namespace Application.Stores.ConfigureWompi;

public sealed record ConfigureWompiCommand(string PublicKey, string PrivateKey, string EventsKey, string IntegrityKey)
    : ICommand;
