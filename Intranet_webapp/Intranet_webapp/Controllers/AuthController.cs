using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Intranet_webapp.Controllers
{
    [Route("auth")]
    public class AuthController(ILogger<AuthController> logger, IConfiguration configuration) : Controller
    {
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly string _gitUrl = configuration.GetValue<string>("GitSettings:BaseUrl") ?? "";
        private readonly string _gitAppID = configuration.GetValue<string>("GitSettings:AppID") ?? "";
        private readonly string _gitAppSecret = configuration.GetValue<string>("GitSettings:AppSecret") ?? "";

        [HttpGet("login")]
        public IActionResult Login()
        {
            
            var authUrl = $"{_gitUrl}/oauth/authorize?" +
                          $"client_id={_gitAppID}" +
                          $"&redirect_uri={HttpUtility.UrlEncode($"http://{Request.Host}/auth/callback")}" +
                          $"&response_type=code" +
                          $"&scope=api+read_user+openid+profile+email";

            return Redirect(authUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            // Получаем token и user info
            var token = await ExchangeCodeForToken(code);

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Получаем данные текущего пользователя
            var response = await client.GetAsync($"{_configuration.GetValue<string>("GitSettings:BaseUrl")}/api/v4/user");

            if (!response.IsSuccessStatusCode)
                return Redirect("/");

            var content = await response.Content.ReadAsStringAsync();
            var userData = JsonSerializer.Deserialize<JsonElement>(content);

            Response.Cookies.Append("gitlab_flag", "true");
            Response.Cookies.Append("gitlab_token", token);
            Response.Cookies.Append("gitlab_user_email", userData.GetProperty("email").GetString());
            Response.Cookies.Append("gitlab_user_login", userData.GetProperty("username").GetString());
            Response.Cookies.Append("gitlab_user_name", userData.GetProperty("name").GetString());

            return Redirect("?gitlab=true");
        }

        private async Task<string?> ExchangeCodeForToken(string code)
        {
            using var client = new HttpClient();

            var test = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _gitAppID,
                ["client_secret"] = _gitAppSecret,
                ["code"] = code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = $"http://{Request.Host}/auth/callback"
            });

            var response = await client.PostAsync($"{_gitUrl}/oauth/token", test);

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(content);
            return tokenData.GetProperty("access_token").GetString();
        }
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            // Очищаем куки
            Response.Cookies.Delete("gitlab_flag");
            Response.Cookies.Delete("gitlab_token");
            Response.Cookies.Delete("gitlab_user_email");
            Response.Cookies.Delete("gitlab_user_login");
            Response.Cookies.Delete("gitlab_user_name");

            return Redirect("/");
        }
    }
}
