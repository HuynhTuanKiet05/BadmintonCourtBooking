using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface IBookingService
{
    Task<List<Booking>> GetPlayerBookingsAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CreateBookingAsync(CreateBookingInputModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> CancelBookingAsync(string id, CancellationToken cancellationToken = default);
}
