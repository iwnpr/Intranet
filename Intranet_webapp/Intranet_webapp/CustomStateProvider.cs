using Domain_lib.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace Intranet_webapp;

class CustomStateProvider(ProtectedSessionStorage sessionStorage, IHttpContextAccessor httpContextAccessor) : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage = sessionStorage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = (await _sessionStorage.GetAsync<TdUser>("login")).Value;

        //Если токен отсуствует либо запрос не прошел, возвращаем анонима
        if (user is null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.NameIdentifier, user.Login)
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        if (!string.IsNullOrWhiteSpace(user.Role?.SystemName))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role.SystemName));

            if (!string.IsNullOrWhiteSpace(user.Role.DisplayName))
            {
                claims.Add(new Claim("role_display_name", user.Role.DisplayName));
            }
        }

        //// Создаем идентификацию для пользователя
        var identity = new ClaimsIdentity(claims, "LDAP");

        var useridentity = new ClaimsPrincipal(identity);
        var UpdatedState = new AuthenticationState(useridentity);

        NotifyAuthenticationStateChanged(Task.FromResult(UpdatedState));

        // Возвращаем статус аутентификации
        return UpdatedState;
    }
}
