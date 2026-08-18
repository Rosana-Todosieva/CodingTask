using Claims.Auditing;
using Claims.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Claims.Services;

public class CoversService : ICoversService
{
    private readonly ClaimsContext _claimsContext;
   // private readonly Auditer _auditer;
   private readonly AuditChannel _auditChannel;

    public CoversService(ClaimsContext claimsContext, AuditChannel auditChannel)
    {
        _claimsContext = claimsContext;
        _auditChannel = auditChannel;
    }

    public async Task<IEnumerable<Cover>> GetCoversAsync()
    {
        return await _claimsContext.Covers.ToListAsync();
    }

    public async Task<Cover?> GetCoverAsync(string id)
    {
        return await _claimsContext.Covers.FirstOrDefaultAsync(cover => cover.Id == id);
    }

    public async Task<Cover> CreateCoverAsync(Cover cover)
    {
        ValidateCoverDates(cover.StartDate, cover.EndDate);

        cover.Id = Guid.NewGuid().ToString();
        cover.Premium = ComputePremium(cover.StartDate, cover.EndDate, cover.Type);

        _claimsContext.Covers.Add(cover);
        await _claimsContext.SaveChangesAsync();
       // _auditer.AuditCover(cover.Id, "POST");
       await _auditChannel.AddAuditAsync(new AuditMessage("Cover", cover.Id, "POST"));

        return cover;
    }

    public async Task DeleteCoverAsync(string id)
    {
       // _auditer.AuditCover(id, "DELETE");
       await _auditChannel.AddAuditAsync(new AuditMessage("Cover", id, "DELETE"));
       
        var cover = await _claimsContext.Covers.FirstOrDefaultAsync(cover => cover.Id == id);

        if (cover is not null)
        {
            _claimsContext.Covers.Remove(cover);
            await _claimsContext.SaveChangesAsync();
        }
    }

    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        ValidateCoverDates(startDate, endDate);

        var multiplier = coverType switch
        {
            CoverType.Yacht => 1.1m,
            CoverType.PassengerShip => 1.2m,
            CoverType.Tanker => 1.5m,
            _ => 1.3m
        };

        //Changed from logic in Task 5
        var basePremiumPerDay = 1250m * multiplier;
        var totalDays = (endDate - startDate).Days;

        var tier1Days = Math.Min(30, totalDays);
        var tier2Days = Math.Min(150, Math.Max(0, totalDays - 30));
        var tier3Days = Math.Max(0, totalDays - 180);

        var tier2Discount = coverType == CoverType.Yacht ? 0.05m : 0.02m;
        var tier3Discount = coverType == CoverType.Yacht ? 0.08m : 0.03m;

        // Calculation of the total premium based on the number of days in each tier and the respective discounts
        var totalPremium = (tier1Days * basePremiumPerDay) +
                       (tier2Days * basePremiumPerDay * (1m - tier2Discount)) +
                       (tier3Days * basePremiumPerDay * (1m - tier3Discount));

        return totalPremium;
    }

    private static void ValidateCoverDates(DateTime startDate, DateTime endDate)
    {
        if (startDate.Date < DateTime.UtcNow.Date)
        {
            throw new ArgumentException("Cover start date cannot be in the past.");
        }

        if (startDate.AddYears(1) < endDate)
        {
            throw new ArgumentException("Total insurance period cannot exceed 1 year.");
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.");
        }
    }
}