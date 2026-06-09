using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;

namespace BadmintonCourtBooking.Services;

public interface IAccountService
{
    Task<OperationResult<AppUserEntity>> LoginAsync(LoginViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<AppUserEntity>> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken = default);
    Task<AppUserEntity?> FindByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<ProfileViewModel?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<OperationResult<AppUserEntity>> UpdateProfileAsync(string userId, ProfileViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateNotificationsAsync(string userId, bool receiveConfirm, bool receiveReminder, bool receivePromo, CancellationToken cancellationToken = default);
    Task<OperationResult<AppUserEntity>> GetOrCreateExternalUserAsync(string email, string fullName, CancellationToken cancellationToken = default);
}
