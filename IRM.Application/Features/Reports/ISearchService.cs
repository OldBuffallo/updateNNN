using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Reports;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(string keyword, string filter = "all");
}
