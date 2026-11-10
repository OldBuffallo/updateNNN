using IRM.Data.Models;

namespace IRM.Services;

public class SearchResult
{
    public string Keyword { get; set; } = "";
    public List<Employee> Employees { get; set; } = new();
    public List<Company> Companies { get; set; } = new();
    public List<Student> Students { get; set; } = new();
    public int TotalCount => Employees.Count + Companies.Count + Students.Count;
}
