using Microsoft.AspNetCore.Mvc;

namespace Mkx.Templates.Sdk.Server.Api;

[ApiController]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public abstract class ApiControllerBase : Controller;
