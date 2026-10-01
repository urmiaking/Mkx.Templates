using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mkx.Templates.Sdk.Server.Api;
using Mkx.Templates.Sdk.Server.Shared.Data;
using Mkx.Templates.Shared.Abstractions;
using Mkx.Templates.Shared.Authorization;
using Mkx.Templates.Shared.DTOs.Tests;
using Mkx.Templates.Shared.Routes;

namespace Mkx.Templates.Server.Controllers;

[Route(ApiRoutes.Tests.Base)]
[Authorize(Policy = AppPolicies.Tests.View)]
public class TestsController(ITestService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] RequestFilter filter, CancellationToken cancellationToken) => Ok(await service.GetAllAsync(filter, cancellationToken));
    [HttpGet(ApiRoutes.Tests.Get)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Ok(await service.GetAsync(id, cancellationToken));
    [HttpPost]
    [Authorize(Policy = AppPolicies.Tests.Manage)]
    public async Task<IActionResult> Create(CreateTestRequest request, CancellationToken cancellationToken)
    {
        var created = await service.CreateAsync(request, cancellationToken);
        return Created(ApiUrls.Tests.Get(created.Id), created);
    }
    [HttpPut(ApiRoutes.Tests.Get)]
    [Authorize(Policy = AppPolicies.Tests.Manage)]
    public async Task<IActionResult> Update(Guid id, UpdateTestRequest request, CancellationToken cancellationToken) => Ok(await service.UpdateAsync(id, request, cancellationToken));
    [HttpDelete(ApiRoutes.Tests.Get)]
    [Authorize(Policy = AppPolicies.Tests.Manage)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid version, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, version, cancellationToken);
        return NoContent();
    }
}
