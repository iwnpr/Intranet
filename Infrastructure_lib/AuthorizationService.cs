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

        var user = await _context.TdUsers.FirstOrDefaultAsync(x => x.Login == login);

        if (user is null)
        {
            var memberRole = await FindRoleAsync(RoleSystemNames.Member);

            if (memberRole is null)
                return Result<TdUser>.Error(-1, $"Роль '{RoleSystemNames.Member}' не настроена.");

            user = new TdUser()
            {
                Login = login,
                StatusId = 1,
                RoleId = memberRole.Keyid,
                UserName = ldapAuth.Data.UserName,
                Email = ldapAuth.Data.Email
            };

            await _context.TdUsers.AddAsync(user);
            //await _context.SaveChangesAsync();
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
                var memberRole = await FindRoleAsync(RoleSystemNames.Member);
                if (memberRole is null)
                {
                    return Result<TdUser>.Error(-1, $"Роль '{RoleSystemNames.Member}' не настроена.");
                }

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
    /// Извлекает роль, соответствующую указанному системному имени, если она существует.
    /// </summary>
    /// <param name="systemName">Уникальное системное имя роли</param>
    /// <param name="cancellationToken">Токен, используемый для отмены поиска в базе данных.</param>
    /// <returns>Сущность роли, если она будет найдена; в противном случае null</returns>
    private Task<TdRole?> FindRoleAsync(string systemName, CancellationToken cancellationToken = default)
    {
        return _context.TdRoles.FirstOrDefaultAsync(r => r.SystemName == systemName, cancellationToken);
    }
}