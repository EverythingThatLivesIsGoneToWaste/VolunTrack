using VolunTrack.DTO;

namespace VolunTrack.Services
{
    public interface IEventService
    {
        Task<EventDto> AddAsync(CreateEventDto model);
    }
}
