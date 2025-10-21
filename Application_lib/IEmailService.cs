using Domain_lib.Models;

namespace Application_lib;

public interface IEmailService
{
    Task SendOrganizationRequestAsync(OrganizationRequestMessage request, CancellationToken cancellationToken = default);
}
