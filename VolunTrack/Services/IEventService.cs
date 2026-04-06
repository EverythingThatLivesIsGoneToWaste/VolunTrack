using VolunTrack.DTO;
using VolunTrack.Enums;

namespace VolunTrack.Services
{
    public interface IEventService
    {
        Task<EventDto> AddAsync(CreateEventDto model);
        Task<EventDto> UpdateStatusAsync(int eventId, EventStatus newStatus, int userId, string userRole);
        Task<List<EventDto>> GetEventsAsync(string type, int userId, string userRole);
        Task<List<ParticipantDto>> GetEventParticipantsAsync(int eventId);

        Task<List<EventPhotoDto>> GetEventPhotosAsync(int eventId);
        Task<EventPhotoDto> AddEventPhotoAsync(int eventId, UploadPhotoDto dto, int userId);
        Task RemoveEventPhotoAsync(int photoId, int userId);

        Task<List<DocumentDto>> GetEventDocumentsAsync(int eventId, int userId, string userRole);
        Task<DocumentDto> AddEventDocumentAsync(int eventId, UploadDocumentDto dto, int userId, string userRole);
        Task RemoveEventDocumentAsync(int documentId, int userId, string userRole);
    }
}
