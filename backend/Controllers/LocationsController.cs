using Applican.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Applican.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public LocationsController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var locations = await _databaseService.GetLocationsAsync(cancellationToken);

        return Ok(new
        {
            count = locations.Count,
            locations = locations
        });
    }
}
