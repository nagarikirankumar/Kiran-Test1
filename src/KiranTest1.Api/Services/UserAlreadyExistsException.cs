namespace KiranTest1.Api.Services;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string message)
        : base(message)
    {
    }
}
