using System.Linq.Expressions;
using Domain.States;

namespace Application.States.FindCity;

public record CityResponse(Guid Id, string Name, string State)
{
    public static Expression<Func<City, CityResponse>> Map => city =>
        new CityResponse(city.Id, city.Name, city.State!.Name);
}
