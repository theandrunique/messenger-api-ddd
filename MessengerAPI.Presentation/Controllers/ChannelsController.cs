using AutoMapper;
using MediatR;
using MessengerAPI.Application.Channels.Commands;
using MessengerAPI.Application.Channels.Commands.CreateMessage;
using MessengerAPI.Application.Channels.Queries.GetChannels;
using MessengerAPI.Application.Channels.Queries.GetMessages;
using MessengerAPI.Domain.ChannelAggregate.ValueObjects;
using MessengerAPI.Domain.UserAggregate.ValueObjects;
using MessengerAPI.Presentation.Common;
using MessengerAPI.Presentation.Schemas.Channels;
using MessengerAPI.Presentation.Schemas.Common;
using Microsoft.AspNetCore.Mvc;

namespace MessengerAPI.Presentation.Controllers;

[Route("channels")]
public class ChannelsController : ApiController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public ChannelsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetChannels()
    {
        var sub = User.GetUserId();

        var query = new GetChannelsQuery(sub);

        var result = await _mediator.Send(query);

        return result.Match(
            success => Ok(_mapper.Map<List<ChannelSchema>>(success)),
            errors => Problem(errors)
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreateChannel([FromBody] CreateChannelRequestSchema schema)
    {
        var sub = User.GetUserId();

        var query = new CreateChannelCommand(
            sub,
            schema.Members.ConvertAll(m => new UserId(m)),
            schema.Title);
        
        var result = await _mediator.Send(query);

        return result.Match(
            success => Ok(_mapper.Map<ChannelSchema>(success)),
            errors => Problem(errors)
        );
    }

    [HttpPost("{channelId}/messages")]
    public async Task<IActionResult> CreateMessage([FromBody] CreateMessageRequestSchema schema, [FromRoute] Guid channelId)
    {
        var sub = User.GetUserId();

        var replyTo = schema.ReplyTo.HasValue ? new MessageId(schema.ReplyTo.Value) : null;

        var command = new CreateMessageCommand(
            sub,
            new ChannelId(channelId),
            schema.Text,
            replyTo,
            schema.Attachments);

        var result = await _mediator.Send(command);

        return result.Match(
            success => Ok(_mapper.Map<MessageSchema>(success)),
            errors => Problem(errors)
        );
    }

    [HttpGet("{channelId}/messages")]
    public async Task<IActionResult> GetMessages([FromRoute] Guid channelId)
    {
        var sub = User.GetUserId();

        var query = new GetMessagesQuery(sub, new ChannelId(channelId));

        var result = await _mediator.Send(query);

        return result.Match(
            success => Ok(_mapper.Map<List<MessageSchema>>(success)),
            errors => Problem(errors)
        );
    }
}
