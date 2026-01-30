using System.Text.Json;

namespace CmsService.Api.Contracts;

public record CmsEventDto(
    string Type,
    string Id,
    int? Version,
    JsonElement? Payload,
    DateTime Timestamp
);