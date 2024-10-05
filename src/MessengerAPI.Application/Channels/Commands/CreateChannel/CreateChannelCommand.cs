using ErrorOr;
using MediatR;
using MessengerAPI.Application.Schemas.Common;
using MessengerAPI.Domain.ChannelAggregate.ValueObjects;

namespace MessengerAPI.Application.Channels.Commands;

/// <summary>
/// Create new channel command
/// </summary>
/// <param name="Sub"><see cref="UserId"/></param>
/// <param name="Members">List of user ids</param>
/// <param name="Type">Type of channel</param>
/// <param name="Title">Title of channel</param>
public record CreateChannelCommand(
    Guid Sub,
    List<Guid> Members,
    ChannelType Type,
    string? Title
) : IRequest<ErrorOr<ChannelSchema>>;
