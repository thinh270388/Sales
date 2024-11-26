using Sales.Models;
using Sales.Models.Requests;
using Sales.Models.Responses;

namespace Sales.APIs.Repositories.Constracts
{
    public interface IAuthRepository
    {
        DtoResult<LoginResponse> Login(Sales.Models.Requests.LoginRequest request);
        DtoResult<LoginResponse> RefreshAccessToken(TokenRequest request);
        DtoResult<LoginResponse> RevokeAccessTokenAll();
        DtoResult<LoginResponse> RevokeAccessTokenByEmail(string email);
    }
}