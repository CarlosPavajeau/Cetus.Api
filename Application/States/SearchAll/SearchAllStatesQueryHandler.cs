using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.States.SearchAll;

internal sealed class SearchAllStatesQueryHandler(IApplicationDbContext context)
    : IQueryHandler<SearchAllStatesQuery, IEnumerable<StateResponse>>
{
    public async Task<Result<IEnumerable<StateResponse>>> Handle(SearchAllStatesQuery request,
        CancellationToken cancellationToken)
    {
        var states = await context.States
            .AsNoTracking()
            .Where(s => s.DeletedAt == null)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return Result.Success(states.Select(s => new StateResponse(s.Id, s.Name)));
    }
}
