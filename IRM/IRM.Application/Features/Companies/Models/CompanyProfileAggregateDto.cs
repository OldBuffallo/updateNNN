using IRM.Data.Models;
using IRM.Services;

namespace IRM.Application.Models;

/// <summary>Detached application data for CompanyProfileAggregate; never tracked by EF.</summary>
public sealed class CompanyProfileAggregateDto
{
    public CompanyDto? Company { get; set; }
    public CompanyProfileDto? Profile { get; set; }
    public IReadOnlyList<CompanySiteDto> Sites { get; set; } = [];
    public IReadOnlyList<CompanyAccommodationAgreementDto> AccommodationAgreements { get; set; } = [];
    public IReadOnlyList<CompanyRepresentativeDto> Representatives { get; set; } = [];
    public IReadOnlyList<CompanyLegalDocumentDto> LegalDocuments { get; set; } = [];
    public IReadOnlyList<FamilyVisitorDto> SponsoredFamilyVisitors { get; set; } = [];
}
