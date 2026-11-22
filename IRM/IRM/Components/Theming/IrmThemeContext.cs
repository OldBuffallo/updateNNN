namespace IRM.Components.Theming;

public sealed record IrmThemeContext(bool IsDarkMode, Func<Task> ToggleAsync);
