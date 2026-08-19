namespace KiranTest1.Api.Services;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
