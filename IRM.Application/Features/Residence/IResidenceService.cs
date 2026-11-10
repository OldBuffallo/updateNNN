using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Residence;

public interface IResidenceService
{
    Task<IReadOnlyList<ResidencePeriodDto>> GetHistoryAsync(int foreignPersonId, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(ResidencePeriodDto residence, CancellationToken cancellationToken = default);
}
