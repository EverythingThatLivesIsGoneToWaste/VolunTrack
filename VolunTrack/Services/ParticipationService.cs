using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Models;
using VolunTrack.Repositories;

namespace VolunTrack.Services
{
    public class ParticipationService : IParticipationService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IParticipationRepository _participationRepository;

        public ParticipationService(
            IEventRepository eventRepository, 
            IParticipationRepository participationRepository)
        {
            _eventRepository = eventRepository;
            _participationRepository = participationRepository;
        }

        public async Task<ParticipationJoinResult> JoinAsync(int userId, int eventId)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId);
            if (eventEntity == null)
                return ParticipationJoinResult.EventNotFound();

            if (eventEntity.Status == EventStatus.Cancelled)
                return ParticipationJoinResult.EventCancelled();

            if (eventEntity.Status != EventStatus.Published)
                return ParticipationJoinResult.EventNotPublished();

            if (eventEntity.StartDateTime <= DateTime.UtcNow)
                return ParticipationJoinResult.EventAlreadyStarted();

            var existingParticipation = await _participationRepository
                .GetByUserAndEventAsync(userId, eventId);
            if (existingParticipation != null)
                return ParticipationJoinResult.AlreadyJoined();

            var participation = new Participation
            {
                UserId = userId,
                EventId = eventId,
                Status = ParticipationStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _participationRepository.AddAsync(participation);

            return ParticipationJoinResult.Success(EventDto.FromEntity(eventEntity));
        }
    }
}
