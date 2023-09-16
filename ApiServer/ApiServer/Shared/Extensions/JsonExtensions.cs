using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ApiServer.Shared.Extensions;

public static class JsonExtensions
{
    public static string SerializeToJson(this object source, bool writeIndented = true, bool propertyNameCaseInsensitive = true, JavaScriptEncoder? encoder = null, bool includeFields = false)
    {

        string retValue = string.Empty;
        if (encoder == null)
            encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

        var options = new JsonSerializerOptions() 
        { 
            PropertyNameCaseInsensitive = propertyNameCaseInsensitive, 
            WriteIndented = writeIndented, 
            Encoder = encoder, 
            IncludeFields = includeFields 
        };
        retValue = JsonSerializer.Serialize(source, options);

        return retValue;
    }


    public static T? DeserializeFromJson<T>(this string value, bool writeIndented = true, bool propertyNameCaseInsensitive = true, JavaScriptEncoder? encoder = null, bool includeFields = false)
    {
        if (encoder == null)
            encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = propertyNameCaseInsensitive, WriteIndented = writeIndented, Encoder = encoder, IncludeFields = includeFields };
        var retValue = JsonSerializer.Deserialize<T>(value, options);
        return retValue;
    }

    public static dynamic? DeserializeFromJson(this string value, bool writeIndented = true, bool propertyNameCaseInsensitive = true, JavaScriptEncoder? encoder = null, bool includeFields = false)
    {
        if (encoder == null)
            encoder = JavaScriptEncoder.Create(UnicodeRanges.All);

        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = propertyNameCaseInsensitive, WriteIndented = writeIndented, Encoder = encoder, IncludeFields = includeFields };
        var retValue = JsonSerializer.Deserialize<dynamic>(value, options);
        return retValue;
    }

    public static async Task<T?> ReadAsAsync<T>(this HttpContent content, bool writeIndented = false, bool propertyNameCaseInsensitive = true, bool includeFields = false)
    {
        var options = new System.Text.Json.JsonSerializerOptions
        { WriteIndented = writeIndented, PropertyNameCaseInsensitive = propertyNameCaseInsensitive, IncludeFields = includeFields };
        return await JsonSerializer.DeserializeAsync<T>(await content.ReadAsStreamAsync(), options);
    }
}