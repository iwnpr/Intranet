using Domain_lib.Entities;

namespace Intranet_webapp;

public static class SessionUserMapper
{
    public static TdUser ToSessionUser(TdUser user)
    {
        if (user is null) 
            throw new ArgumentNullException(nameof(user));

        var sessionUser = new TdUser
        {
            Keyid = user.Keyid,
            UserName = user.UserName,
            Login = user.Login,
            Email = user.Email,
            GitId = user.GitId,
            StatusId = user.StatusId,
            IsSystem = user.IsSystem,
            Created = user.Created,
            RoleId = user.RoleId
        };

        if (user.Role is not null)
        {
            sessionUser.Role = new TdRole
            {
                Keyid = user.Role.Keyid,
                SystemName = user.Role.SystemName,
                DisplayName = user.Role.DisplayName,
                TdUsers = new List<TdUser>()
            };
        }
        else
        {
            sessionUser.Role = null!;
        }

        return sessionUser;
    }
}
