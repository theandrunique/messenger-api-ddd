using MediatR;
using MessengerAPI.Application.Channels.Commands;
using MessengerAPI.Application.Channels.Commands.CreateMessage;
using MessengerAPI.Application.Channels.Commands.EditMessage;
using MessengerAPI.Application.Channels.Queries.GetChannels;
using MessengerAPI.Application.Channels.Queries.GetMessages;
using MessengerAPI.Application.Schemas.Common;
using MessengerAPI.Domain.ChannelAggregate.ValueObjects;
using MessengerAPI.Domain.UserAggregate.ValueObjects;
using MessengerAPI.Presentation.Common;
using MessengerAPI.Presentation.Schemas.Channels;
using Microsoft.AspNetCore.Mvc;

namespace MessengerAPI.Presentation.Controllers;

[Route("channels")]
public class ChannelsController : ApiController
{
    private readonly IMediator _mediator;

    public ChannelsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get user's channels
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="ChannelSchema"/></returns>
    [HttpGet]
    [ProducesResponseType(typeof(ChannelSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChannelsAsync(CancellationToken cancellationToken)
    {
        var identity = User.GetIdentity();

        var query = new GetChannelsQuery(identity.UserId);

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create channel
    /// </summary>
    /// <param name="schema"><see cref="CreateChannelRequestSchema"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="ChannelSchema"/></returns>
    [HttpPost]
    [ProducesResponseType(typeof(ChannelSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateChannelAsync(CreateChannelRequestSchema schema, CancellationToken cancellationToken)
    {
        var identity = User.GetIdentity();

        var query = new CreateChannelCommand(
            identity.UserId,
            schema.members.ConvertAll(m => new UserId(m)),
            schema.type,
            schema.title);

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create message
    /// </summary>
    /// <param name="schema"><see cref="CreateMessageRequestSchema"/></param>
    /// <param name="channelId">Id of channel</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="MessageSchema"/></returns>
    [HttpPost("{channelId}/messages")]
    [ProducesResponseType(typeof(MessageSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateMessageAsync(
        CreateMessageRequestSchema schema,
        Guid channelId,
        CancellationToken cancellationToken)
    {
        var identity = User.GetIdentity();

        var replyTo = schema.replyTo.HasValue ? new MessageId(schema.replyTo.Value) : null;

        var command = new CreateMessageCommand(
            identity.UserId,
            new ChannelId(channelId),
            schema.text,
            replyTo,
            schema.attachments);

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get messages from channel
    /// </summary>
    /// <param name="channelId">Channel id</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <param name="offset">Offset</param>
    /// <param name="limit">Limit</param>
    /// <returns><see cref="MessageSchema"/></returns>
    [HttpGet("{channelId}/messages")]
    [ProducesResponseType(typeof(List<MessageSchema>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessagesAsync(
        Guid channelId,
        CancellationToken cancellationToken,
        int offset = 0,
        int limit = 50)
    {
        var identity = User.GetIdentity();

        var query = new GetMessagesQuery(identity.UserId, new ChannelId(channelId), offset, limit);

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Edit message
    /// </summary>
    /// <param name="schema"><see cref="CreateMessageRequestSchema"/></param>
    /// <param name="channelId">Channel id</param>
    /// <param name="messageId">Message id</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns></returns>
    [HttpPut("{channelId}/messages/{messageId}")]
    [ProducesResponseType(typeof(MessageSchema), StatusCodes.Status200OK)]
    public async Task<IActionResult> EditMessageAsync(
        CreateMessageRequestSchema schema,
        Guid channelId,
        long messageId,
        CancellationToken cancellationToken)
    {
        var identity = User.GetIdentity();

        var replyTo = schema.replyTo.HasValue ? new MessageId(schema.replyTo.Value) : null;

        var command = new EditMessageCommand(
            new MessageId(messageId),
            identity.UserId,
            new ChannelId(channelId),
            schema.text,
            replyTo,
            schema.attachments);

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            success => Ok(success),
            errors => Problem(errors)
        );
    }
}
