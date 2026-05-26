using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface IOwnerDashboardService
{
    Task<OwnerDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<OwnerVenueManagementViewModel> GetVenueManagementAsync(string? selectedVenueId = null, CancellationToken cancellationToken = default);
    Task<OperationResult<string>> CreateVenueAsync(OwnerVenueInputModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<string>> UpdateVenueAsync(OwnerVenueInputModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<string>> CreateCourtAsync(OwnerCourtInputModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<string>> UpdateCourtAsync(OwnerCourtInputModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ApproveBookingAsync(string id, CancellationToken cancellationToken = default);
    Task<OperationResult> RejectBookingAsync(string id, CancellationToken cancellationToken = default);
}
