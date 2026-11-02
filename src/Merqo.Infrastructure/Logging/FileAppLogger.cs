using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Merqo.Infrastructure.Logging;

/// <summary>
/// Dependency-free, append-only JSON-lines logger with daily rolling files and retention.
/// Secrets are scrubbed before anything is written to disk.
/// </summary>
public sealed partial class FileAppLogger : IAppLogger
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private readonly object _gate = new();
    private readonly string _directory;
    private readonly int _retentionDays;
    private readonly LogLevel _minimumLevel;

    public FileAppLogger(string logDirectory, LogLevel minimumLevel = LogLevel.Information, int retentionDays = 60)
    {
        _directory = logDirectory;
        _minimumLevel = minimumLevel;
        _retentionDays = retentionDays;
        Directory.CreateDirectory(_directory);
        PruneOldFiles();
    }

    public void Write(LogLevel level, string category, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? data = null)
    {
        if (level < _minimumLevel) return;

        var payload = new Dictionary<string, object?>
        {
            ["ts"] = DateTime.UtcNow.ToString("O"),
            ["level"] = level.ToString(),
            ["category"] = category,
            ["message"] = Scrub(message)
        };

        if (data is { Count: > 0 })
        {
            var scrubbed = new Dictionary<string, object?>();
            foreach (var pair in data)
            {
                var key = pair.Key;
                scrubbed[key] = SensitiveKeyRegex().IsMatch(key) ? "[redacted]" : ScrubValue(pair.Value);
            }
            payload["data"] = scrubbed;
        }

        if (exception is not null)
        {
            payload["exception"] = exception.GetType().FullName;
            payload["exceptionMessage"] = Scrub(exception.Message);
            payload["stack"] = exception.StackTrace;
        }

        try
        {
            var line = JsonSerializer.Serialize(payload, JsonOptions);
            lock (_gate)
            {
                var file = Path.Combine(_directory, $"tahabil-{DateTime.UtcNow:yyyyMMdd}.log");
                using var stream = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream, new UTF8Encoding(false));
                writer.WriteLine(line);
            }
        }
        catch (IOException)
        {
            // Logging must never take the application down.
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    public void Debug(string category, string message, IReadOnlyDictionary<string, object?>? data = null) => Write(LogLevel.Debug, category, message, null, data);
    public void Info(string category, string message, IReadOnlyDictionary<string, object?>? data = null) => Write(LogLevel.Information, category, message, null, data);
    public void Warn(string category, string message, IReadOnlyDictionary<string, object?>? data = null) => Write(LogLevel.Warning, category, message, null, data);
    public void Error(string category, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? data = null) => Write(LogLevel.Error, category, message, exception, data);
    public void Critical(string category, string message, Exception? exception = null, IReadOnlyDictionary<string, object?>? data = null) => Write(LogLevel.Critical, category, message, exception, data);

    private static object? ScrubValue(object? value) => value switch
    {
        null => null,
        string s => Scrub(s),
        _ => value
    };

    private static string Scrub(string message) => SecretValueRegex().Replace(message ?? string.Empty, "$1[redacted]");

    private void PruneOldFiles()
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-Math.Abs(_retentionDays));
            foreach (var file in Directory.EnumerateFiles(_directory, "tahabil-*.log"))
            {
                if (File.GetLastWriteTimeUtc(file) < cutoff) File.Delete(file);
            }
        }
        catch (IOException)
        {
        }
    }

    [GeneratedRegex(@"(?i)(password|passwd|pwd|secret|token|pin|private[_\- ]?key)\s*[=:]\s*\S+")]
    private static partial Regex SecretValueRegex();

    [GeneratedRegex(@"(?i)(password|passwd|pwd|secret|token|pin|private[_\- ]?key)")]
    private static partial Regex SensitiveKeyRegex();
}
