using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class ProfileUpdateResult
    {
        public bool IsSuccess { get; set; }
        public ProfileUpdateStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;

        private ProfileUpdateResult() { }

        public static ProfileUpdateResult Success() => new()
        {
            IsSuccess = true,
            Status = ProfileUpdateStatus.Success,
            Message = "Profile updated successfully"
        };

        public static ProfileUpdateResult InvalidCurrentPassword() => new()
        {
            IsSuccess = false,
            Status = ProfileUpdateStatus.InvalidCurrentPassword,
            Message = "Current password is incorrect"
        };

        public static ProfileUpdateResult CurrentPasswordRequired() => new()
        {
            IsSuccess = false,
            Status = ProfileUpdateStatus.CurrentPasswordRequired,
            Message = "Current password is required to set a new password"
        };

        public static ProfileUpdateResult EmailAlreadyExists() => new()
        {
            IsSuccess = false,
            Status = ProfileUpdateStatus.EmailAlreadyExists,
            Message = "This email is already taken"
        };

        public static ProfileUpdateResult UserNotFound() => new()
        {
            IsSuccess = false,
            Status = ProfileUpdateStatus.UserNotFound,
            Message = "User not found"
        };
    }
}
