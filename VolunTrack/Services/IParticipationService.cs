using VolunTrack.DTO;

namespace VolunTrack.Services
{
    public interface IParticipationService
    {
        Task<ParticipationJoinResult> JoinAsync(int userId, int EventId);
        Task<ParticipationDto> UpdateHoursAsync(int participationId, UpdateHoursDto dto, int currentUserId, string currentUserRole);
        Task<ParticipationDto> ConfirmHoursAsync(int participationId, int currentUserId, string currentUserRole);
        Task<ParticipationDto> RejectHoursAsync(int participationId, int currentUserId, string currentUserRole);
    }
}
