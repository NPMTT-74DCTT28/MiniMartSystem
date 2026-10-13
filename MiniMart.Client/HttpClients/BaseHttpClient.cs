using System.Net.Http.Json;

namespace MiniMart.Client.HttpClients
{
    public class BaseHttpClient
    {
        /// <summary>
        /// Cổng chuẩn của API khi chạy ở máy cá nhân (xem <c>MiniMart.API/Properties/launchSettings.json</c>):
        /// profile "http" bind <c>http://localhost:5000</c>, profile "https" bind
        /// <c>https://localhost:5001</c> và <c>http://localhost:5000</c>.
        /// Vì vậy địa chỉ http vẫn dùng được cho cả hai profile: khi chạy profile "https",
        /// API tự chuyển hướng (HTTPS Redirection) request từ http:5000 sang https:5001.
        /// </summary>
        private const string DefaultBaseAddress = "http://localhost:5000/";

        /// <summary>
        /// Biến môi trường cho phép đổi địa chỉ API mà không cần build lại Client,
        /// VD khi muốn gọi thẳng HTTPS: <c>set MINIMART_API_BASE_URL=https://localhost:5001/</c>
        /// hoặc khi trỏ vào DB/API tập trung qua Tailscale.
        /// </summary>
        public const string BaseAddressEnvironmentVariable = "MINIMART_API_BASE_URL";

        protected readonly HttpClient _httpClient;

        /// <summary>Khởi tạo client dùng địa chỉ API mặc định (http://localhost:5000/).</summary>
        public BaseHttpClient() : this(baseAddress: null)
        {
        }

        /// <summary>
        /// Khởi tạo client với địa chỉ API chỉ định.
        /// Truyền <c>null</c> để dùng biến môi trường <see cref="BaseAddressEnvironmentVariable"/>
        /// hoặc địa chỉ mặc định.
        /// </summary>
        public BaseHttpClient(string? baseAddress)
        {
            // AuthTokenHandler tự gắn Bearer token của phiên đăng nhập hiện tại vào mỗi request.
            var handler = new AuthTokenHandler(new HttpClientHandler());

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(ResolveBaseAddress(baseAddress))
            };
        }

        /// <summary>Xác định địa chỉ API theo thứ tự: tham số truyền vào, biến môi trường, mặc định.</summary>
        public static string ResolveBaseAddress(string? baseAddress = null)
        {
            if (!string.IsNullOrWhiteSpace(baseAddress))
            {
                return EnsureTrailingSlash(baseAddress);
            }

            var fromEnvironment = Environment.GetEnvironmentVariable(BaseAddressEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(fromEnvironment))
            {
                return EnsureTrailingSlash(fromEnvironment);
            }

            return DefaultBaseAddress;
        }

        private static string EnsureTrailingSlash(string address)
        {
            var trimmed = address.Trim();
            return trimmed.EndsWith('/') ? trimmed : trimmed + "/";
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

        // Phương thức dùng chung để PATCH dữ liệu
        public async Task<HttpResponseMessage> PatchAsync<T>(string endpoint, T data)
        {
            return await _httpClient.PatchAsJsonAsync(endpoint, data);
        }

        // Phương thức dùng chung để DELETE dữ liệu
        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            return await _httpClient.DeleteAsync(endpoint);
        }

        /// <summary>
        /// Ném ngoại lệ kèm thông báo lỗi do API trả về (nếu có) để màn hình hiển thị qua
        /// <c>MessageBox</c> theo quy ước xử lý lỗi trong <c>docs/AI_CONVENTIONS.md</c>.
        /// </summary>
        public static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var errorMessage = await ReadErrorMessageAsync(response);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Đọc thông báo lỗi từ response: ưu tiên trường <c>message</c> mà API trả về,
        /// nếu không có thì dùng toàn bộ nội dung response.
        /// </summary>
        public static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(content);

                if (document.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object &&
                    document.RootElement.TryGetProperty("message", out var messageElement) &&
                    messageElement.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    return messageElement.GetString() ?? string.Empty;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                // Response không phải JSON (VD: trang lỗi của server) thì trả về nguyên nội dung.
            }

            return content;
        }
    }
}
