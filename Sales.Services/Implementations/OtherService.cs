using Newtonsoft.Json;
using Sales.APIs.Helpers;
using Sales.Models;
using Sales.Models.DTOs;
using Sales.Services.Constracts;
using System.Net.Http.Headers;

namespace Sales.Services.Implementations
{
    public class OtherService : IOtherService
    {
        private readonly HttpClient _httpClient;
        private readonly UrlDictionary urlDictionary = new UrlDictionary();
        private readonly IAuthService _authService;
        public OtherService(HttpClient httpClient, IAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }
        public async Task<DtoResult<UsersDto>> GetFullUserAsync()
        {
            DtoResult<UsersDto> result = new();
            try
            {
                var loginResponse = await _authService.GetValidAccessTokenAsync();
                if (loginResponse == null || loginResponse.Result == null)
                {
                    result.Message = "Access Token không hợp lệ hoặc đã hết hạn";
                    result.Success = false;
                    return result;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Result.AccessToken);

                var response = await _httpClient.GetAsync(urlDictionary.UrlGetFull[typeof(UsersDto)]);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    result = JsonConvert.DeserializeObject<DtoResult<UsersDto>>(responseContent)!;
                }
                else
                {
                    result.Message = "Không thể kết nối máy chủ";
                    result.Success = false;
                }
            }
            catch (Exception ex)
            {
                result.Message = $"Không thể kết nối máy chủ khi xảy ra lỗi: {ex.Message}";
                result.Success = false;
            }
            return result;
        }
    }
}
