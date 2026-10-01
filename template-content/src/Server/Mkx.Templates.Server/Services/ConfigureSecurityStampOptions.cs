using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Mkx.Templates.Server.Services;

public class ConfigureSecurityStampOptions : IConfigureOptions<SecurityStampValidatorOptions>
{
    public void Configure(SecurityStampValidatorOptions options)
    {
        options.ValidationInterval = TimeSpan.FromMinutes(5);
    }
}
