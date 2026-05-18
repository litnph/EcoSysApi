using System.Text.Json;

namespace PFP.IntegrationTests.Auth;

internal static class AuthApiWire
{
    internal static async Task<(string AccessToken, string RefreshToken)> ReadTokensAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;
        var data = root.TryGetProperty("data", out var envelope) && envelope.ValueKind == JsonValueKind.Object
            ? envelope
            : root;

        var access = data.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Missing accessToken.");
        var refresh = data.GetProperty("refreshToken").GetString()
            ?? throw new InvalidOperationException("Missing refreshToken.");
        return (access, refresh);
    }

    internal static async Task<(string AccessToken, Guid OrganizationId, Guid PersonalSpaceId)> ReadRegisterPayloadAsync(
        HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var data = doc.RootElement.GetProperty("data");
        var access = data.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Missing accessToken.");
        var orgId = data.GetProperty("organizationId").GetGuid();
        var spaceId = data.GetProperty("personalSpaceId").GetGuid();
        return (access, orgId, spaceId);
    }
}
