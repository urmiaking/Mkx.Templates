namespace Mkx.Templates.Sdk.Server.Shared.Extensions;

public static class DictionaryExtensions
{
    extension(Dictionary<string, string>? first)
    {
        public Dictionary<string, string> Merge(Dictionary<string, string>? second)
        {
            var result = new Dictionary<string, string>();

            if (first != null)
            {
                foreach (var kvp in first) result[kvp.Key] = kvp.Value;
            }

            if (second != null)
            {
                foreach (var kvp in second) result[kvp.Key] = kvp.Value;
            }

            return result;
        }
    }
}
