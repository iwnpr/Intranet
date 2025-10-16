using Application_lib.Authorization;
using Common_lib.Models.ServiceModels;
using Domain_lib.LDAP;
using Microsoft.Extensions.Configuration;
using System.DirectoryServices.Protocols;
using System.Net;

namespace Infrastructure_lib
{
    public class LdapAuthService(IConfiguration config) : ILdapAuthService
    {
        private readonly string? _ldapServer = config.GetValue<string?>("LDAP:Server");
        private readonly int _port = config.GetValue<int>("LDAP:Port");
        private readonly string? _domain = config.GetValue<string>("LDAP:Domain");
        private readonly string? _searchBaseDn = config.GetValue<string>("LDAP:SearchBaseDn"); // Например: "DC=domain,DC=local"

        public Result<LdapUserInfo> Authenticate(string username, string password)
        {
            var userPrincipalName = string.IsNullOrWhiteSpace(_domain)
                ? username
                : $"{username}@{_domain}";

            try
            {
                using var connection = new LdapConnection(new LdapDirectoryIdentifier(_ldapServer, _port));
                connection.SessionOptions.ProtocolVersion = 3;
                connection.AuthType = AuthType.Basic;

                var credential = new NetworkCredential(userPrincipalName, password);
                connection.Bind(credential); // проверка пароля

                // Поиск информации о пользователе
                string filter = $"(sAMAccountName={username})";
                string[] attributesToLoad = ["mail", "memberOf", "distinguishedName"];

                var request = new SearchRequest(_searchBaseDn, filter, SearchScope.Subtree, attributesToLoad);
                var response = (SearchResponse)connection.SendRequest(request);

                if (response.Entries.Count == 0)
                    return Result<LdapUserInfo>.Error(10, "Пользователь не найден");

                var entry = response.Entries[0];
                var email = entry.Attributes["mail"]?[0]?.ToString();

                //var groups = new List<string>();
                //if (entry.Attributes["memberOf"] != null)
                //{
                //    foreach (var g in entry.Attributes["memberOf"])
                //        groups.Add(g.ToString());
                //}

                return Result<LdapUserInfo>.Success(new LdapUserInfo
                {
                    UserName = username,
                    Email = email,
                    //Groups = groups
                });
            }
            catch (Exception ex)
            {
                return Result<LdapUserInfo>.Error(10, $"LDAP error: {ex.Message}");
            }
        }
    }
}
