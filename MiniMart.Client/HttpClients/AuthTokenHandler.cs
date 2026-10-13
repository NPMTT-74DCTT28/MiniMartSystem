using System.Net.Http.Headers;
using MiniMart.Client.Security;

namespace MiniMart.Client.HttpClients
{
    /// <summary>
    /// Tự gắn header <c>Authorization: Bearer &lt;token&gt;</c> vào mỗi request dựa trên
    /// phiên đăng nhập hiện tại (<see cref="AuthSession"/>).
    /// </summary>
    /// <remarks>
    /// Đọc token ở thời điểm gửi request (thay vì lúc khởi tạo client) nên mọi instance
    /// <see cref="BaseHttpClient"/> đều dùng đúng token mới nhất sau khi đăng nhập hoặc đổi tài khoản.
    /// </remarks>
    internal sealed class AuthTokenHandler : DelegatingHandler
    {
        public AuthTokenHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var accessToken = AuthSession.AccessToken;

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
