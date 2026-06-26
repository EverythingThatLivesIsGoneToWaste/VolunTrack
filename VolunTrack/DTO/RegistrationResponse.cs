using VolunTrack.Enums;

namespace VolunTrack.DTO
{
    public class RegistrationResponse
    {
        public bool IsSuccess { get; set; }
        public RegistrationStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserDto? User { get; set; }

        private RegistrationResponse() { }

        public static RegistrationResponse Success(UserDto userDto)
        {
            return new RegistrationResponse
            {
                IsSuccess = true,
                Status = RegistrationStatus.Success,
                Message = "Registration completed successfully",
                User = userDto
            };
        }

        public static RegistrationResponse LoginTaken()
        {
            return new RegistrationResponse
            {
                IsSuccess = false,
                Status = RegistrationStatus.LoginAlreadyExists,
                Message = "This login is already taken",
            };
        }

        public static RegistrationResponse InvalidLogin()
        {
            return new RegistrationResponse
            {
                IsSuccess = false,
                Status = RegistrationStatus.InvalidLogin,
                Message = "Login must be at least 3 characters long",
            };
        }

        public static RegistrationResponse InvalidPassword()
        {
            return new RegistrationResponse
            {
                IsSuccess = false,
                Status = RegistrationStatus.InvalidPassword,
                Message = "Password must be at least 6 characters long",
            };
        }

        public static RegistrationResponse EmailAlreadyExists()
        {
            return new RegistrationResponse
            {
                IsSuccess = false,
                Status = RegistrationStatus.EmailAlreadyExists,
                Message = "This email is already taken",
            };
        }
    }
}
