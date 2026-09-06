using UKAccounts.Security;
using Xunit;

namespace UKAccounts.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ShouldProduceDifferentHashesForSamePassword()
    {
        var hash1 = PasswordHasher.Hash("password123");
        var hash2 = PasswordHasher.Hash("password123");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_ShouldReturnTrueForCorrectPassword()
    {
        var hash = PasswordHasher.Hash("password123");
        Assert.True(PasswordHasher.Verify("password123", hash));
    }

    [Fact]
    public void Verify_ShouldReturnFalseForIncorrectPassword()
    {
        var hash = PasswordHasher.Hash("password123");
        Assert.False(PasswordHasher.Verify("wrongpassword", hash));
    }

    [Fact]
    public void Verify_ShouldReturnFalseForInvalidHash()
    {
        Assert.False(PasswordHasher.Verify("password123", "invalid-hash"));
    }
}
