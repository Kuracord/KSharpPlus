﻿using KSharpPlus.Net.Rest;
using Newtonsoft.Json.Linq;

namespace KSharpPlus.Exceptions; 

/// <summary>
/// Represents an exception thrown when a malformed request is sent.
/// </summary>
public class BadRequestException : KuracordException {
    /// <summary>
    /// Gets the error code for this exception.
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Gets the form error responses in JSON format.
    /// </summary>
    public string Errors { get; } = null!;

    internal BadRequestException(BaseRestRequest request, RestResponse response) : base($"Bad request: {response.ResponseCode}; {JObject.Parse(response.Response)["message"]}") {
        WebRequest = request;
        WebResponse = response;

        try {
            JObject j = JObject.Parse(response.Response);
            
            JToken? code = j["code"];
            JToken? message = j["message"];
            JToken? errors = j["errors"];
            
            if (code != null) Code = (int)code;
            if (message != null) JsonMessage = message.ToString();
            if (errors != null) Errors = errors.ToString();
        } catch {/**/}
    }
}