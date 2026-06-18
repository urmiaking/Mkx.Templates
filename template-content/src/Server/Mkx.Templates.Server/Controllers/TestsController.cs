using Microsoft.AspNetCore.Mvc;
using Mkx.Templates.Sdk.Server.Api;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Server.Controllers;

[Route(ApiRoutes.Tests.Base)]
public class TestsController(ITestService testService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var items = await testService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet(ApiRoutes.Tests.Get)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await testService.GetAsync(id, cancellationToken);
        return Ok(item);
    }
}
