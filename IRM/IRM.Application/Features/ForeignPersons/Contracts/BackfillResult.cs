using IRM.Data.Models;

namespace IRM.Services;

public sealed record BackfillResult(int Linked, int Created, int Issues);
