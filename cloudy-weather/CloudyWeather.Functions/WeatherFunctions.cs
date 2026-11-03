using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CloudyWeather.Functions;

public class WeatherFunctions
{
    private static string SysEnvironment =>
        System.Environment.GetEnvironmentVariable("SYS_ENVIRONMENT") ?? "";

    // GET /ServiceCheck
    [Function("ServiceCheck")]
    public async Task<HttpResponseData> ServiceCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ServiceCheck")] HttpRequestData req)
    {
        return await TextResponse(req, $"The current environment is: {SysEnvironment}");
    }

    // GET /api/Weather
    [Function("Weather")]
    public async Task<HttpResponseData> Weather(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/Weather")] HttpRequestData req)
    {
        if (SysEnvironment == "LOCAL")
        {
            return await TextResponse(req, "The weather is great.");
        }

        return await TextResponse(req, "The weather is snowy.");
    }

    private static async Task<HttpResponseData> TextResponse(HttpRequestData req, string text)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(text);
        return response;
    }
}