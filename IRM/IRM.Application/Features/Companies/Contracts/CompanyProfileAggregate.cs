using IRM.Data.Models;

namespace IRM.Services;

public sealed class CompanyProfileAggregate
{
    public Company? Company { get; set; }
    public CompanyProfile? Profile { get; set; }
    public IReadOnlyList<CompanySite> Sites { get; set; } = [];
    public IReadOnlyList<CompanyAccommodationAgreement> AccommodationAgreements { get; set; } = [];
    public IReadOnlyList<CompanyRepresentative> Representatives { get; set; } = [];
    public IReadOnlyList<CompanyLegalDocument> LegalDocuments { get; set; } = [];
    public IReadOnlyList<FamilyVisitor> SponsoredFamilyVisitors { get; set; } = [];
}
