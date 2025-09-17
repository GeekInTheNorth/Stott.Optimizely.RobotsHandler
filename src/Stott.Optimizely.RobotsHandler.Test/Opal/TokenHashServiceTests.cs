using System;
using NUnit.Framework;
using Stott.Optimizely.RobotsHandler.Opal;

namespace Stott.Optimizely.RobotsHandler.Test.Opal;

[TestFixture]
public sealed class TokenHashServiceTests
{
    private TokenHashService _tokenHashService;

    [SetUp]
    public void Setup()
    {
        _tokenHashService = new TokenHashService();
    }

    [Test]
    public void HashToken_WithTheSameSalt_ReturnsConsistentHash()
    {
        // Arrange
        var token = "test-token-123";
        var tokenSalt = Guid.NewGuid().ToString();

        // Act
        var hash1 = _tokenHashService.HashToken(token, tokenSalt);
        var hash2 = _tokenHashService.HashToken(token, tokenSalt);

        // Assert
        Assert.That(hash1, Is.EqualTo(hash2), "Same token and date should produce identical hashes");
        Assert.That(hash1, Is.Not.Null.And.Not.Empty, "Hash should not be null or empty");
    }

    [Test]
    public void HashToken_WithSalts_ReturnsDifferentHashes()
    {
        // Arrange
        var token = "test-token-123";
        var saltOne = Guid.NewGuid().ToString();
        var saltTwo = Guid.NewGuid().ToString();

        // Act
        var hash1 = _tokenHashService.HashToken(token, saltOne);
        var hash2 = _tokenHashService.HashToken(token, saltTwo);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2), "Different dates should produce different hashes");
    }

    [Test]
    public void VerifyToken_WithCorrectTokenAndDate_ReturnsTrue()
    {
        // Arrange
        var token = "test-token-123";
        var tokenSalt = Guid.NewGuid().ToString();
        var hash = _tokenHashService.HashToken(token, tokenSalt);

        // Act
        var isValid = _tokenHashService.VerifyToken(token, hash, tokenSalt);

        // Assert
        Assert.That(isValid, Is.True, "Correct token and date should verify successfully");
    }

    [Test]
    public void VerifyToken_WithIncorrectToken_ReturnsFalse()
    {
        // Arrange
        var originalToken = "test-token-123";
        var wrongToken = "wrong-token-456";
        var tokenSalt = Guid.NewGuid().ToString();
        var hash = _tokenHashService.HashToken(originalToken, tokenSalt);

        // Act
        var isValid = _tokenHashService.VerifyToken(wrongToken, hash, tokenSalt);

        // Assert
        Assert.That(isValid, Is.False, "Wrong token should fail verification");
    }

    [Test]
    public void VerifyToken_WithIncorrectDate_ReturnsFalse()
    {
        // Arrange
        var token = "test-token-123";
        var correctSalt = Guid.NewGuid().ToString();
        var incorrectSalt = Guid.NewGuid().ToString();
        var hash = _tokenHashService.HashToken(token, correctSalt);

        // Act
        var isValid = _tokenHashService.VerifyToken(token, hash, incorrectSalt);

        // Assert
        Assert.That(isValid, Is.False, "Wrong date should fail verification");
    }

    [Test]
    public void HashToken_WithNullOrEmptyToken_ThrowsArgumentException()
    {
        // Arrange
        var tokenSalt = Guid.NewGuid().ToString();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenHashService.HashToken(null, tokenSalt));
        Assert.Throws<ArgumentException>(() => _tokenHashService.HashToken(string.Empty, tokenSalt));
    }

    [Test]
    public void VerifyToken_WithNullOrEmptyInputs_ReturnsFalse()
    {
        // Arrange
        var tokenSalt = Guid.NewGuid().ToString();

        // Act & Assert
        Assert.That(_tokenHashService.VerifyToken(null, "hash", tokenSalt), Is.False);
        Assert.That(_tokenHashService.VerifyToken("token", null, tokenSalt), Is.False);
        Assert.That(_tokenHashService.VerifyToken("", "hash", tokenSalt), Is.False);
        Assert.That(_tokenHashService.VerifyToken("token", "", tokenSalt), Is.False);
    }
}