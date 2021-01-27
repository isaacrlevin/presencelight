using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using PresenceLight.Core;

namespace PresenceLight.Services
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
            try
            {
                string state = RandomDataBase64Url(32);

                string redirectURI = "";
                var http = new System.Net.HttpListener();

                foreach (var i in new int[] { 17236, 17284, 17287, 17291, 17296 })
                {
                    try
                    {
                        redirectURI = string.Format("http://localhost:{0}/", i);
                        http.Prefixes.Add(redirectURI);
                        http.Start();

                        break;
                    }
                    catch
                    {
                        http = new System.Net.HttpListener();
                    }
                }

                // Creates the OAuth 2.0 authorization request.
                string authorizationRequest = string.Format("{0}?response_type=code&scope=remote_control:all&client_id={1}&state={2}&redirect_uri={3}",
                    _lIFXAuthorizationEndpoint,
                    _options.LightSettings.LIFX.LIFXClientId,
                    state,
                  HttpUtility.UrlEncode(redirectURI)
                    );

                Helpers.OpenBrowser(authorizationRequest);

                // Waits for the OAuth authorization response.

                var context = await http.GetContextAsync().ConfigureAwait(true);

                //Sends an HTTP response to the browser.
                var response = context.Response;
                string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://lifx.com'></head><body>Please return to the app.</body></html>");
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                var responseOutput = response.OutputStream;
                Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
                {
                    responseOutput.Close();
                    http.Stop();
                    Debug.WriteLine("HTTP server stopped.");
                });

                // Checks for errors.
                if (context.Request.QueryString.Get("error") != null)
                {
                    return "";
                }
                if (context.Request.QueryString.Get("code") == null
                    || context.Request.QueryString.Get("state") == null)
                {
                    return "";
                }

                // extracts the code
                var code = context.Request.QueryString.Get("code") ?? "";
                var incoming_state = context.Request.QueryString.Get("state");

                // Compares the receieved state to the expected value, to ensure that
                // this app made the request which resulted in authorization.
                if (incoming_state != state)
                {
                    return "";
                }

                // Starts the code exchange at the Token Endpoint.
                return await PerformCodeExchange(code).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error retrieving LIFX Token", e);
                throw;
            }
        }

        private async Task<string> PerformCodeExchange(string code)
        {
            // builds the  request

            string tokenRequestBody = string.Format("code={0}&client_id={1}&client_secret={2}&grant_type=authorization_code",
                code,
               _options.LightSettings.LIFX.LIFXClientId,
               _options.LightSettings.LIFX.LIFXClientSecret
                );

            // sends the request
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(_lIFXTokenEndpoint);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length).ConfigureAwait(true);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync().ConfigureAwait(true);
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync().ConfigureAwait(true);

                    // converts to dictionary
                    Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    string access_token = tokenEndpointDecoded["access_token"];

                    if (!string.IsNullOrEmpty(access_token))
                    {
                        _options.LightSettings.LIFX.LIFXApiKey = access_token;
                        return access_token;
                    }
                }
                return "";
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = await reader.ReadToEndAsync().ConfigureAwait(true);
                        }
                    }
                }
                Helpers.AppendLogger(_logger, "Error processing LIFX Token", ex);
            }
            catch (Exception e)
            {
                Helpers.AppendLogger(_logger, "Error processing LIFX Token", e);
            }

            return "";
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
