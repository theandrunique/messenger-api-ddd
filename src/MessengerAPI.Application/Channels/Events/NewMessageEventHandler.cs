using AutoMapper;
using MediatR;
using MessengerAPI.Application.Common.Interfaces;
using MessengerAPI.Application.Common.Interfaces.Persistance;
using MessengerAPI.Contracts.Common;
using MessengerAPI.Domain.ChannelAggregate.Events;

namespace MessengerAPI.Application.Channels.Events;

public class NewMessageEventHandler : INotificationHandler<NewMessageCreated>
{
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly IChannelRepository _channelRepository;

    public NewMessageEventHandler(INotificationService notificationService, IMapper mapper, IChannelRepository channelRepository)
    {
        _notificationService = notificationService;
        _mapper = mapper;
        _channelRepository = channelRepository;
    }

    /// <summary>
    /// Send a notification about new message to all channel members
    /// </summary>
    /// <param name="notification"><see cref="NewMessageCreated"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    public async Task Handle(NewMessageCreated notification, CancellationToken cancellationToken)
    {
        List<Guid> recipientIds = await _channelRepository.GetMemberIdsFromChannelByIdOrNullAsync(notification.NewMessage.ChannelId, cancellationToken);

        await _notificationService.NewMessageReceived(recipientIds, _mapper.Map<MessageSchema>(notification.NewMessage));
    }
}
