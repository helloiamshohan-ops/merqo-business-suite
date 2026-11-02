namespace Merqo.Infrastructure.Logging;

public enum LogLevel
{
    Debug = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

/// <summary>Application-wide structured logging contract. Technical logs never hold credentials or secrets.</summary>
public interface IAppLogger
{
    void Write(LogLevel level, string category, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? data = null);
    void Debug(string category, string message, IReadOnlyDictionary<string, object?>? data = null);
    void Info(string category, string message, IReadOnlyDictionary<string, object?>? data = null);
    void Warn(string category, string message, IReadOnlyDictionary<string, object?>? data = null);
    void Error(string category, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? data = null);
    void Critical(string category, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? data = null);
}

public static class LogCategories
{
    public const string Startup = "startup";
    public const string Database = "database";
    public const string Sale = "sale";
    public const string Purchase = "purchase";
    public const string Payment = "payment";
    public const string Inventory = "inventory";
    public const string Accounting = "accounting";
    public const string Security = "security";
    public const string Licensing = "licensing";
    public const string Printing = "printing";
    public const string Export = "export";
    public const string Backup = "backup";
    public const string License = "license";
    public const string Ui = "ui";
}
