using Application_lib.Authorization;
using Common_lib.Models.ServiceModels;
using Domain_lib.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure_lib
{
    public class AuthorizationService(AppDBContext context, ILdapAuthService ADService) : IAuthService
    {
        private readonly AppDBContext _context = context;
        private readonly ILdapAuthService _ldapAuthService = ADService;

        public async Task<Result<TdUser>> LogIn(string login, string password)
        {
            var ldapAuth = _ldapAuthService.Authenticate(login, password);

            if (ldapAuth.IsSuccess)
            {
                var user = await _context.TdUsers.FirstOrDefaultAsync(x => x.Login == login);

                if (user is null)
                {
                    user = new TdUser()
                    {
                        Login = login,
                        StatusId = 1,
                        UserName = ldapAuth.Data.UserName,
                        Email = ldapAuth.Data.Email
                    };

                    await _context.TdUsers.AddAsync(user);
                    //await _context.SaveChangesAsync();
                }

                return Result<TdUser>.Success(user);
            }
            else
            {
                return Result<TdUser>.Error(ldapAuth.InnerError.ErrorCode, ldapAuth.InnerError.Message);
            }
        }

        public async Task<Result<TdUser>> LogInWithGit(string login, string name, string email)
        {
            try
            {
                var user = await _context.TdUsers.FirstOrDefaultAsync(x => x.Login == login);
                if (user is null)
                {
                    user = new TdUser()
                    {
                        Login = login,
                        StatusId = 1,
                        UserName = name,
                        Email = email
                    };

                    await _context.TdUsers.AddAsync(user);
                    await _context.SaveChangesAsync();
                }

                return Result<TdUser>.Success(user);
            }
            catch(Exception ex)
            {
                return Result<TdUser>.Error(-1, ex.Message);
            }
        }
    }
}
