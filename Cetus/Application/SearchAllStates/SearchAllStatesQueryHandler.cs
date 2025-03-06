using Cetus.Infrastructure.Persistence.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cetus.Application.SearchAllStates;

public sealed class SearchAllStatesQueryHandler : IRequestHandler<SearchAllStatesQuery, IEnumerable<StateResponse>>
{
    private readonly CetusDbContext _context;

    public SearchAllStatesQueryHandler(CetusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StateResponse>> Handle(SearchAllStatesQuery request,
        CancellationToken cancellationToken)
    {
        var states = await _context.States
            .AsNoTracking()
            .Where(s => s.DeletedAt == null)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        return states.Select(s => new StateResponse(s.Id, s.Name));
    }
}
