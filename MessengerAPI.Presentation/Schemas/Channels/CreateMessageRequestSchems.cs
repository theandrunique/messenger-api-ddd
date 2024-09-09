namespace MessengerAPI.Presentation.Schemas.Channels;

public record CreateMessageRequestSchema
{
    public string Text { get; init; }
    public int? ReplyTo { get; init; }
    public List<Guid>? Attachments { get; init; }
}
