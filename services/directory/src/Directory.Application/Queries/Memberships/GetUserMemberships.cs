using Directory.Application.Interfaces;
using MediatR;

namespace Directory.Application.Queries.Memberships;

public sealed record GetUserMembershipsQuery(Guid UserId) : IRequest<IReadOnlyList<MembershipResponse>>;

public sealed class GetUserMembershipsHandler : IRequestHandler<GetUserMembershipsQuery, IReadOnlyList<MembershipResponse>>
{
    private readonly IMembershipRepository _repository;

    public GetUserMembershipsHandler(IMembershipRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<MembershipResponse>> Handle(GetUserMembershipsQuery request, CancellationToken cancellationToken)
    {
        var memberships = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        return memberships.Select(MembershipResponse.FromEntity).ToList();
    }
}
