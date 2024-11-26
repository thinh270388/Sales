using Newtonsoft.Json;
using Sales.APIs.Helpers;
using Sales.Models;
using Sales.Models.Requests;
using Sales.Models.Responses;
using Sales.Services.Constracts;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Text;

namespace Sales.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly UrlDictionary urlDictionary = new UrlDictionary();
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<DtoResult<LoginResponse>> LoginAsync(Models.Requests.LoginRequest request)
        {
            DtoResult<LoginResponse> result = new();
            try
            {
                string json = JsonConvert.SerializeObject(request);
                StringContent content = new(json, Encoding.UTF8, "text/json");
                var response = await _httpClient.PostAsync(urlDictionary.UrAuth["login"], content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    result = JsonConvert.DeserializeObject<DtoResult<LoginResponse>>(responseContent)!;

                    SetLoginResponse(result.Result!);
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
        public async Task<DtoResult<LoginResponse>> RefreshAccessTokenAsync(TokenRequest request)
        {
            DtoResult<LoginResponse> result = new();
            try
            {
                string json = JsonConvert.SerializeObject(request);
                StringContent content = new(json, Encoding.UTF8, "text/json");
                var response = await _httpClient.PostAsync(urlDictionary.UrAuth["refresh"], content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    result = JsonConvert.DeserializeObject<DtoResult<LoginResponse>>(responseContent)!;
                    SetLoginResponse(result.Result!);
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
        public async Task<DtoResult<LoginResponse>> GetValidAccessTokenAsync()
        {
            DtoResult<LoginResponse> result = new();
            try
            {
                var loginResponse = GetLoginResponse();
                if (loginResponse == null)
                {
                    result.Success = false;
                    return result;
                }

                if (IsTokenExpired(loginResponse.AccessTokenExpiryTime))
                {
                    if (!IsTokenExpired(loginResponse!.RefreshTokenExpiryTime))
                    {
                        var newLoginResponse = await RefreshAccessTokenAsync(new TokenRequest() { AccessToken = loginResponse.AccessToken!, RefreshToken = loginResponse.RefreshToken! });
                        if (newLoginResponse.Success)
                        {
                            SetLoginResponse(newLoginResponse.Result!);
                            return result;
                        }
                    }
                    result.Success = false;
                    return result;
                }

                result.Success = true;
                result.Result = loginResponse;
                return result;
            }
            catch (Exception ex)
            {
                result.Message = $"Không thể kết nối máy chủ khi xảy ra lỗi: {ex.Message}";
                result.Success = false;
                return result;
            }            
        }

        public void SetLoginResponse(LoginResponse response)
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                using (var stream = new IsolatedStorageFileStream("loginResponse.dat", FileMode.Create, isoStore))
                {
                    var serializer = new DataContractSerializer(typeof(LoginResponse));
                    serializer.WriteObject(stream, response);
                }
            }
        }
        public LoginResponse GetLoginResponse()
        {
            using (var isoStore = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (isoStore.FileExists("loginResponse.dat"))
                {
                    using (var stream = new IsolatedStorageFileStream("loginResponse.dat", FileMode.Open, isoStore))
                    {
                        var serializer = new DataContractSerializer(typeof(LoginResponse));
                        return (LoginResponse)serializer.ReadObject(stream)!;
                    }
                }
            }
            return null!;
        }
        public bool IsTokenExpired(DateTime? expiryTime)
        {
            return DateTime.UtcNow >= expiryTime;
        }
    }
}