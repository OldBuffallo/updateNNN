namespace IRM.Features.ImportExport.Help;

public sealed record ImportTourSnapshot(int Stage, bool IsBusy, bool IsManual, int ContextVersion);
