using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Mkx.Templates.Sdk.Server.Shared.Extensions;

public static class MemberInfoExtensions
{
    extension(MemberInfo? member)
    {
        public string GetDisplayName()
        {
            var customAttribute1 = member?.GetCustomAttribute<DisplayAttribute>();

            if (customAttribute1 != null && !string.IsNullOrEmpty(customAttribute1.Name))
                return customAttribute1.Name;

            var customAttribute2 = member?.GetCustomAttribute<DisplayNameAttribute>();
            return customAttribute2 != null && !string.IsNullOrEmpty(customAttribute2.DisplayName) ? customAttribute2.DisplayName : member?.Name ?? string.Empty;
        }

        public string? GetPrompt()
        {
            var customAttribute = member?.GetCustomAttribute<DisplayAttribute>();
            return customAttribute != null && !string.IsNullOrEmpty(customAttribute.Prompt) ? customAttribute.Prompt : null;
        }
    }
}
