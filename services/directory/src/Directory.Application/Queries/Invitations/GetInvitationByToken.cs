using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Queries.Invitations;

public sealed record GetInvitationByTokenQuery(string Token) : IRequest<InvitationResponse>;

public sealed class GetInvitationByTokenHandler : IRequestHandler<GetInvitationByTokenQuery, InvitationResponse>
{
    private readonly IInvitationRepository _repository;

    public GetInvitationByTokenHandler(IInvitationRepository repository)
    {
        _repository = repository;
    }

    public async Task<InvitationResponse> Handle(GetInvitationByTokenQuery request, CancellationToken cancellationToken)
    {
        var invitation = await _repository.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.Token);

        return InvitationResponse.FromEntity(invitation);
    }
}
