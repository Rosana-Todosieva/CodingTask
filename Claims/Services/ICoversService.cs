namespace Claims.Services;

public interface ICoversService
{
    /// <summary>
    /// Retrieves all insurance covers.
    /// </summary>
    Task<IEnumerable<Cover>> GetCoversAsync();

    /// <summary>
    /// Retrieves a specific cover by its unique identifier.
    /// </summary>
    Task<Cover?> GetCoverAsync(string id);

    /// <summary>
    /// Creates a new cover, computes its premium, and audits the operation.
    /// </summary>
    Task<Cover> CreateCoverAsync(Cover cover);

    /// <summary>
    /// Deletes a cover by identifier and audits the operation.
    /// </summary>
    Task DeleteCoverAsync(string id);

    /// <summary>
    /// Computes the total premium for a given insurance period and cover type.
    /// </summary>
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}