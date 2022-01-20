using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;


namespace PresenceLight.Core
{
    public class LIFXOAuthHelper
    {
        private const string LIFXAuthority = "https://cloud.lifx.com/oauth";
        private readonly string _lIFXTokenEndpoint = $"{LIFXAuthority}/token";
        private readonly string _lIFXAuthorizationEndpoint = $"{LIFXAuthority}/authorize";
        private readonly BaseConfig _options;
        private readonly ILogger<LIFXOAuthHelper> _logger;

        public LIFXOAuthHelper(Microsoft.Extensions.Options.IOptionsMonitor<BaseConfig> optionsAccessor, ILogger<LIFXOAuthHelper> logger)
        {
            _logger = logger;
            _options = optionsAccessor.CurrentValue;

        }

        public async Task<string> InitiateTokenRetrieval()
        {

            var builder = WebApplication.CreateBuilder();
            builder.Host.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            });
            var app = builder.Build();

            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            app.Run(async ctx =>
            {
                Task WriteResponse(HttpContext ctx)
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "text/html";
                    return ctx.Response.WriteAsync("<html><head><meta http-equiv='refresh' content='10;url=https://lifx.com'></head><body>Please return to the app.</body></html>", Encoding.UTF8);
                }

                switch (ctx.Request.Method)
                {
                    case "GET":
                        await WriteResponse(ctx);

                        tcs.TrySetResult(ctx.Request.QueryString.Value);
                        break;

                    case "POST" when !ctx.Request.HasFormContentType:
                        ctx.Response.StatusCode = 415;
                        break;

                    case "POST":
                        {
                            using var sr = new StreamReader(ctx.Request.Body, Encoding.UTF8);
                            var body = await sr.ReadToEndAsync();

                            await WriteResponse(ctx);

                            tcs.TrySetResult(body);
                            break;
                        }

                    default:
                        ctx.Response.StatusCode = 405;
                        break;
                }
            });

            var browserPort = 17236;

            app.Urls.Add($"http://localhost:{browserPort}/");

            app.Start();

            var timeout = TimeSpan.FromMinutes(5);

            string redirectUri = string.Format($"http://localhost:{browserPort}/");

            string state = RandomDataBase64Url(32);

            string authorizationRequest = string.Format("{0}?response_type=code&scope=remote_control:all&client_id={1}&state={2}&redirect_uri={3}",
                _lIFXAuthorizationEndpoint,
                _options.LightSettings.LIFX.LIFXClientId,
                state,
              HttpUtility.UrlEncode(redirectUri)
                );

            Helpers.OpenBrowser(authorizationRequest);

            var qs = await tcs.Task.WaitAsync(timeout);

            var qsDict = QueryHelpers.ParseQuery(qs.Replace("?", ""));
            StringValues code;
            qsDict.TryGetValue("code", out code);
            await app.DisposeAsync();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code.ToString()),
                new KeyValuePair<string, string>("client_id", _options.LightSettings.LIFX.LIFXClientId),
                new KeyValuePair<string, string>("client_secret",  _options.LightSettings.LIFX.LIFXClientSecret),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });


            // sends the request
            HttpClient _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _client.PostAsync(_lIFXTokenEndpoint, formContent);

            string responseText = await response.Content.ReadAsStringAsync();

            Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

            string _accessToken = tokenEndpointDecoded["access_token"];


            return _accessToken;
        }


        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        private string RandomDataBase64Url(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64UrlEncodeNoPadding(bytes);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private string Base64UrlEncodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-", StringComparison.OrdinalIgnoreCase);
            base64 = base64.Replace("/", "_", StringComparison.OrdinalIgnoreCase);
            // Strips padding.
            base64 = base64.Replace("=", "", StringComparison.OrdinalIgnoreCase);

            return base64;
        }
    }
}
