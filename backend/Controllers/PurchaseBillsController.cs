using Applican.Api.Models;
using Applican.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Applican.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseBillsController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public PurchaseBillsController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] PurchaseBillCreateRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await _databaseService.EnsureDatabaseAndTablesAsync(cancellationToken);
        var newId = await _databaseService.AddPurchaseBillAsync(request, cancellationToken);

        return Ok(new
        {
            message = "Purchase bill added successfully.",
            id = newId
        });
    }
}
