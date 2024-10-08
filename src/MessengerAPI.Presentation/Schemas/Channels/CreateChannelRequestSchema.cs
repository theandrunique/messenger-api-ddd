using MessengerAPI.Domain.ChannelAggregate.ValueObjects;

namespace MessengerAPI.Presentation.Schemas.Channels;

/// <summary>
/// Request schema for create channel
/// </summary>
public record CreateChannelRequestSchema
{
    /// <summary>
    /// Channel title
    /// </summary>
    public string? title { get; init; }
    /// <summary>
    /// Channel type
    /// </summary>
    public ChannelType type { get; init; }
    /// <summary>
    /// List of members
    /// </summary>
    public List<Guid> members { get; init; }
}
