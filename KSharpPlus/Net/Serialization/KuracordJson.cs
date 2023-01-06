using System.Globalization;
using System.Text;
using KSharpPlus.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Net.Serialization; 

public static class KuracordJson {
    static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings {
        ContractResolver = new OptionalJsonContractResolver(),
        DateParseHandling = DateParseHandling.None,
        Converters = new JsonConverter[] { new ISO8601DateTimeOffsetJsonConverter() }
    });
    
    /// <summary>Serializes the specified object to a JSON string.</summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeObject(object value) => SerializeObjectInternal(value, null, Serializer);

    /// <summary>Populates an object with the values from a JSON node.</summary>
    /// <param name="value">The token to populate the object with.</param>
    /// <param name="target">The object to populate.</param>
    public static void PopulateObject(JToken value, object target) {
        using JsonReader reader = value.CreateReader();
        Serializer.Populate(reader, target);
    }

    /// <summary>
    /// Converts this token into an object, passing any properties through extra <see cref="JsonConverter"/>s if
    /// needed.
    /// </summary>
    /// <param name="token">The token to convert</param>
    /// <typeparam name="T">Type to convert to</typeparam>
    /// <returns>The converted token</returns>
    public static T ToKuracordObject<T>(this JToken token) => token.ToObject<T>(Serializer)!;

    static string SerializeObjectInternal(object value, Type? type, JsonSerializer jsonSerializer) {
        StringWriter stringWriter = new(new StringBuilder(256), CultureInfo.InvariantCulture);
        
        using (JsonTextWriter jsonTextWriter = new(stringWriter)) {
            jsonTextWriter.Formatting = jsonSerializer.Formatting;
            jsonSerializer.Serialize(jsonTextWriter, value, type);
        }
        
        return stringWriter.ToString();
    }
}