using Application.Abstractions.Messaging;

namespace Application.States.FindCity;

public sealed record FindCityQuery(Guid Id) : IQuery<CityResponse>;
