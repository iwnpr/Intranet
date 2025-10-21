using System.Threading;
using System.Threading.Tasks;

namespace Application_lib.Email;

public interface IEmailService
{
    Task SendOrganizationRequestAsync(OrganizationRequestMessage request, CancellationToken cancellationToken = default);
}
