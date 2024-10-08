using AutoMapper;
using ErrorOr;
using MediatR;
using MessengerAPI.Application.Common.Interfaces.Persistance;
using MessengerAPI.Contracts.Common;
using MessengerAPI.Domain.ChannelAggregate;

namespace MessengerAPI.Application.Channels.Queries.GetChannels;

public class GetChannelsQueryHandler : IRequestHandler<GetChannelsQuery, ErrorOr<List<ChannelSchema>>>
{
    private readonly IChannelRepository _channelRepository;
    private readonly IMapper _mapper;

    public GetChannelsQueryHandler(IChannelRepository channelRepository, IMapper mapper)
    {
        _channelRepository = channelRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all user's channels
    /// </summary>
    /// <param name="request"><see cref="GetChannelsQuery"/></param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    public async Task<ErrorOr<List<ChannelSchema>>> Handle(GetChannelsQuery request, CancellationToken cancellationToken)
    {
        List<Channel> channels = await _channelRepository.GetChannelsByUserIdOrNullAsync(request.Sub, cancellationToken);
        return _mapper.Map<List<ChannelSchema>>(channels);
    }
}
