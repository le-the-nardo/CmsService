namespace CmsService.Api.Contracts;

public record EntityResponse(
    string Id,
    int? LatestPublishedVersion,
    string? Payload
);