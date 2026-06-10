using System.Globalization;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public class AccountService(ApplicationDbContext context, UserManager<AppUserEntity> userManager) : IAccountService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<AppUserEntity> _userManager = userManager;

    public async Task<OperationResult<AppUserEntity>> LoginAsync(LoginViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await FindByEmailOrPhoneAsync(model.EmailOrPhone.Trim(), cancellationToken);
        if (user is null)
        {
            return OperationResult<AppUserEntity>.Fail("Email, số điện thoại hoặc mật khẩu không đúng.");
        }

        if (!user.IsActive)
        {
            return OperationResult<AppUserEntity>.Fail("Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên.");
        }

        if (!await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return OperationResult<AppUserEntity>.Fail("Email, số điện thoại hoặc mật khẩu không đúng.");
        }

        user.LastSignInAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return OperationResult<AppUserEntity>.Success(user, "Đăng nhập thành công. Chào mừng trở lại!");
    }

    public async Task<OperationResult<AppUserEntity>> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken = default)
    {
        var email = model.Email.Trim();
        var phone = model.Phone.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return OperationResult<AppUserEntity>.Fail("Email này đã được sử dụng cho một tài khoản khác.");
        }

        if (await PhoneExistsAsync(phone, excludedUserId: null, cancellationToken))
        {
            return OperationResult<AppUserEntity>.Fail("Số điện thoại này đã được sử dụng cho một tài khoản khác.");
        }

        var role = AppRoles.FromSelection(model.Role);
        if (role == AppRoles.Admin)
        {
            return OperationResult<AppUserEntity>.Fail("Không thể tự đăng ký tài khoản quản trị viên.");
        }

        var user = new AppUserEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            UserName = email,
            FullName = model.FullName.Trim(),
            Email = email,
            PhoneNumber = phone,
            PhoneNumberConfirmed = false,
            PlayArea = "Quận 11, TP.HCM",
            JoinedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            return OperationResult<AppUserEntity>.Fail(BuildIdentityErrorMessage(createResult));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return OperationResult<AppUserEntity>.Fail(BuildIdentityErrorMessage(roleResult));
        }

        return OperationResult<AppUserEntity>.Success(
            user,
            "Tạo tài khoản thành công! Chào mừng bạn đến với Đặt Sân Cầu Lông.");
    }

    public Task<AppUserEntity?> FindByIdAsync(string userId, CancellationToken cancellationToken = default) =>
        _userManager.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);

    public async Task<ProfileViewModel?> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var bookings = await _context.Bookings
            .AsNoTracking()
            .Where(item => item.PlayerUserId == userId)
            .ToListAsync(cancellationToken);

        var attendedCount = bookings.Count(item => item.Status != BookingStatus.Cancelled);
        var completedCount = bookings.Count(item => item.Status == BookingStatus.Completed
            || (item.Status == BookingStatus.Confirmed && item.EndAt < DateTime.Now));

        return new ProfileViewModel
        {
            FullName = user.FullName,
            Phone = user.PhoneNumber ?? string.Empty,
            Email = user.Email ?? string.Empty,
            PlayArea = user.PlayArea,
            BookingCount = bookings.Count,
            FavoriteVenuesCount = 0,
            ShowUpRate = attendedCount == 0 ? 100 : (int)Math.Round(completedCount * 100d / attendedCount),
            JoinedDate = user.JoinedAt.ToString("MM/yyyy", CultureInfo.InvariantCulture),
            IsPhoneVerified = user.PhoneNumberConfirmed,
            ReceiveBookingConfirm = user.ReceiveBookingConfirm,
            ReceivePlayReminder = user.ReceivePlayReminder,
            ReceivePromo = user.ReceivePromo
        };
    }

    public async Task<OperationResult<AppUserEntity>> UpdateProfileAsync(string userId, ProfileViewModel model, CancellationToken cancellationToken = default)
    {
        var user = await FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return OperationResult<AppUserEntity>.Fail("Không tìm thấy tài khoản đang đăng nhập.");
        }

        var email = model.Email.Trim();
        var phone = model.Phone.Trim();

        var existingEmailUser = await _userManager.FindByEmailAsync(email);
        if (existingEmailUser is not null && existingEmailUser.Id != userId)
        {
            return OperationResult<AppUserEntity>.Fail("Email này đã được sử dụng cho tài khoản khác.");
        }

        if (await PhoneExistsAsync(phone, userId, cancellationToken))
        {
            return OperationResult<AppUserEntity>.Fail("Số điện thoại này đã được sử dụng cho tài khoản khác.");
        }

        user.FullName = model.FullName.Trim();
        user.UserName = email;
        user.Email = email;
        user.PhoneNumber = phone;
        user.PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(user.PhoneNumber);
        user.PlayArea = model.PlayArea.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return OperationResult<AppUserEntity>.Fail(BuildIdentityErrorMessage(updateResult));
        }

        return OperationResult<AppUserEntity>.Success(user, "Đã cập nhật hồ sơ cá nhân thành công.");
    }

    public async Task<OperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return OperationResult.Fail("Không tìm thấy tài khoản đang đăng nhập.");
        }

        if (newPassword.Length < 8)
        {
            return OperationResult.Fail("Mật khẩu mới phải có ít nhất 8 ký tự.");
        }

        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            return OperationResult.Fail(BuildIdentityErrorMessage(result, "Mật khẩu hiện tại không chính xác."));
        }

        return OperationResult.Success("Đã thay đổi mật khẩu tài khoản thành công.");
    }

    public async Task<OperationResult> UpdateNotificationsAsync(string userId, bool receiveConfirm, bool receiveReminder, bool receivePromo, CancellationToken cancellationToken = default)
    {
        var user = await FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return OperationResult.Fail("Không tìm thấy tài khoản đang đăng nhập.");
        }

        user.ReceiveBookingConfirm = receiveConfirm;
        user.ReceivePlayReminder = receiveReminder;
        user.ReceivePromo = receivePromo;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return OperationResult.Success("Đã cập nhật tùy chọn nhận thông báo.");
    }

    public async Task<OperationResult<AppUserEntity>> GetOrCreateExternalUserAsync(string email, string fullName, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is not null)
        {
            if (!user.IsActive)
            {
                return OperationResult<AppUserEntity>.Fail("Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên.");
            }

            user.LastSignInAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return OperationResult<AppUserEntity>.Success(user, "Đăng nhập thành công.");
        }

        var newUser = new AppUserEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            UserName = email.Trim(),
            FullName = fullName.Trim(),
            Email = email.Trim(),
            EmailConfirmed = true,
            PhoneNumber = string.Empty,
            PlayArea = "Chưa cập nhật",
            JoinedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            return OperationResult<AppUserEntity>.Fail(BuildIdentityErrorMessage(createResult));
        }

        var roleResult = await _userManager.AddToRoleAsync(newUser, AppRoles.Player);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(newUser);
            return OperationResult<AppUserEntity>.Fail(BuildIdentityErrorMessage(roleResult));
        }

        return OperationResult<AppUserEntity>.Success(newUser, "Tạo tài khoản người chơi mới thành công.");
    }

    private async Task<AppUserEntity?> FindByEmailOrPhoneAsync(string lookup, CancellationToken cancellationToken)
    {
        var normalizedEmail = AccountValueNormalizer.NormalizeEmail(lookup);
        var user = await _userManager.Users.FirstOrDefaultAsync(
            item => item.NormalizedEmail == normalizedEmail,
            cancellationToken);
        if (user is not null)
        {
            return user;
        }

        var normalizedPhone = AccountValueNormalizer.NormalizePhone(lookup);
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return null;
        }

        var phoneUsers = await _userManager.Users
            .Where(item => item.PhoneNumber != null && item.PhoneNumber != string.Empty)
            .ToListAsync(cancellationToken);

        return phoneUsers.FirstOrDefault(item =>
            AccountValueNormalizer.NormalizePhone(item.PhoneNumber) == normalizedPhone);
    }

    private async Task<bool> PhoneExistsAsync(string phone, string? excludedUserId, CancellationToken cancellationToken)
    {
        var normalizedPhone = AccountValueNormalizer.NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(normalizedPhone))
        {
            return false;
        }

        var users = await _userManager.Users
            .Where(item => item.Id != excludedUserId && item.PhoneNumber != null && item.PhoneNumber != string.Empty)
            .ToListAsync(cancellationToken);

        return users.Any(item => AccountValueNormalizer.NormalizePhone(item.PhoneNumber) == normalizedPhone);
    }

    private static string BuildIdentityErrorMessage(IdentityResult result, string fallback = "Không thể xử lý yêu cầu tài khoản.")
    {
        var firstError = result.Errors.FirstOrDefault()?.Description;
        return string.IsNullOrWhiteSpace(firstError) ? fallback : firstError;
    }
}
