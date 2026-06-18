using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Mkx.Templates.Sdk.Server.Shared.Extensions;

public static class EnumExtensions
{
    extension(Enum enumValue)
    {
        internal TAttribute? GetAttribute<TAttribute>() where TAttribute : Attribute
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
            return memberInfo == null ? null : memberInfo.GetCustomAttribute<TAttribute>();
        }

        public string GetDisplayName()
        {
            var attribute = enumValue.GetAttribute<DisplayAttribute>();
            return !string.IsNullOrEmpty(attribute?.Name) ? attribute.Name : enumValue.ToString();
        }
    }
}

