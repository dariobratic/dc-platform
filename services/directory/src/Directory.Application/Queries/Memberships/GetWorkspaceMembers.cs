using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Queries.Memberships;

public sealed record GetWorkspaceMembersQuery(Guid WorkspaceId) : IRequest<IReadOnlyList<MembershipResponse>>;

public sealed class GetWorkspaceMembersHandler : IRequestHandler<GetWorkspaceMembersQuery, IReadOnlyList<MembershipResponse>>
{
    private readonly IMembershipRepository _repository;

    public GetWorkspaceMembersHandler(IMembershipRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<MembershipResponse>> Handle(GetWorkspaceMembersQuery request, CancellationToken cancellationToken)
    {
        var memberships = await _repository.GetByWorkspaceIdAsync(request.WorkspaceId, cancellationToken);

        return memberships.Select(MembershipResponse.FromEntity).ToList();
    }
}
