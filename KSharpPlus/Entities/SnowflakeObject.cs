using KSharpPlus.Clients;
using Newtonsoft.Json;

namespace KSharpPlus.Entities; 

/// <summary>
/// Represents an object in Kuracord API.
/// </summary>
public abstract class SnowflakeObject {
    /// <summary>
    /// Gets the ID of this object.
    /// </summary>
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public ulong Id { get; internal set; }
    
    /// <summary>
    /// Gets the date and time this object was created.
    /// </summary>
    [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset CreationTimestamp { get; internal set; }
    
    /// <summary>
    /// Gets the client instance this object is tied to.
    /// </summary>
    [JsonIgnore] internal BaseKuracordClient? Kuracord { get; set; }

    internal SnowflakeObject() { }
}