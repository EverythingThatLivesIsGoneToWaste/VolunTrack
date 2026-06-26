namespace VolunTrack.Exceptions
{
    public class TimeAlreadyRecordedException : Exception
    {
        public TimeAlreadyRecordedException(string message): base(message) { }
    }
}
