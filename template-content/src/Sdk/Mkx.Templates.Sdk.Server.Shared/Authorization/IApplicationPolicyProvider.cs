namespace Mkx.Templates.Sdk.Server.Shared.Authorization;

public interface IApplicationPolicyProvider
{
    string Category { get; }
    IEnumerable<PolicyDefinition> GetPolicies();
}

