using System.Collections.Generic;

namespace Application_lib.Email;

public record OrganizationRequestMessage(
    string OrganizationName,
    string Inn,
    string Ogrn,
    IReadOnlyCollection<string> WhiteListIps,
    IReadOnlyCollection<string> Services);
