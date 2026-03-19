using Applican.Api.Models;
using Applican.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Applican.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ExternalAuthService _externalAuthService;
    private readonly DatabaseService _databaseService;

    public AuthController(ExternalAuthService externalAuthService, DatabaseService databaseService)
    {
        _externalAuthService = externalAuthService;
        _databaseService = databaseService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var loginResult = await _externalAuthService.LoginAsync(request, cancellationToken);
        if (!loginResult.IsSuccess)
        {
            return Unauthorized(new { message = loginResult.Message });
        }

        await _databaseService.EnsureDatabaseAndTablesAsync(cancellationToken);
        var savedCount = await _databaseService.SaveLocationsAsync(loginResult.UserLocations, cancellationToken);

        return Ok(new
        {
            message = loginResult.Message,
            locationsSaved = savedCount,
            locations = loginResult.UserLocations
        });
    }
}
