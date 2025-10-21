using Application_lib.Authorization;
using Common_lib.Models.ServiceModels;
using Domain_lib.Constants;
using Domain_lib.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure_lib;

public class AuthorizationService(AppDBContext context, ILdapAuthService ADService) : IAuthService
{
    private readonly AppDBContext _context = context;
    private readonly ILdapAuthService _ldapAuthService = ADService;

    public async Task<Result<TdUser>> LogIn(string login, string password)
    {
        var ldapAuth = _ldapAuthService.Authenticate(login, password);

        if (!ldapAuth.IsSuccess)
            return Result<TdUser>.Error(ldapAuth.InnerError.ErrorCode, ldapAuth.InnerError.Message);

        var user = await _context.TdUsers
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Login == login);

        if (user is null)
        {
            var memberRole = await EnsureRoleExistsAsync(RoleSystemNames.Member, RoleSystemNames.Member);

            user = new TdUser()
            {
                Login = login,
                StatusId = 1,
                RoleId = memberRole.Keyid,
                Role = memberRole,
                UserName = ldapAuth.Data.UserName,
                Email = ldapAuth.Data.Email
            };

            await _context.TdUsers.AddAsync(user);
            //await _context.SaveChangesAsync();
        }

        if (user.Role is null)
        {
            var role = await _context.TdRoles
                .FirstOrDefaultAsync(r => r.Keyid == user.RoleId);

            if (role is null)
            {
                return Result<TdUser>.Error(-1, $"Роль с идентификатором '{user.RoleId}' не найдена для пользователя '{login}'.");
            }

            user.Role = role;
        }

        return Result<TdUser>.Success(user);
    }

    public async Task<Result<TdUser>> LogInWithGit(string login, string name, string email)
    {
        try
        {
            var user = await _context.TdUsers.Include(x => x.Role).FirstOrDefaultAsync(x => x.Login == login);

            if (user is null)
            {
                var memberRole = await EnsureRoleExistsAsync(RoleSystemNames.Member, RoleSystemNames.Member);

                user = new TdUser()
                {
                    Login = login,
                    StatusId = 1,
                    RoleId = memberRole.Keyid,
                    Role = memberRole,
                    UserName = name,
                    Email = email
                };

                await _context.TdUsers.AddAsync(user);
                await _context.SaveChangesAsync();
            }

            return Result<TdUser>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<TdUser>.Error(-1, ex.Message);
        }
    }

    /// <summary>
    /// Обеспечивает наличие роли с указанным системным именем, создавая её при отсутствии.
    /// </summary>
    /// <param name="systemName">Уникальное системное имя роли.</param>
    /// <param name="displayName">Отображаемое имя роли, используемое при создании новой записи.</param>
    private async Task<TdRole> EnsureRoleExistsAsync(string systemName, string? displayName = null, CancellationToken cancellationToken = default)
    {
        var role = await _context.TdRoles.FirstOrDefaultAsync(r => r.SystemName == systemName, cancellationToken);

        if (role is not null)
        {
            return role;
        }

        role = new TdRole
        {
            SystemName = systemName,
            DisplayName = displayName ?? systemName
        };

        await _context.TdRoles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return role;
    }
}