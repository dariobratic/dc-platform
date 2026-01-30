namespace Authentication.API.Models;

public record SigninRequest(
    string Email,
    string Password);
