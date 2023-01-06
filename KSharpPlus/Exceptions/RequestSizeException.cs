using KSharpPlus.Net.Rest;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Exceptions; 

/// <summary>
/// Represents an exception thrown when the request sent to Kuracord is too large.
/// </summary>
public class RequestSizeException : KuracordException {
    internal RequestSizeException(BaseRestRequest request, RestResponse response) 
        : base($"Request entity too large: {response.ResponseCode}. Make sure the data sent is within Kuracord's upload limit.") {
        WebRequest = request;
        WebResponse = response;

        try {
            JObject jObject = JObject.Parse(response.Response);
            JToken? message = jObject["message"];

            if (message != null) JsonMessage = message.ToString();
        } catch (Exception) {/**/}
    }
}