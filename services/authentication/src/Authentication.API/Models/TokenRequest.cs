namespace Authentication.API.Models;

public record TokenRequest
{
    public required string Code { get; init; }
    public required string RedirectUri { get; init; }
}
