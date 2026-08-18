using Claims.Auditing;
using Claims.Controllers;
using Claims.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Claims.Tests;

public class CoversServiceTests : IDisposable
{
    private readonly ClaimsContext _dbContext;
    private readonly CoversService _sut;

    public CoversServiceTests()
    {
        var options = new DbContextOptionsBuilder<ClaimsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ClaimsContext(options);
        _sut = new CoversService(_dbContext, new AuditChannel());
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public void ComputePremium_StartDateInPast_ThrowsArgumentException()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-2);
        var endDate = DateTime.UtcNow.AddDays(10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _sut.ComputePremium(pastDate, endDate, CoverType.Yacht));
    }

    [Fact]
    public void ComputePremium_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        var startDate = DateTime.UtcNow.AddDays(10);
        var endDate = DateTime.UtcNow.AddDays(5);

        Assert.Throws<ArgumentException>(() => 
            _sut.ComputePremium(startDate, endDate, CoverType.Yacht));
    }

    [Fact]
    public void ComputePremium_PeriodExceedsOneYear_ThrowsArgumentException()
    {
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddYears(1).AddDays(1);

        Assert.Throws<ArgumentException>(() => 
            _sut.ComputePremium(startDate, endDate, CoverType.Yacht));
    }

    [Theory]
    [InlineData(CoverType.Yacht, 10)]
    [InlineData(CoverType.PassengerShip, 15)]
    [InlineData(CoverType.Tanker, 5)]
    public void ComputePremium_ValidDates_ReturnsExpectedPositiveAmount(CoverType coverType, int days)
    {
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(days);

        var premium = _sut.ComputePremium(startDate, endDate, coverType);

        Assert.True(premium > 0m);
    }

    //Tests added for Task 5

    [Fact]
    public void ComputePremium_YachtFor365Days_CalculatesCorrectProgressivePremium()
    {
        // Arrange
        // Base day rate = 1250 * 1.1 (Yacht) = 1375
        // First 30 days: 30 * 1375 = 41,250
        // Next 150 days (5% off): 150 * (1375 * 0.95) = 195,937.5
        // Remaining 185 days (8% off): 185 * (1375 * 0.92) = 234,025
        // Total = 471,212.5

        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(365);

        // Act
        var premium = _sut.ComputePremium(startDate, endDate, CoverType.Yacht);

        // Assert
        Assert.Equal(471212.5m, premium);
    }

    [Fact]
    public void ComputePremium_PassengerShipFor365Days_CalculatesCorrectProgressivePremium()
    {
        // Arrange
        // Base day rate = 1250 * 1.2 (PassengerShip) = 1500
        // First 30 days: 30 * 1500 = 45,000
        // Next 150 days (2% off): 150 * (1500 * 0.98) = 220,500
        // Remaining 185 days (3% off): 185 * (1500 * 0.97) = 269,175
        // Total = 534,675

        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(365);

        // Act
        var premium = _sut.ComputePremium(startDate, endDate, CoverType.PassengerShip);

        // Assert
        Assert.Equal(534675m, premium);
    }

    [Fact]
    public void ComputePremium_TankerFor365Days_CalculatesCorrectProgressivePremium()
    {
        // Arrange
        // Base day rate = 1250 * 1.5 (Tanker) = 1875
        // First 30 days: 30 * 1875 = 56,250
        // Next 150 days (2% off): 150 * (1875 * 0.98) = 275,625
        // Remaining 185 days (3% off): 185 * (1875 * 0.97) = 335,812.5
        // Total = 668,343.75

        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(365);

        // Act
        var premium = _sut.ComputePremium(startDate, endDate, CoverType.Tanker);

        // Assert
        Assert.Equal(668343.75m, premium);
    }

    [Fact]
    public void ComputePremium_ContainerShipFor365Days_UsesOtherTypeProgressiveDiscounts()
    {
        // Arrange
        // Base day rate = 1250 * 1.3 (other type) = 1625
        // First 30 days: 30 * 1625 = 48,750
        // Next 150 days (2% off): 150 * (1625 * 0.98) = 238,125
        // Remaining 185 days (3% off): 185 * (1625 * 0.97) = 291,421.25
        // Total = 579,231.25

        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = startDate.AddDays(365);

        // Act
        var premium = _sut.ComputePremium(startDate, endDate, CoverType.ContainerShip);

        // Assert
        Assert.Equal(579231.25m, premium);
    }
}