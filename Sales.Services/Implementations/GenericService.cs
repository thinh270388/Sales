using Newtonsoft.Json;
using Sales.APIs.Helpers;
using Sales.Models;
using Sales.Services.Constracts;
using System.Net.Http.Headers;
using System.Text;

namespace Sales.Services.Implementations
{
    public class GenericService<T> : IGenericService<T>
    {
        private readonly HttpClient _httpClient;
        private readonly UrlDictionary urlDictionary = new UrlDictionary();
        private readonly IAuthService _authService;
        public GenericService(HttpClient httpClient, IAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }
        public async Task<DtoResult<T>> GetAllAsync(T dto)
        {
            DtoResult<T> result = new();
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

                var response = await _httpClient.GetAsync(urlDictionary.UrlGetAll[dto!.GetType()]);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    result = JsonConvert.DeserializeObject<DtoResult<T>>(responseContent)!;
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
        public async Task<DtoResult<T>> GetOneAsync(T dto)
        {
            DtoResult<T> result = new();
            try
            {
                string json = JsonConvert.SerializeObject(dto);
                StringContent content = new(json, Encoding.UTF8, "text/json");
                var response = await _httpClient.PostAsync(urlDictionary.UrlGetOne[dto!.GetType()], content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    result = JsonConvert.DeserializeObject<DtoResult<T>>(responseContent)!;
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
        public async Task<DtoResult<T>> FindAsync(T dto)
        {
            DtoResult<T> result = new();
            try
            {
                string json = JsonConvert.SerializeObject(dto);
                StringContent content = new(json, Encoding.UTF8, "text/json");
                var response = await _httpClient.PostAsync(urlDictionary.UrlFind[dto!.GetType()], content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    result = JsonConvert.DeserializeObject<DtoResult<T>>(responseContent)!;
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
        public async Task<DtoResult<T>> AddAsync(T dto)
        {
            DtoResult<T> result = new();
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

                string json = JsonConvert.SerializeObject(dto);
                StringContent content = new(json, Encoding.UTF8, "text/json");
                var response = await _httpClient.PostAsync(urlDictionary.UrlAdd[dto!.GetType()], content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    result = JsonConvert.DeserializeObject<DtoResult<T>>(responseContent)!;
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
        public async Task<DtoResult<T>> UpdateAsync(T dto)
        {
            DtoResult<T> result = new();
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

                string json = JsonConvert.SerializeObject(dto);
                StringContent content = new(json, Encoding.UTF8, "text/json");
                var response = await _httpClient.PostAsync(urlDictionary.UrlUpdate[dto!.GetType()], content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    result = JsonConvert.DeserializeObject<DtoResult<T>>(responseContent)!;
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
        public async Task<DtoResult<T>> DeleteAsync(T dto)
        {
            DtoResult<T> result = new();
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

                string json = JsonConvert.SerializeObject(dto);
                StringContent content = new(json, Encoding.UTF8, "text/json");
                var response = await _httpClient.PostAsync(urlDictionary.UrlDelete[dto!.GetType()], content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent != null)
                {
                    result = JsonConvert.DeserializeObject<DtoResult<T>>(responseContent)!;
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