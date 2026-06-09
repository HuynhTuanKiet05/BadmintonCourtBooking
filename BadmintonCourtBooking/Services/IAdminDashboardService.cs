using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<AdminVenueManagementViewModel> GetVenueManagementAsync(string? selectedVenueId = null, CancellationToken cancellationToken = default);
    Task<OperationResult> ApproveVenueAsync(string id, CancellationToken cancellationToken = default);
    Task<OperationResult> RejectVenueAsync(string id, CancellationToken cancellationToken = default);
    Task<OperationResult> ApproveBookingAsync(string id, CancellationToken cancellationToken = default);
    Task<OperationResult> RejectBookingAsync(string id, CancellationToken cancellationToken = default);
    Task<OperationResult<string>> CreateVenueAsync(AdminVenueInputModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<string>> UpdateVenueAsync(AdminVenueInputModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<string>> CreateCourtAsync(AdminCourtInputModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<string>> UpdateCourtAsync(AdminCourtInputModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> LockUserAsync(string id, CancellationToken cancellationToken = default);
    Task<OperationResult> UnlockUserAsync(string id, CancellationToken cancellationToken = default);
}
