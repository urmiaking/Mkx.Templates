using Mkx.Templates.Sdk.Server.Shared.Authorization;

namespace Mkx.Templates.Shared.Authorization;

public class AppPolicyProvider : IApplicationPolicyProvider
{
    public string Category => "Mkx.Templates";
    public IEnumerable<PolicyDefinition> GetPolicies()
    {
        yield return PolicyDefinition.Build(AppPolicies.Tests.View, "دسترسی مشاهده تست");
        yield return PolicyDefinition.Build(AppPolicies.Users.View, "مشاهده کاربران و دسترسی‌ها",
            childPolicies:
            [
                PolicyDefinition.Build(AppPolicies.Users.Manage, "مدیریت کاربران (ایجاد/ویرایش/حذف)"),
                PolicyDefinition.Build(AppPolicies.Users.ManageClaims, "مدیریت دسترسی‌ها (تخصیص مجوزهای سیستم)")
            ]);
    }
}
