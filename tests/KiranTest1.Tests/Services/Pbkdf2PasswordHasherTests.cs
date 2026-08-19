using KiranTest1.Api.Services;

namespace KiranTest1.Tests.Services;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _sut = new();

    [Fact]
    public void Hash_ProducesSaltedFormatWithDistinctValues()
    {
        var first = _sut.Hash("sup3rSecret!");
        var second = _sut.Hash("sup3rSecret!");

        Assert.Equal(3, first.Split('.').Length);
        Assert.NotEqual(first, second);
        Assert.DoesNotContain("sup3rSecret!", first);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Hash_BlankPassword_Throws(string? password)
    {
        Assert.ThrowsAny<ArgumentException>(() => _sut.Hash(password!));
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        Assert.True(_sut.Verify("sup3rSecret!", _sut.Hash("sup3rSecret!")));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        Assert.False(_sut.Verify("wrongPassword", _sut.Hash("sup3rSecret!")));
    }

    [Theory]
    [InlineData("", "hash")]
    [InlineData("password", "")]
    [InlineData("password", "not-a-hash")]
    [InlineData("password", "abc.def.ghi")]
    [InlineData("password", "0.c2FsdA==.a2V5")]
    [InlineData("password", "1000.!!!.a2V5")]
    public void Verify_MalformedInput_ReturnsFalse(string password, string hash)
    {
        Assert.False(_sut.Verify(password, hash));
    }
}
