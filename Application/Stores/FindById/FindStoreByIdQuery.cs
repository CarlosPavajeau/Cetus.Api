using Application.Abstractions.Messaging;

namespace Application.Stores.FindById;

public sealed record FindStoreByIdQuery(Guid Id) : IQuery<SimpleStoreResponse>;
