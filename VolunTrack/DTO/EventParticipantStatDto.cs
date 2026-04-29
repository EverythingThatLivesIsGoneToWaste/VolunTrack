namespace VolunTrack.DTO
{
    public class EventParticipantStatDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public int ParticipantsCount { get; set; }
    }
}
