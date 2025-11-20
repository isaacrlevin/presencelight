using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using JeffWilcox.Utilities.Silverlight;

using Newtonsoft.Json;

using Q42.HueApi.Interfaces;
using Q42.HueApi.Models;

namespace Q42.HueApi.Models
{
    public class AccessTokenResponseV2
    {
        public DateTimeOffset CreatedDate { get; set; }

        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }

        public AccessTokenResponseV2()
        {
            CreatedDate = DateTimeOffset.UtcNow;
        }
        public DateTimeOffset AccessTokenExpireTime()
        {
            return CreatedDate.AddSeconds(expires_in);
        }
    }
}

namespace Q42.HueApi.Interfaces
{
    public interface IRemoteAuthenticationClient
    {
        Uri BuildAuthorizeUri(string state, string deviceId, string? deviceName = null, string responseType = "code");

        RemoteAuthorizeResponse ProcessAuthorizeResponse(string responseData);

        /// <summary>
        /// Initialize with existing AccessTokenResponse
        /// </summary>
        /// <param name="storedAccessToken"></param>
        void Initialize(AccessTokenResponseV2 storedAccessToken);

        Task<AccessTokenResponseV2?> GetToken(string code);

        Task<AccessTokenResponseV2?> RefreshToken(string refreshToken);

        /// <summary>
        /// Gets a valid access token
        /// </summary>
        /// <returns></returns>
        Task<string?> GetValidToken();
    }
}

namespace Q42.HueApi
{
    /// <summary>
    /// https://developers.meethue.com/develop/hue-api/remote-authentication-oauth/
    /// </summary>
    public class RemoteAuthenticationClientV2 : IRemoteAuthenticationClient
    {
        public bool IsInitialized { get; protected set; }

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _appId;

