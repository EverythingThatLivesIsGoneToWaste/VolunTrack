using VolunTrack.DTO;

namespace VolunTrack.Services
{
    public interface IParticipationService
    {
        Task<ParticipationJoinResult> JoinAsync(int userId, int EventId);
    }
}
