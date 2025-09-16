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
    public void HashToken_WithCreatedDate_ReturnsConsistentHash()
    {
        // Arrange
        var token = "test-token-123";
        var createdDate = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);

        // Act
        var hash1 = _tokenHashService.HashToken(token, createdDate);
        var hash2 = _tokenHashService.HashToken(token, createdDate);

        // Assert
        Assert.That(hash1, Is.EqualTo(hash2), "Same token and date should produce identical hashes");
        Assert.That(hash1, Is.Not.Null.And.Not.Empty, "Hash should not be null or empty");
    }

    [Test]
    public void HashToken_WithDifferentDates_ReturnsDifferentHashes()
    {
        // Arrange
        var token = "test-token-123";
        var date1 = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var date2 = new DateTime(2024, 1, 15, 10, 30, 46, DateTimeKind.Utc); // 1 second difference

        // Act
        var hash1 = _tokenHashService.HashToken(token, date1);
        var hash2 = _tokenHashService.HashToken(token, date2);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2), "Different dates should produce different hashes");
    }

    [Test]
    public void VerifyToken_WithCorrectTokenAndDate_ReturnsTrue()
    {
        // Arrange
        var token = "test-token-123";
        var createdDate = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var hash = _tokenHashService.HashToken(token, createdDate);

        // Act
        var isValid = _tokenHashService.VerifyToken(token, hash, createdDate);

        // Assert
        Assert.That(isValid, Is.True, "Correct token and date should verify successfully");
    }

    [Test]
    public void VerifyToken_WithIncorrectToken_ReturnsFalse()
    {
        // Arrange
        var originalToken = "test-token-123";
        var wrongToken = "wrong-token-456";
        var createdDate = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var hash = _tokenHashService.HashToken(originalToken, createdDate);

        // Act
        var isValid = _tokenHashService.VerifyToken(wrongToken, hash, createdDate);

        // Assert
        Assert.That(isValid, Is.False, "Wrong token should fail verification");
    }

    [Test]
    public void VerifyToken_WithIncorrectDate_ReturnsFalse()
    {
        // Arrange
        var token = "test-token-123";
        var correctDate = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var wrongDate = new DateTime(2024, 1, 15, 10, 30, 46, DateTimeKind.Utc);
        var hash = _tokenHashService.HashToken(token, correctDate);

        // Act
        var isValid = _tokenHashService.VerifyToken(token, hash, wrongDate);

        // Assert
        Assert.That(isValid, Is.False, "Wrong date should fail verification");
    }

    [Test]
    public void HashToken_WithNullOrEmptyToken_ThrowsArgumentException()
    {
        // Arrange
        var createdDate = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _tokenHashService.HashToken(null, createdDate));
        Assert.Throws<ArgumentException>(() => _tokenHashService.HashToken("", createdDate));
    }

    [Test]
    public void VerifyToken_WithNullOrEmptyInputs_ReturnsFalse()
    {
        // Arrange
        var createdDate = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);

        // Act & Assert
        Assert.That(_tokenHashService.VerifyToken(null, "hash", createdDate), Is.False);
        Assert.That(_tokenHashService.VerifyToken("token", null, createdDate), Is.False);
        Assert.That(_tokenHashService.VerifyToken("", "hash", createdDate), Is.False);
        Assert.That(_tokenHashService.VerifyToken("token", "", createdDate), Is.False);
    }

    [Test]
    public void HashToken_WithTimezoneVariations_ProducesConsistentResults()
    {
        // Arrange
        var token = "test-token-123";
        var utcDate = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var localDate = utcDate.ToLocalTime();

        // Act
        var utcHash = _tokenHashService.HashToken(token, utcDate);
        var localHash = _tokenHashService.HashToken(token, localDate);

        // Assert
        Assert.That(utcHash, Is.EqualTo(localHash), "UTC and local times representing the same moment should produce identical hashes");
    }
}