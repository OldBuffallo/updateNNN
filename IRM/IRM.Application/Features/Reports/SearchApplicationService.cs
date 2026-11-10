using static IRM.Application.Models.DtoMapper;
using IRM.Data.Models;
using IRM.Services;
using IRM.Application.Abstractions;
using IRM.Application.Models;
using System.Security.Claims;

namespace IRM.Application.Features.Reports;

public sealed class SearchApplicationService(ISearchStore store, IServiceAuthorizationGuard guard) : ISearchService
{
    private readonly ISearchStore _store = store;
    private readonly IServiceAuthorizationGuard _guard = guard;

    public async Task<SearchResultDto> SearchAsync(string keyword, string filter = "all")
    {
        var result = await _store.SearchAsync(keyword, filter);
        return ToDto(result);
    }
}