        private AccessTokenResponseV2? _lastAuthorizationResponse;
        private HttpClient _httpClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId">Identifies the client that is making the request. The value passed in this parameter must exactly match the value you receive from hue. Note that the underscore is not used in the clientid name of this parameter.</param>
        /// <param name="clientSecret">The clientsecret you have received from Hue when registering for the Hue Remote API.</param>
        /// <param name="appId">Identifies the app that is making the request. The value passed in this parameter must exactly match the value you receive from hue.</param>
        public RemoteAuthenticationClientV2(string clientId, string clientSecret, string appId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId));
            if (string.IsNullOrEmpty(clientSecret))
                throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrEmpty(appId))
                throw new ArgumentNullException(nameof(appId));

            _clientId = clientId;
            _clientSecret = clientSecret;
            _appId = appId;
            _httpClient = new HttpClient();
        }

        public void Initialize(AccessTokenResponseV2 accessTokenResponse)
        {
            IsInitialized = true;
            _lastAuthorizationResponse = accessTokenResponse;
        }

        /// <summary>
        /// Authorization request
        /// </summary>
        /// <param name="state">Provides any state that might be useful to your application upon receipt of the response. The Hue Authorization Server roundtrips this parameter, so your application receives the same value it sent. To mitigate against cross-site request forgery (CSRF), it is strongly recommended to include an anti-forgery token in the state, and confirm it in the response. One good choice for a state token is a string of 30 or so characters constructed using a high-quality random-number generator.</param>
        /// <param name="deviceId">The device identifier must be a unique identifier for the app or device accessing the Hue Remote API.</param>
        /// <param name="deviceName">The device name should be the name of the app or device accessing the remote API. The devicename is used in the user's "My Apps" overview in the Hue Account (visualized as: "<app name> on <devicename>"). If not present, deviceid is also used for devicename. The <app name> is the application name you provided to us the moment you requested access to the remote API.</param>
        /// <param name="responseType">The response_type value must be "code".</param>
        /// <returns></returns>
        public Uri BuildAuthorizeUri(string state, string deviceId, string? deviceName = null, string responseType = "code")
        {
            if (string.IsNullOrEmpty(responseType))
                throw new ArgumentNullException(nameof(responseType));

            string url = string.Format("https://api.meethue.com/v2/oauth2/authorize?client_id={0}&response_type={5}&state={1}&appid={3}&deviceid={2}&devicename={4}", _clientId, state, deviceId, _appId, deviceName, responseType);

            return new Uri(url);
        }

        public RemoteAuthorizeResponse ProcessAuthorizeResponse(string responseData)
        {
            string url = responseData;
            string[] parts = url.Split(new char[] { '?', '&' });

            RemoteAuthorizeResponse result = new RemoteAuthorizeResponse();

            foreach (var part in parts)
            {
                string[] nv = part.Split(new char[] { '=' });
                if (nv.Length == 2)
                {
                    if (nv[0].ToLower() == "code")
                        result.Code = nv[1];
                    if (nv[0].ToLower() == "state")
                        result.State = nv[1];
                }
            }

            return result;
        }

        /// <summary>
        /// Get an access token
        /// </summary>
        /// <param name="code">Code retreived using ProcessAuthorizeResponse</param>
        /// <returns></returns>
        public async Task<AccessTokenResponseV2?> GetToken(string code)
        {
            var requestUri = new Uri($"https://api.meethue.com/v2/oauth2/token");


            var formParameters = new Dictionary<string, string> {
        {"code", code},
        {"grant_type", "authorization_code"}
      };

            var formContent = new FormUrlEncodedContent(formParameters);

            //Do a token request
            var responseTask = await _httpClient.PostAsync(requestUri, formContent).ConfigureAwait(false);
            var responseString = responseTask.Headers.WwwAuthenticate.ToString();
            responseString = responseString.Replace("Digest ", string.Empty);
            string nonce = GetNonce(responseString);

            if (!string.IsNullOrEmpty(nonce))
            {
                //Get token
                var request = new HttpRequestMessage()
                {
                    RequestUri = requestUri,
                    Method = HttpMethod.Post,
                    Content = formContent
                };

                //Build request                
                var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                var accessTokenResponse = await _httpClient.SendAsync(request).ConfigureAwait(false);
                var accessTokenResponseString = await accessTokenResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                var accessToken = JsonConvert.DeserializeObject<AccessTokenResponseV2>(accessTokenResponseString);

                _lastAuthorizationResponse = accessToken;

                return accessToken;

            }


            return null;
        }

        private static string GetNonce(string r)
        {
            //Find the nonce
            int startNonce = r.IndexOf("nonce=") + 7;
            int endNonce = r.IndexOf("\"", startNonce);
            string nonce = r.Substring(startNonce, endNonce - startNonce);

            return nonce;
        }

        public async Task<AccessTokenResponseV2?> RefreshToken(string refreshToken)
        {
            CheckInitialized();

            var requestUri = new Uri("https://api.meethue.com/v2/oauth2/token");

            var formParameters = new Dictionary<string, string> {
        {"refresh_token", refreshToken},
        {"grant_type", "refresh_token"}
      };

            var formContent = new FormUrlEncodedContent(formParameters);

            //Do a token request
            var responseTask = await _httpClient.PostAsync(requestUri, formContent).ConfigureAwait(false);
            var responseString = responseTask.Headers.WwwAuthenticate.ToString();
            responseString = responseString.Replace("Digest ", string.Empty);
            string nonce = GetNonce(responseString);

            if (!string.IsNullOrEmpty(nonce))
            {
                //Get token
                var request = new HttpRequestMessage()
                {
                    RequestUri = requestUri,
                    Method = HttpMethod.Post,
                    Content = formContent
                };

                //Build request
                var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}"));
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                var accessTokenResponse = await _httpClient.SendAsync(request).ConfigureAwait(false);
                var accessTokenResponseString = await accessTokenResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                var accessToken = JsonConvert.DeserializeObject<AccessTokenResponseV2>(accessTokenResponseString);

                _lastAuthorizationResponse = accessToken;

                return accessToken;
            }

            return null;
        }

        /// <summary>
        /// Calculate hash for token request
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="nonce"></param>
        /// <returns></returns>
        private static string CalculateHash(string clientId, string clientSecret, string nonce, string path)
        {
            var HASH1 = MD5.GetMd5String($"{clientId}:oauth2_client@api.meethue.com:{clientSecret}");
            var HASH2 = MD5.GetMd5String("POST:" + path);
            var response = MD5.GetMd5String(HASH1 + ":" + nonce + ":" + HASH2);

            return response;
        }

        /// <summary>
        /// Refreshes the token if needed
        /// </summary>
        /// <returns></returns>
        public async Task<string?> GetValidToken()
        {
            CheckInitialized();
            if (_lastAuthorizationResponse != null)
            {
                if (_lastAuthorizationResponse.AccessTokenExpireTime() > DateTimeOffset.UtcNow.AddMinutes(-5))
                {
                    return _lastAuthorizationResponse.access_token;
                }
                else
                {
                    var newToken = await this.RefreshToken(_lastAuthorizationResponse.refresh_token).ConfigureAwait(false);

                    return newToken?.access_token;
                }
            }

            throw new HueException("Unable to get access token. Access token and Refresh token expired.");
        }

        /// <summary>
        /// Check if the RemoteAuthenticationClient is initialized
        /// </summary>
        private void CheckInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("RemoteAuthenticationClient is not initialized. First call Initialize.");
        }
    }
}
