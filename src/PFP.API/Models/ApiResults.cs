namespace PFP.API.Models;

/// <summary>Factory helpers for the standard API envelope (spec §5.1).</summary>
public static class ApiResults
{
    /// <summary>Successful response with payload.</summary>
    public static ApiResponse<T> Ok<T>(T data, object? meta = null) =>
        new() { Success = true, Data = data, Meta = meta };

    /// <summary>Failed response (controllers should also set the appropriate HTTP status).</summary>
    public static ApiResponse<T> Fail<T>(object error) =>
        new() { Success = false, Error = error };
}
