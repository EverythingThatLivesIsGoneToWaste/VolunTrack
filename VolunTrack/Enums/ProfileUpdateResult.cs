namespace VolunTrack.Enums
{
    public enum ProfileUpdateStatus
    {
        Success = 0,
        InvalidCurrentPassword = 1,
        CurrentPasswordRequired = 2,
        EmailAlreadyExists = 3,
        UserNotFound = 4,
    }
}
