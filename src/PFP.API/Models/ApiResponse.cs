namespace PFP.API.Models;

/// <summary>Standard API envelope (spec §5.1 — <c>success</c>, <c>data</c>, <c>error</c>, <c>meta</c>).</summary>
/// <typeparam name="T">Payload type carried in <see cref="Data"/>.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary><c>true</c> when the request succeeded.</summary>
    public bool Success { get; init; } = true;

    /// <summary>Response body; <c>null</c> on failure.</summary>
    public T? Data { get; init; }

    /// <summary>Error descriptor; <c>null</c> on success.</summary>
    public object? Error { get; init; }

    /// <summary>Optional metadata (pagination, correlation ids, …).</summary>
    public object? Meta { get; init; }
}
