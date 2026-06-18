using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mkx.Templates.Sdk.Shared.Extensions;

public static class StringExtensions
{
    extension(string template)
    {
        public string FormatRoute() => template.FormatRoute(new { });

        public string FormatRoute(object parameters)
        {
            var route = template.RichFormat(parameters);
            route = route.Replace("//", "/");

            return route;
        }

        public string RichFormat(object parameters)
        {
            var properties = parameters.GetType().GetProperties();
            var dictionary =
                properties.ToDictionary(property => property.Name, property => property.GetValue(parameters));

            return template.RichFormat(dictionary);
        }

        public string RichFormat(Dictionary<string, object> parameters)
        {
            var matches = Regex.Matches(template, "{(.*?)}");

            foreach (Match match in matches)
            {
                var valueWithoutBrackets = match.Groups[1].Value;
                var valueWithBrackets = match.Value;
                var isNullable = valueWithoutBrackets.EndsWith("?");

                var argName = valueWithoutBrackets.Split(':')[0];

                if (isNullable && argName.EndsWith("?"))
                    argName = argName.Substring(0, argName.Length - 1);

                var key = parameters.Keys
                    .FirstOrDefault(x => string.Compare(x, argName, StringComparison.OrdinalIgnoreCase) == 0);

                if (key != null)
                {
                    var value = parameters[key];
                    template = template.Replace(valueWithBrackets, value?.ToString());
                }
                else if (isNullable)
                {
                    template = template.Replace(valueWithBrackets, string.Empty);
                }
                else
                {
                    throw new ArgumentException("The '" + argName + "' argument is required.");
                }
            }

            return template;
        }

        public string AppendQueryString(object parameters)
        {
            if (parameters == null)
                return template;

            var dictionary = new Dictionary<string, object>();

            var props = parameters.GetType().GetProperties();
            foreach (var prop in props)
            {
                var value = prop.GetValue(parameters);
                if (value == null) continue;

                if (value is IEnumerable && !(value is string))
                {
                    var list = ((IEnumerable)value).Cast<object>().ToList();
                    dictionary[prop.Name] = list;
                }
                else
                {
                    dictionary[prop.Name] = value;
                }
            }

            return template.AppendQueryString(dictionary);
        }

        public string AppendQueryString(Dictionary<string, object> parameters)
        {
            string url;
            var query = string.Empty;

            if (template.Contains("?"))
            {
                var splits = template.Split('?');
                url = splits[0];
                query = splits[1];
            }
            else
            {
                url = template;
            }

            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(query))
            {
                queryParams.AddRange(query.Split('&'));
            }

            foreach (var kvp in parameters)
            {
                if (kvp.Value is IEnumerable && !(kvp.Value is string))
                {
                    foreach (var item in (IEnumerable)kvp.Value)
                    {
                        var val = item is DateTime dt
                            ? dt.ToString(new CultureInfo("en-US"))
                            : item?.ToString();

                        if (val != null)
                            queryParams.Add($"{kvp.Key}={Uri.EscapeDataString(val)}");
                    }
                }
                else
                {
                    var val = kvp.Value is DateTime dt
                        ? dt.ToString(new CultureInfo("en-US"))
                        : kvp.Value?.ToString();

                    if (val != null)
                        queryParams.Add($"{kvp.Key}={Uri.EscapeDataString(val)}");
                }
            }

            return $"{url}?{string.Join("&", queryParams)}";
        }

        public string ToPersianChars()
        {
            return string.IsNullOrEmpty(template) ? template : template.Replace('ك', 'ک').Replace('ي', 'ی');
        }

        public string ToEnglishNumericsChars()
        {
            if (string.IsNullOrEmpty(template))
                return template;

            return template.Replace('۰', '0')
                    .Replace('۱', '1')
                    .Replace('۲', '2')
                    .Replace('۳', '3')
                    .Replace('۴', '4')
                    .Replace('۵', '5')
                    .Replace('۶', '6')
                    .Replace('۷', '7')
                    .Replace('۸', '8')
                    .Replace('۹', '9')
                ;
        }

        public string NormalizeText()
        {
            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            var t = template.Trim();

            t = t.Replace("ي", "ی")
                .Replace("ى", "ی")
                .Replace("ئ", "ی")
                .Replace("ؤ", "و")
                .Replace("ك", "ک")
                .Replace("\u200C", " "); // remove ZWNJ

            while (t.Contains("  "))
                t = t.Replace("  ", " ");

            return t;
        }

        public string GetInitials()
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            // Handle both English and Arabic names correctly
            var names = Regex.Split(template.Trim(), @"\s+");

            if (names.Length == 0)
                return "";

            return names
                .Where(n => !string.IsNullOrEmpty(n))
                .Aggregate("", (current, n) => current + n.Substring(0, 1));
        }

        public string GetInitialsWithPeriods()
        {
            var initials = GetInitials(template);
            return string.Join(".", initials.ToCharArray()); // Add periods
        }

        public string FormatDateString()
        {
            if (string.IsNullOrWhiteSpace(template))
                return template ?? string.Empty;

            // Must be exactly yyyyMMdd and all digits
            if (template.Length != 8 || !template.All(char.IsDigit))
                return template;

            // Validate it is a real date
            DateTime parsedDate;
            if (!DateTime.TryParseExact(
                    template,
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsedDate))
                return template;

            return template.Substring(0, 4) + "/" +
                   template.Substring(4, 2) + "/" +
                   template.Substring(6, 2);
        }
    }
}
