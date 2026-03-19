namespace Applican.Api.Models;

public class ExternalLoginResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<UserLocation> UserLocations { get; set; } = Array.Empty<UserLocation>();
}
