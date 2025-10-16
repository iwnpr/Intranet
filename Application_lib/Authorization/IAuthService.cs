using Common_lib.Models.ServiceModels;
using Domain_lib.Entities;

namespace Application_lib.Authorization;

public interface IAuthService
{
    Task<Result<TdUser>> LogIn(string login, string password);

    Task<Result<TdUser>> LogInWithGit(string login, string name, string email);
}

