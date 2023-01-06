using KSharpPlus.Net.Rest;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Exceptions; 

/// <summary>
/// Represents an exception thrown when requester doesn't have necessary permissions to complete the request.
/// </summary>
public class UnauthorizedException : KuracordException {
    internal UnauthorizedException(BaseRestRequest request, RestResponse response) : base($"Unauthorized: {response.ResponseCode}") {
        WebRequest = request;
        WebResponse = response;

        try {
            JObject jObject = JObject.Parse(response.Response);
            JToken? message = jObject["message"];

            if (message != null) JsonMessage = message.ToString();
        } catch (Exception) {/**/}
    }
}