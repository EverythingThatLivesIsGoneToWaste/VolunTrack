using VolunTrack.DTO;
using VolunTrack.Enums;

namespace VolunTrack.Services
{
    public interface IEventService
    {
        Task<EventDto> AddAsync(CreateEventDto model);
        Task<EventDto> UpdateStatusAsync(int eventId, EventStatus newStatus, int userId, string userRole);
    }
}
