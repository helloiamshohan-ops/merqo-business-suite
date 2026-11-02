namespace Merqo.Application.Common;

/// <summary>
/// Result of a business operation. <see cref="ErrorCode"/> is a stable machine key that the presentation
/// layer maps to a Bangla message; the UI never shows a raw exception message for expected failures.
/// </summary>
public sealed record OperationResult(bool Success, string? ErrorCode = null, string? DeveloperMessage = null)
{
    public static OperationResult Ok() => new(true);
    public static OperationResult Fail(string code, string message) => new(false, code, message);
}

public sealed record OperationResult<T>(bool Success, T? Value, string? ErrorCode = null, string? DeveloperMessage = null)
{
    public static OperationResult<T> Ok(T value) => new(true, value);
    public static OperationResult<T> Fail(string code, string message) => new(false, default, code, message);
}

public static class ApplicationErrorCodes
{
    public const string NotFound = "APP.NOT_FOUND";
    public const string ValidationFailed = "APP.VALIDATION_FAILED";
    public const string Duplicate = "APP.DUPLICATE";
    public const string InsufficientStock = "APP.INSUFFICIENT_STOCK";
    public const string PermissionDenied = "APP.PERMISSION_DENIED";
    public const string DuplicateSubmission = "APP.DUPLICATE_SUBMISSION";
    public const string InvalidState = "APP.INVALID_STATE";
    public const string NoActiveSession = "APP.NO_ACTIVE_SESSION";
}
