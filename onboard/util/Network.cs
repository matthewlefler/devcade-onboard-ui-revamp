using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using log4net;
using log4net.Repository.Hierarchy;

namespace onboard.util; 

public static class Network {
    private static readonly ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName);

    public static Task<Result<HttpResponseMessage, Exception>> getResponseAsync(string uri) {
        return getResponseAsync(uri, Option<string>.None());
    }
    
    public static Task<Result<HttpResponseMessage, Exception>> getResponseAsync(string uri, string token) {
        return getResponseAsync(uri, Option<string>.Some(token));
    }
    
    private static async Task<Result<HttpResponseMessage, Exception>> getResponseAsync(string uri, Option<string> token) {
        logger.Trace($"Getting response from {uri} Authorization: {token.is_some()}");
        using var client = new HttpClient();
        if (token.is_some()) {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token.unwrap()}");
        }
        try {
            HttpResponseMessage response = await client.GetAsync(uri);
            return response.IsSuccessStatusCode ? 
                Result<HttpResponseMessage, Exception>.Ok(response) : 
                Result<HttpResponseMessage, Exception>.Err(new HttpRequestException(response.StatusCode.ToString()));
        }
        catch (Exception e) {
            return Result<HttpResponseMessage, Exception>.Err(e);
        }
    }

    public static Result<string, Exception> getResponseString(Result<HttpResponseMessage, Exception> message) {
        return message.is_err() ? 
            message.map(_ => "") : 
            getResponseString(message.unwrap());
    }

    public static Result<string, Exception> getResponseString(HttpResponseMessage message) {
        Result<HttpResponseMessage, int> status = getResultFromResponse(message);
        return status.match(
            responseMessage => Result<string, Exception>.Ok(responseMessage.Content.ReadAsStringAsync().Result),
            e => {
                logger.Warn($"HTTP request to {message.RequestMessage?.RequestUri} failed with status code {e}");
                return Result<string, Exception>.Err(new Exception($"Error getting response string: {e}"));
            }
        );
    }

    public static Result<Stream, Exception> getResponseStream(Result<HttpResponseMessage, Exception> message) {
        return message.is_err() ? 
            message.map<Stream>(_ => null) : 
            getResponseStream(message.unwrap());
    }
    
    public static Result<Stream, Exception> getResponseStream(HttpResponseMessage message) {
        Result<HttpResponseMessage, int> status = getResultFromResponse(message);
        return status.match(
            responseMessage => Result<Stream, Exception>.Ok(responseMessage.Content.ReadAsStreamAsync().Result),
            e => {
                logger.Warn($"HTTP request to {message.RequestMessage?.RequestUri} failed with status code {e}");
                return Result<Stream, Exception>.Err(new Exception($"Error getting response stream: {e}"));
            }
        );
    }
    
    public static Result<byte[], Exception> getResponseBytes(Result<HttpResponseMessage, Exception> message) {
        return message.is_err() ? 
            message.map<byte[]>(_ => null) : 
            getResponseBytes(message.unwrap());
    }
    
    public static Result<byte[], Exception> getResponseBytes(HttpResponseMessage message) {
        Result<HttpResponseMessage, int> status = getResultFromResponse(message);
        return status.match(
            responseMessage => Result<byte[], Exception>.Ok(responseMessage.Content.ReadAsByteArrayAsync().Result),
            e => {
                logger.Warn($"HTTP request to {message.RequestMessage?.RequestUri} failed with status code {e}");
                return Result<byte[], Exception>.Err(new Exception($"Error getting response bytes: {e}"));
            }
        );
    }
    
    private static Result<HttpResponseMessage, int> getResultFromResponse(HttpResponseMessage message) {
        return message.IsSuccessStatusCode ? 
            Result<HttpResponseMessage, int>.Ok(message) : 
            Result<HttpResponseMessage, int>.Err((int) message.StatusCode);
    }
}