namespace CmsService.Api.Contracts;

public record PublishEntityRequest(
    string EntityId,
    int Version,
    string Payload,
    DateTime Timestamp
);