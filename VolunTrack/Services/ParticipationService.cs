using VolunTrack.DTO;
using VolunTrack.Enums;
using VolunTrack.Exceptions;
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

        public async Task<ParticipationDto> UpdateHoursAsync(int participationId, UpdateHoursDto dto, int currentUserId, string currentUserRole)
        {
            var participationEntity = await _participationRepository.GetByIdAsync(participationId)
                ?? throw new NotFoundException($"Participation {participationId} not found");

            var eventEntity = await _eventRepository.GetByIdAsync(participationEntity.EventId)
                ?? throw new NotFoundException($"Event {participationEntity.EventId} not found");

            if (eventEntity.Status != EventStatus.Completed)
                throw new ArgumentException("Hours can only be updated for completed events");

            if (participationEntity.CheckInTime == null || participationEntity.CheckOutTime == null)
                throw new TimeNotRecordedException("Hours need to be recorded first");

            if (participationEntity.Status == ParticipationStatus.Approved)
                throw new InvalidOperationException("Cannot edit approved hours");

            bool isAdmin = currentUserRole == nameof(UserRole.Administrator);
            bool isCreator = currentUserRole == nameof(UserRole.EventCoordinator) && eventEntity.CreatedByUserId == currentUserId;
            bool isLeader = await _eventRepository.IsUserLeaderOfEventAsync(eventEntity.Id, currentUserId);

            if (!isAdmin && !isCreator && !isLeader)
                throw new Exceptions.UnauthorizedAccessException($"No permission to update hours");

            if (dto.CheckOutTime <= dto.CheckInTime)
                throw new ArgumentException("Check-out must be after check-in");

            if (dto.CheckInTime < eventEntity.StartDateTime || dto.CheckOutTime > eventEntity.EndDateTime)
                throw new ArgumentException("Time must be within event boundaries");

            bool hasChanges = dto.CheckInTime != participationEntity.CheckInTime ||
                  dto.CheckOutTime != participationEntity.CheckOutTime;

            if (hasChanges)
            {
                participationEntity.CheckInTime = dto.CheckInTime;
                participationEntity.CheckOutTime = dto.CheckOutTime;
                participationEntity.HoursModerated = true;
                participationEntity.TotalHours = (decimal)dto.CheckOutTime.Subtract(dto.CheckInTime).TotalHours;

                await _participationRepository.UpdateAsync(participationEntity);
            }

            return ParticipationDto.FromEntity(participationEntity);
        }

        public async Task<ParticipationDto> ConfirmHoursAsync(int participationId, int currentUserId, string currentUserRole)
        {
            var participationEntity = await _participationRepository.GetByIdAsync(participationId)
                ?? throw new NotFoundException($"Participation {participationId} not found");

            var eventEntity = await _eventRepository.GetByIdAsync(participationEntity.EventId)
                ?? throw new NotFoundException($"Event {participationEntity.EventId} not found");

            if (eventEntity.Status != EventStatus.Completed)
                throw new ArgumentException("Hours can only be confirmed for completed events");

            if (participationEntity.CheckInTime == null || participationEntity.CheckOutTime == null)
                throw new TimeNotRecordedException("Hours need to be recorded first");

            if (participationEntity.Status == ParticipationStatus.Approved)
                throw new InvalidOperationException("Cannot confirm already approved hours");

            bool isAdmin = currentUserRole == nameof(UserRole.Administrator);
            bool isCreator = currentUserRole == nameof(UserRole.EventCoordinator) && eventEntity.CreatedByUserId == currentUserId;
            bool isLeader = await _eventRepository.IsUserLeaderOfEventAsync(eventEntity.Id, currentUserId);

            if (!isAdmin && !isCreator && !isLeader)
                throw new Exceptions.UnauthorizedAccessException($"No permission to update hours");
            
            if (isAdmin || isCreator)
                participationEntity.IsConfirmedByCoordinator = true;
            if (isAdmin || isLeader)
                participationEntity.IsConfirmedByLeader = true;

            if (participationEntity.IsConfirmedByLeader && participationEntity.IsConfirmedByCoordinator)
                participationEntity.Status = ParticipationStatus.Approved;

            await _participationRepository.UpdateAsync(participationEntity);

            return ParticipationDto.FromEntity(participationEntity);
        }

        public async Task<ParticipationDto> RejectHoursAsync(int participationId, int currentUserId, string currentUserRole)
        {
            var participationEntity = await _participationRepository.GetByIdAsync(participationId)
                ?? throw new NotFoundException($"Participation {participationId} not found");

            var eventEntity = await _eventRepository.GetByIdAsync(participationEntity.EventId)
                ?? throw new NotFoundException($"Event {participationEntity.EventId} not found");

            if (eventEntity.Status != EventStatus.Completed)
                throw new ArgumentException("Hours can only be rejected for completed events");

            if (participationEntity.CheckInTime == null || participationEntity.CheckOutTime == null)
                throw new TimeNotRecordedException("Hours need to be recorded first");

            bool isAdmin = currentUserRole == nameof(UserRole.Administrator);
            bool isCreator = currentUserRole == nameof(UserRole.EventCoordinator) && eventEntity.CreatedByUserId == currentUserId;
            bool isLeader = await _eventRepository.IsUserLeaderOfEventAsync(eventEntity.Id, currentUserId);

            if (!isAdmin && !isCreator && !isLeader)
                throw new Exceptions.UnauthorizedAccessException($"No permission to update hours");

            if (isAdmin || isCreator)
                participationEntity.IsConfirmedByCoordinator = false;

            if (isAdmin || isLeader)
                participationEntity.IsConfirmedByLeader = false;

            participationEntity.Status = ParticipationStatus.Pending;
            if (!participationEntity.IsConfirmedByCoordinator && !participationEntity.IsConfirmedByLeader)
                participationEntity.Status = ParticipationStatus.Rejected;

            await _participationRepository.UpdateAsync(participationEntity);

            return ParticipationDto.FromEntity(participationEntity);
        }
    }
}
