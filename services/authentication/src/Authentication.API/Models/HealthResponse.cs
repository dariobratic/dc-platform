namespace Authentication.API.Models;

public class HealthResponse
{
    public required string ServiceName { get; set; }
    public required string Status { get; set; }
    public DateTime Timestamp { get; set; }
}
