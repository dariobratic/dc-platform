using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Queries.Invitations;

public sealed record GetWorkspaceInvitationsQuery(Guid WorkspaceId) : IRequest<IReadOnlyList<InvitationResponse>>;

public sealed class GetWorkspaceInvitationsHandler : IRequestHandler<GetWorkspaceInvitationsQuery, IReadOnlyList<InvitationResponse>>
{
    private readonly IInvitationRepository _repository;

    public GetWorkspaceInvitationsHandler(IInvitationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<InvitationResponse>> Handle(GetWorkspaceInvitationsQuery request, CancellationToken cancellationToken)
    {
        var invitations = await _repository.GetByWorkspaceIdAsync(request.WorkspaceId, cancellationToken);

        return invitations.Select(InvitationResponse.FromEntity).ToList();
    }
}
