using Sales.Models;
using Sales.Models.Requests;
using Sales.Models.Responses;

namespace Sales.Services.Constracts
{
    public interface IAuthService
    {
        Task<DtoResult<LoginResponse>> LoginAsync(LoginRequest request);
        Task<DtoResult<LoginResponse>> RefreshAccessTokenAsync(TokenRequest request);
        Task<DtoResult<LoginResponse>> GetValidAccessTokenAsync();
    }
}
