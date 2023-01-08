using KSharpPlus.Net.Rest;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Exceptions; 

/// <summary>
/// Represents an exception thrown when Kuracord returns an Internal Server Error.
/// </summary>
public class ServerErrorException : KuracordException {
    internal ServerErrorException(BaseRestRequest request, RestResponse response) : base($"Internal Server Error: {response.ResponseCode}; {JObject.Parse(response.Response)["message"]}") {
        WebRequest = request;
        WebResponse = response;

        try {
            JObject jObject = JObject.Parse(response.Response);
            JToken? message = jObject["message"];

            if (message != null) JsonMessage = message.ToString();
        } catch (Exception) {/**/}
    }
}