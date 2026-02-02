using SharedKernel;

namespace Domain.States;

public static class CityErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("City.NotFound", $"The city with id '{id}' was not found.");
}
