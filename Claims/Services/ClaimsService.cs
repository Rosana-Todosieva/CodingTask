using Claims.Auditing;
using Claims.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Claims.Services;

public class ClaimsService : IClaimsService
{
    private readonly ClaimsContext _claimsContext;
   // private readonly Auditer _auditer;
   private readonly AuditChannel _auditChannel;

    public ClaimsService(ClaimsContext claimsContext, AuditChannel auditChannel)
    {
        _claimsContext = claimsContext;
        _auditChannel = auditChannel;
    }

    public async Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        return await _claimsContext.GetClaimsAsync();
    }

    public async Task<Claim?> GetClaimAsync(string id)
    {
        return await _claimsContext.GetClaimAsync(id);
    }

    public async Task<Claim> CreateClaimAsync(Claim claim)
    {
       // ClaimDamageCost must not exceed 100,000
        if (claim.DamageCost > 100_000m)
        {
            throw new ArgumentException("Claim damage cost cannot exceed 100,000.");
        }

        var cover = await _claimsContext.Covers.FirstOrDefaultAsync(c => c.Id == claim.CoverId);
        if (cover is null)
        {
            throw new InvalidOperationException($"Related cover with ID '{claim.CoverId}' does not exist.");
        }

        // Created date must be within the Cover period
        if (claim.Created < cover.StartDate || claim.Created > cover.EndDate)
        {
            throw new ArgumentException("Created date must be within the period of the related Cover.");
        }

        claim.Id = Guid.NewGuid().ToString();
        await _claimsContext.AddItemAsync(claim);
       // _auditer.AuditClaim(claim.Id, "POST");

       await _auditChannel.AddAuditAsync(new AuditMessage("Claim", claim.Id, "POST"));
        
        return claim;
    }

    public async Task DeleteClaimAsync(string id)
    {
        await _auditChannel.AddAuditAsync(new AuditMessage("Claim", id, "DELETE"));
        await _claimsContext.DeleteItemAsync(id);
    }
}