namespace DiffingApi.Api.Contracts;

/// <summary>
/// Request body for <c>PUT /v1/diff/{id}/left</c> and <c>PUT /v1/diff/{id}/right</c>.
/// </summary>
/// <param name="Data">Base64-encoded binary payload. Required and must be valid base64.</param>
public sealed record DataPayload(string? Data);