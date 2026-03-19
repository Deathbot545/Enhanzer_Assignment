using Applican.Api.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Applican.Api.Services;

public class ExternalAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExternalAuthService> _logger;

    public ExternalAuthService(IHttpClientFactory httpClientFactory, ILogger<ExternalAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ExternalLoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ExternalApi");

        var payload = new
        {
            API_Action = "GetLoginData",
            Device_Id = "D001",
            Sync_Time = "",
            Company_Code = request.Email,
            API_Body = new
            {
                Username = request.Email,
                Pw = request.Password
            }
        };

        var requestJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        });

        using var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await client.PostAsync(string.Empty, httpContent, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new ExternalLoginResult
            {
                IsSuccess = false,
                Message = "Login request failed."
            };
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return new ExternalLoginResult
            {
                IsSuccess = false,
                Message = "Empty response from login API."
            };
        }

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        if (!TryGetUserLocations(root, out var locations) || locations.Count == 0)
        {
            _logger.LogWarning("Login response did not contain User_Locations. Response: {ResponseContent}", content);
            return new ExternalLoginResult
            {
                IsSuccess = false,
                Message = GetFailureMessage(root) ?? "Invalid username or password."
            };
        }

        return new ExternalLoginResult
        {
            IsSuccess = true,
            Message = "Login successful.",
            UserLocations = locations
        };
    }

    private static bool TryGetUserLocations(JsonElement root, out IReadOnlyList<UserLocation> locations)
    {
        locations = Array.Empty<UserLocation>();

        if (!TryFindArrayPropertyRecursively(root, "User_Locations", out var userLocationsElement))
        {
            return false;
        }

        var parsed = new List<UserLocation>();
        foreach (var item in userLocationsElement.EnumerateArray())
        {
            var locationCode = GetStringProperty(item, "Location_Code");
            var locationName = GetStringProperty(item, "Location_Name");

            if (string.IsNullOrWhiteSpace(locationCode) && string.IsNullOrWhiteSpace(locationName))
            {
                continue;
            }

            parsed.Add(new UserLocation
            {
                Location_Code = locationCode,
                Location_Name = locationName
            });
        }

        locations = parsed;
        return true;
    }

    private static bool TryFindArrayPropertyRecursively(JsonElement element, string propertyName, out JsonElement arrayElement)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.Array)
                {
                    arrayElement = property.Value;
                    return true;
                }

                if (TryFindArrayPropertyRecursively(property.Value, propertyName, out arrayElement))
                {
                    return true;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryFindArrayPropertyRecursively(item, propertyName, out arrayElement))
                {
                    return true;
                }
            }
        }

        arrayElement = default;
        return false;
    }

    private static string? GetFailureMessage(JsonElement root)
    {
        if (TryGetPropertyCaseInsensitive(root, "Status_Code", out var statusCode)
            && statusCode.ValueKind == JsonValueKind.Number
            && statusCode.TryGetInt32(out var code)
            && code == 200)
        {
            return null;
        }

        return GetStringProperty(root, "Message")
               ?? GetStringProperty(root, "MSG")
               ?? GetStringProperty(root, "Error")
               ?? GetStringProperty(root, "Error_Message");
    }

    private static string GetStringProperty(JsonElement element, string propertyName)
    {
        return TryGetPropertyCaseInsensitive(element, propertyName, out var property)
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static bool TryGetPropertyCaseInsensitive(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }
}
