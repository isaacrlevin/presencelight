using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace PresenceLight.Core
{
    public class CustomApiResponse
    {
        public static CustomApiResponse None = new CustomApiResponse() { IsSuccessful = true };

        public bool IsSuccessful { get; private set; }
        public string Method { get; private set; }
        public string Url { get; private set; }
        public HttpStatusCode? StatusCode { get; private set; }
        public string? Content { get; private set; }
        public Exception? Exception { get; private set; }

        private CustomApiResponse()
        {
        }

        public static async Task<CustomApiResponse> CreateAsync(string method, string url, HttpResponseMessage response, CancellationToken ct)
        {
            return new CustomApiResponse
            {
                IsSuccessful = response.IsSuccessStatusCode,
                Method = method,
                Url = url,
                StatusCode = response.StatusCode,
                Content = await response.Content.ReadAsStringAsync(ct),
            };
        }

        public static CustomApiResponse Create(string method, string url, Exception exception)
        {
            return new CustomApiResponse
            {
                IsSuccessful = false,
                Method = method,
                Url = url,
                Exception = exception
            };
        }

        public override string ToString()
        {
            string result =
                Exception != null ?
                    Exception.ToString() :
                    StatusCode != null ?
                        $"[{(int)StatusCode}] (Body: '{ Content }')'" :
                        $"IsSuccessful: {IsSuccessful}";
            return $"{Method} {Url}: {result}";
        }
    }
}
