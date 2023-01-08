using KSharpPlus.Net.Rest;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Exceptions; 

/// <summary>
/// Represents an exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : KuracordException {
    internal NotFoundException(BaseRestRequest request, RestResponse response) : base($"Not found: {response.ResponseCode}; {JObject.Parse(response.Response)["message"]}") {
        WebRequest = request;
        WebResponse = response;

        try {
            JObject jObject = JObject.Parse(response.Response);
            JToken? message = jObject["message"];

            if (message != null) JsonMessage = message.ToString();
        } catch (Exception) {/**/}
    }
}