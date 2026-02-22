using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Services.Interfaces.Auth;
using Newtonsoft.Json;

namespace AttandanceSyncApp.Services.Auth
{
    /// <summary>
    /// Service for handling Google OAuth authentication.
    /// </summary>
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public GoogleAuthService()
        {
            _clientId = ConfigurationManager.AppSettings["GoogleClientId"];
            _clientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
            _redirectUri = ConfigurationManager.AppSettings["GoogleRedirectUri"];
        }

        /// <summary>
        /// Generate Google login URL
        /// </summary>
        public string GetAuthorizationUrl(string state)
        {
            var scope = "openid email profile";

            return "https://accounts.google.com/o/oauth2/v2/auth?" +
                   "client_id=" + _clientId +
                   "&redirect_uri=" + Uri.EscapeDataString(_redirectUri) +
                   "&response_type=code" +
                   "&scope=" + Uri.EscapeDataString(scope) +
                   "&state=" + state +
                   "&access_type=offline" +
                   "&prompt=select_account";
        }

        /// <summary>
        /// REQUIRED BY INTERFACE (FIX ERROR)
        /// </summary>
        public ServiceResult<GoogleUserInfo> ExchangeCodeForTokens(string code)
        {
            try
            {
                var idTokenTask = ExchangeCodeForIdToken(code);
                idTokenTask.Wait();

                var idToken = idTokenTask.Result;

                return ValidateIdToken(idToken);
            }
            catch (Exception ex)
            {
                return ServiceResult<GoogleUserInfo>.FailureResult(ex.Message);
            }
        }

        /// <summary>
        /// Exchange code → id_token
        /// </summary>
        public async Task<string> ExchangeCodeForIdToken(string code)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "redirect_uri", _redirectUri },
                    { "grant_type", "authorization_code" }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    content);

                var json = await response.Content.ReadAsStringAsync();

                var tokenResponse =
                    JsonConvert.DeserializeObject<GoogleTokenResponse>(json);

                return tokenResponse.IdToken;
            }
        }
        public async Task<string> GetEmailFromGoogle(string code)
        {
            var token = await GetToken(code);

            var userInfo = await GetUserInfo(token);

            return userInfo.Email;
        }


        private async Task<GoogleUserInfo> GetUserInfo(string idToken)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(
                    $"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");

                var json = await response.Content.ReadAsStringAsync();

                var userInfo = JsonConvert.DeserializeObject<GoogleUserInfo>(json);

                return userInfo;
            }
        }

        private async Task<string> GetToken(string code)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("client_id", ConfigurationManager.AppSettings["GoogleClientId"]),
            new KeyValuePair<string, string>("client_secret", ConfigurationManager.AppSettings["GoogleClientSecret"]),
            new KeyValuePair<string, string>("redirect_uri", ConfigurationManager.AppSettings["GoogleRedirectUri"]),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        });

                var response = await client.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    content);

                var json = await response.Content.ReadAsStringAsync();

                var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(json);

                return tokenResponse.IdToken;
            }
        }


        /// <summary>
        /// Validate ID Token
        /// </summary>
        public ServiceResult<GoogleUserInfo> ValidateIdToken(string idToken)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var response = client.DownloadString(
                        "https://oauth2.googleapis.com/tokeninfo?id_token=" + idToken);

                    var tokenInfo =
                        JsonConvert.DeserializeObject<GoogleTokenInfo>(response);

                    if (tokenInfo.Aud != _clientId)
                    {
                        return ServiceResult<GoogleUserInfo>
                            .FailureResult("Invalid client");
                    }

                    long unixSeconds = long.Parse(tokenInfo.Exp);

                    var expTime =
                        DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

                    if (expTime <= DateTimeOffset.UtcNow)
                    {
                        return ServiceResult<GoogleUserInfo>
                            .FailureResult("Token expired");
                    }

                    var user = new GoogleUserInfo
                    {
                        GoogleId = tokenInfo.Sub,
                        Email = tokenInfo.Email,
                        Name = tokenInfo.Name,
                        Picture = tokenInfo.Picture,
                        EmailVerified = tokenInfo.EmailVerified == "true"
                    };

                    return ServiceResult<GoogleUserInfo>
                        .SuccessResult(user);
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<GoogleUserInfo>
                    .FailureResult(ex.Message);
            }
        }
    }

    internal class GoogleTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }

    internal class GoogleTokenInfo
    {
        [JsonProperty("aud")]
        public string Aud { get; set; }

        [JsonProperty("sub")]
        public string Sub { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("email_verified")]
        public string EmailVerified { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("picture")]
        public string Picture { get; set; }

        [JsonProperty("exp")]
        public string Exp { get; set; }
    }
}
