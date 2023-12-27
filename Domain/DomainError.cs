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

        public static class FailToCreateAccount__InvalidFileType
        {
            public static string Code = "2001";
            public static string Message = "Unable to create account. Please upload a valid image file for profile picture.";
            public static DomainError Error() => DomainError.New(Code, Message);
        }

        public static class FailToUpdateAccount__InvalidFileType
        {
            public static string Code = "2002";
            public static string Message = "Unable to update account details. Please upload a valid image file for profile picture.";
            public static DomainError Error() => DomainError.New(Code, Message);
        }
    }
}
