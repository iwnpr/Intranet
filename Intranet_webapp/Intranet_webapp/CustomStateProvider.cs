using System.Security.Claims;
using Application_lib.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using static Intranet_webapp.Components.Pages.Login;

namespace Intranet_webapp;

class CustomStateProvider(ProtectedSessionStorage sessionStorage, IHttpContextAccessor httpContextAccessor, IAuthService loginService) : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage = sessionStorage;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IAuthService _loginService = loginService;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        //var httpContext = _httpContextAccessor.HttpContext;
        //var flag = httpContext?.Request.Cookies["gitlab_flag"];

        var user = (await _sessionStorage.GetAsync<UserFrontend>("login")).Value;

        Console.WriteLine($"HttpContext.User.IsAuthenticated = {_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated}");

        //Если токен отсуствует либо запрос не прошел, возвращаем анонима
        if (user is null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Создаем идентификацию для пользователя
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, user.UserName)
        //], !string.IsNullOrWhiteSpace(flag) ? "Gitlab" : "LDAP");
        ], "LDAP");

        var useridentity = new ClaimsPrincipal(identity);
        var UpdatedState = new AuthenticationState(useridentity);

        NotifyAuthenticationStateChanged(Task.FromResult(UpdatedState));

        // Возвращаем статус аутентификации
        return UpdatedState;
    }
}
