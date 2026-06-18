using Mkx.Templates.Sdk.Server.Shared.Authorization;

namespace Mkx.Templates.Shared.Authorization;

public class AppPolicyProvider : IApplicationPolicyProvider
{
    public string Category => "Mkx.Templates";
    public IEnumerable<PolicyDefinition> GetPolicies()
    {
        yield return PolicyDefinition.Build(AppPolicies.Tests.View, "دسترسی مشاهده تست");
    }
}