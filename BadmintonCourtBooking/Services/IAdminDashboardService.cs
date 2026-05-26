using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> ApproveVenueAsync(string id, CancellationToken cancellationToken = default);
    Task<OperationResult> RejectVenueAsync(string id, CancellationToken cancellationToken = default);
    Task<OperationResult> LockUserAsync(string id, CancellationToken cancellationToken = default);
    Task<OperationResult> UnlockUserAsync(string id, CancellationToken cancellationToken = default);
}
