using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mkx.Templates.Sdk.Shared.Utilities;

public static class JsonOptionHelpers
{
    public static JsonSerializerOptions GetJsonOptions()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        jsonOptions.Converters.Clear();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        return jsonOptions;
    }
}
