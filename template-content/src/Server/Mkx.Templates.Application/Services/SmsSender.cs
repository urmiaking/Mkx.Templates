using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mkx.Templates.Application.Abstractions;
using Mkx.Templates.Sdk.Shared.Attributes;

namespace Mkx.Templates.Application.Services;

[ScopedService]
public class SmsSender(ILogger<SmsSender> logger) : ISmsSender
{
    public Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation($"[SMS Sandbox] Sending SMS to {phoneNumber}: {message}");
        return Task.FromResult(true);
    }
}
