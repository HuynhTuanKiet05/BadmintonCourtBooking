using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface IVenueCatalogService
{
    Task<List<Venue>> GetFeaturedVenuesAsync(int count = 3, CancellationToken cancellationToken = default);
    Task<List<Venue>> GetApprovedVenuesAsync(CancellationToken cancellationToken = default);
    Task<List<Venue>> SearchApprovedVenuesAsync(VenueSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<List<string>> GetApprovedDistrictsAsync(CancellationToken cancellationToken = default);
    Task<VenueDetailViewModel?> GetVenueDetailAsync(string id, CancellationToken cancellationToken = default);
    Task<Dictionary<string, Dictionary<string, SlotStatus>>?> GetSlotMatrixAsync(string id, string dateKey, CancellationToken cancellationToken = default);
}
