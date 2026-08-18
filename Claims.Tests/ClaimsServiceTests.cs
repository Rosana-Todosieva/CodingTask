using Claims.Auditing;
using Claims.Controllers;
using Claims.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Claims.Tests;

public class ClaimsServiceTests : IDisposable
{
    private readonly ClaimsContext _dbContext;
    private readonly ClaimsService _sut;

    public ClaimsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClaimsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ClaimsContext(options);
        _sut = new ClaimsService(_dbContext, new AuditChannel());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CreateClaimAsync_DamageCostExceedsLimit_ThrowsArgumentException()
    {
        // Arrange
        var claim = new Claim
        {
            CoverId = "cover-1",
            DamageCost = 100_001m,
            Created = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateClaimAsync(claim));
    }

    [Fact]
    public async Task CreateClaimAsync_RelatedCoverDoesNotExist_ThrowsInvalidOperationException()
    {
        var claim = new Claim
        {
            CoverId = "non-existent-cover",
            DamageCost = 50_000m,
            Created = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateClaimAsync(claim));
    }

    [Fact]
    public async Task CreateClaimAsync_CreatedDateOutsideCoverPeriod_ThrowsArgumentException()
    {
        // Arrange
        var cover = new Cover
        {
            Id = "cover-1",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(30),
            Type = CoverType.Yacht
        };
        _dbContext.Covers.Add(cover);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var claim = new Claim
        {
            CoverId = cover.Id,
            DamageCost = 50_000m,
            Created = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateClaimAsync(claim));
    }
}