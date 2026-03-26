using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class ParticipationJoinResult
    {
        public bool IsSuccess { get; set; }
        public JoinEventStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public EventDto? Event { get; set; }

        private ParticipationJoinResult() { }

        public static ParticipationJoinResult Success(EventDto eventDto) => new()
        {
            IsSuccess = true,
            Status = JoinEventStatus.Success,
            Message = "Вы успешно записались на событие",
            Event = eventDto
        };

        public static ParticipationJoinResult EventNotFound() => new()
        {
            IsSuccess = false,
            Status = JoinEventStatus.EventNotFound,
            Message = "Событие не найдено"
        };

        public static ParticipationJoinResult EventCancelled() => new()
        {
            IsSuccess = false,
            Status = JoinEventStatus.EventCancelled,
            Message = "Событие отменено"
        };

        public static ParticipationJoinResult EventNotPublished() => new()
        {
            IsSuccess = false,
            Status = JoinEventStatus.EventNotPublished,
            Message = "Событие ещё не опубликовано"
        };
        
        public static ParticipationJoinResult EventAlreadyStarted() => new()
        {
            IsSuccess = false,
            Status = JoinEventStatus.EventAlreadyStarted,
            Message = "Событие уже началось"
        };

        public static ParticipationJoinResult AlreadyJoined() => new()
        {
            IsSuccess = false,
            Status = JoinEventStatus.AlreadyJoined,
            Message = "Вы уже записаны на это событие"
        };
    }
}
