using Mkx.Templates.Server.Common;
using Mkx.Templates.Sdk.Shared.Attributes;
using Microsoft.Extensions.Options;

namespace Mkx.Templates.Server.Services;

[SingletonService]
internal sealed class OidcProviderStore(IOptions<List<OidcDescriptor>> options)
{
    public IReadOnlyList<OidcDescriptor> GetAll() => options.Value;
}
