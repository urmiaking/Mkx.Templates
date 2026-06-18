using System;
using System.Net.Http;

namespace Mkx.Templates.Sdk.Shared.Utilities;

public static class HttpClientHelper
{
    public static HttpClient GetHttpClient(string baseAddress)
    {
        return new HttpClient { BaseAddress = new Uri(baseAddress) };
    }
}
