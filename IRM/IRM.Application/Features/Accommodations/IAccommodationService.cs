using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Accommodations;

public interface IAccommodationService
{
    Task<IReadOnlyList<AccommodationDto>> SearchAsync(string? search, int? administrativeUnitId, int? companyId, DateTime? asOfDate, CancellationToken cancellationToken = default);
    Task<int> SaveAsync(AccommodationDto accommodation, CancellationToken cancellationToken = default);
    Task<int> SaveAgreementAsync(CompanyAccommodationAgreementDto agreement, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
}
