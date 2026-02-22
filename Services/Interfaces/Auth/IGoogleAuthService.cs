using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.DTOs;
using System.Threading.Tasks;

namespace AttandanceSyncApp.Services.Interfaces.Auth
{
    public interface IGoogleAuthService
    {
        string GetAuthorizationUrl(string state);

        ServiceResult<GoogleUserInfo> ExchangeCodeForTokens(string code);

        ServiceResult<GoogleUserInfo> ValidateIdToken(string idToken);

        Task<string> GetEmailFromGoogle(string code);

    }
}

