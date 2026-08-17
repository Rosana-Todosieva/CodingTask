namespace Claims.Services;

public interface IClaimsService
{

    /// <summary>
    /// Retrieves all recorded claims from the database.
    /// </summary>
    Task<IEnumerable<Claim>> GetClaimsAsync();

    /// <summary>
    /// Retrieves a specific claim by its unique identifier.
    /// </summary>
    Task<Claim?> GetClaimAsync(string id);

    /// <summary>
    /// Creates a new claim and audits the operation.
    /// </summary>
    Task<Claim> CreateClaimAsync(Claim claim);

    /// <summary>
    /// Deletes a claim by its unique identifier and audits the operation.
    /// </summary>
    Task DeleteClaimAsync(string id);
}