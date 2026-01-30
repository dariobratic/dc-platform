namespace Authentication.API.Models;

public record SignupRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string OrganizationName);
