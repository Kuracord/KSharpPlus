using KSharpPlus.Entities.User;
using Newtonsoft.Json;

namespace KSharpPlus.Entities.Channel; 

public class KuracordDmChannel : KuracordChannel {
    /// <summary>
    /// Gets the recipients of this direct message.
    /// </summary>
    [JsonProperty("recipients", NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<KuracordUser> Recipients { get; internal set; }
}