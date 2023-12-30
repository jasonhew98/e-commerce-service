namespace Domain
{
    public class DomainError
    {
        public readonly string Code;
        public readonly string Message;

        private DomainError(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public static DomainError New(string code, string message)
        {
            return new DomainError(code, message);
        }
    }

    public class BusinessError
    {
        #region Generic Error 1001 - 1100
        public static class UnauthorizedAccess
        {
            public static string Code = "1001";
            public static string Message = "Unable to access resource due to unauthorized access.";
            public static DomainError Error() => DomainError.New(Code, Message);
        }

        public static class ConcurrencyUpdate
        {
            public static string Code = "1002";
            public static string Message = "Unable to update due to record was recently modified by others.";
            public static DomainError Error() => DomainError.New(Code, Message);
        }
        #endregion

        #region File Error 2001 - 2100
        public static class FailToCreateUser__InvalidFileType
        {
            public static string Code = "2001";
            public static string Message = "Unable to create user. Please upload a valid image file for profile picture.";
            public static DomainError Error() => DomainError.New(Code, Message);
        }

        public static class FailToUpdateUser__InvalidFileType
        {
            public static string Code = "2002";
            public static string Message = "Unable to update user details. Please upload a valid image file for profile picture.";
            public static DomainError Error() => DomainError.New(Code, Message);
        }
        #endregion

        #region User Error 3001 - 3100
        public static class FailToAuthenticate__IncorrectPassword
        {
            public static string Code = "3001";
            public static string Message = "Unable to authenticate user due to incorrect password.";
            public static DomainError Error() => DomainError.New(Code, Message);
        }

        public static class FailToSignUp__UserAlreadyExist
        {
            public static string Code = "3002";
            public static string Message = "Unable to sign up due to user already exists.";
            public static DomainError Error() => DomainError.New(Code, Message);
        }
        #endregion
    }
}
