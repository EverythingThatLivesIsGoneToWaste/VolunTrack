namespace VolunTrack.Exceptions
{
    public class EventLeaderLimitExceededException : Exception
    {
        public int MaxAllowed { get; }
        public int EventId { get; }

        public EventLeaderLimitExceededException(int eventId, int maxAllowed)
            : base($"Event {eventId} already has the maximum allowed number of leaders ({maxAllowed})")
        {
            EventId = eventId;
            MaxAllowed = maxAllowed;
        }
    }
}
