using IRM.Data.Models;
using IRM.Services;
using System.Security.Claims;

namespace IRM.Application.Abstractions;

/// <summary>Materialized operations for Reports; query execution and transactions remain in Infrastructure.</summary>
public interface ISearchStore
{
    Task<SearchResult> SearchAsync(string keyword, string filter = "all");
}
