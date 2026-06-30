using System;
using System.Linq;
using System.Linq.Expressions;
using Mkx.Templates.Sdk.Server.Shared.Authorization;
using Mkx.Templates.Sdk.Server.Shared.Extensions;

namespace Mkx.Templates.Client.Extensions;

public static class ModelExtensions
{
    public static string GetDisplayName<TProperty>(this object model, Expression<Func<TProperty>> expression)
    {
        return expression.GetDisplayName();
    }

    public static string GetRoleName(this string roleName)
    {
        return roleName switch
        {
            BuiltinRoles.Administrators => "مدیر سیستم",
            BuiltinRoles.Users => "کاربر",
            _ => roleName
        };
    }

    public static string GetInitialsWithPeriods(this string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var parts = name.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Select(p => p[0]);
        return string.Join(".", initials);
    }
}
