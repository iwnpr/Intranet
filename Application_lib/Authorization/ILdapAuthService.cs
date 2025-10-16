using Common_lib.Models.ServiceModels;
using Domain_lib.LDAP;

namespace Application_lib.Authorization;
public interface ILdapAuthService
{
    Result<LdapUserInfo> Authenticate(string username, string password);
}

