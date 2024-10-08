using AutoMapper;
using MediatR;
using MessengerAPI.Application.Common.Interfaces;
using MessengerAPI.Application.Common.Interfaces.Persistance;
using MessengerAPI.Contracts.Common;
using MessengerAPI.Domain.ChannelAggregate.Events;

namespace MessengerAPI.Application.Channels.Events;

public class MessageUpdatedEventHandler : INotificationHandler<MessageUpdated>
{
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly IChannelRepository _channelRepository;

    public MessageUpdatedEventHandler(IChannelRepository channelRepository, IMapper mapper, INotificationService notificationService)
    {
        _channelRepository = channelRepository;
        _mapper = mapper;
        _notificationService = notificationService;
    }
    /// <summary>
    /// Send a notification about new message to all channel members
    /// </summary>
    /// <param name="notification"><see cref="MessageUpdated"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    public async Task Handle(MessageUpdated notification, CancellationToken cancellationToken)
    {
        List<Guid> recipientIds = await _channelRepository.GetMemberIdsFromChannelByIdOrNullAsync(notification.NewMessage.ChannelId, cancellationToken);

        await _notificationService.MessageUpdated(recipientIds, _mapper.Map<MessageSchema>(notification.NewMessage));
    }
}
