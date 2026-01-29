using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Commands.Invitations;

public sealed record RevokeInvitationCommand(Guid Id) : IRequest;

public sealed class RevokeInvitationHandler : IRequestHandler<RevokeInvitationCommand>
{
    private readonly IInvitationRepository _repository;

    public RevokeInvitationHandler(IInvitationRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.Id);

        invitation.Revoke();

        await _repository.UpdateAsync(invitation, cancellationToken);
    }
}
