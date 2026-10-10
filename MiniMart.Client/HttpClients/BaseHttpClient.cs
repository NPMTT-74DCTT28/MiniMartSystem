using System.Net.Http.Json;

namespace MiniMart.Client.HttpClients
{
    public class BaseHttpClient
    {
        protected readonly HttpClient _httpClient;

        // Địa chỉ base của Web API (Thay đổi cổng chạy thực tế của bạn)
        private const string BaseAddress = "https://localhost:7001/";

        public BaseHttpClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseAddress)
            };
        }

        // Phương thức dùng chung để GET danh sách dữ liệu
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            return await _httpClient.GetFromJsonAsync<T>(endpoint);
        }

        // Phương thức dùng chung để POST dữ liệu
        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
        {
            return await _httpClient.PostAsJsonAsync(endpoint, data);
        }

        // Phương thức dùng chung để PUT dữ liệu
        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
        {
            return await _httpClient.PutAsJsonAsync(endpoint, data);
        }

        // Phương thức dùng chung để DELETE dữ liệu
        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            return await _httpClient.DeleteAsync(endpoint);
        }
    }
}