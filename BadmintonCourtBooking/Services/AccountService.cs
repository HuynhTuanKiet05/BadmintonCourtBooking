using System.Globalization;
using BadmintonCourtBooking.Data;
using BadmintonCourtBooking.Data.Entities;
using BadmintonCourtBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BadmintonCourtBooking.Services;

public class AccountService(ApplicationDbContext context, IPasswordHasher<AppUserEntity> passwordHasher) : IAccountService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IPasswordHasher<AppUserEntity> _passwordHasher = passwordHasher;

    public async Task<OperationResult<AppUserEntity>> LoginAsync(LoginViewModel model, CancellationToken cancellationToken = default)
    {
        var lookup = model.EmailOrPhone.Trim();
        var normalizedEmail = AccountValueNormalizer.NormalizeEmail(lookup);
        var normalizedPhone = AccountValueNormalizer.NormalizePhone(lookup);

        var user = await _context.Users.FirstOrDefaultAsync(
            item => item.NormalizedEmail == normalizedEmail || item.NormalizedPhoneNumber == normalizedPhone,
            cancellationToken);

        if (user is null)
        {
            return OperationResult<AppUserEntity>.Fail("Email, số điện thoại hoặc mật khẩu không đúng.");
        }

        if (!user.IsActive)
        {
            return OperationResult<AppUserEntity>.Fail("Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return OperationResult<AppUserEntity>.Fail("Email, số điện thoại hoặc mật khẩu không đúng.");
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
        }

        user.LastSignInAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult<AppUserEntity>.Success(user, "Đăng nhập thành công. Chào mừng trở lại!");
    }

    public async Task<OperationResult<AppUserEntity>> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = AccountValueNormalizer.NormalizeEmail(model.Email);
        var normalizedPhone = AccountValueNormalizer.NormalizePhone(model.Phone);

        if (await _context.Users.AnyAsync(item => item.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            return OperationResult<AppUserEntity>.Fail("Email này đã được sử dụng cho một tài khoản khác.");
        }

        if (await _context.Users.AnyAsync(item => item.NormalizedPhoneNumber == normalizedPhone, cancellationToken))
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
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            PhoneNumber = model.Phone.Trim(),
            NormalizedPhoneNumber = normalizedPhone,
            Role = role,
            PlayArea = "Quận 11, TP.HCM",
            JoinedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPhoneVerified = false
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult<AppUserEntity>.Success(
            user,
            "Tạo tài khoản thành công! Chào mừng bạn đến với Đặt Sân Cầu Lông.");
    }

    public Task<AppUserEntity?> FindByIdAsync(string userId, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);

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
            Phone = user.PhoneNumber,
            Email = user.Email,
            PlayArea = user.PlayArea,
            BookingCount = bookings.Count,
            FavoriteVenuesCount = 0,
            ShowUpRate = attendedCount == 0 ? 100 : (int)Math.Round(completedCount * 100d / attendedCount),
            JoinedDate = user.JoinedAt.ToString("MM/yyyy", CultureInfo.InvariantCulture),
            IsPhoneVerified = user.IsPhoneVerified,
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

        var normalizedEmail = AccountValueNormalizer.NormalizeEmail(model.Email);
        var normalizedPhone = AccountValueNormalizer.NormalizePhone(model.Phone);

        var emailExists = await _context.Users.AnyAsync(
            item => item.Id != userId && item.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (emailExists)
        {
            return OperationResult<AppUserEntity>.Fail("Email này đã được sử dụng cho tài khoản khác.");
        }

        var phoneExists = await _context.Users.AnyAsync(
            item => item.Id != userId && item.NormalizedPhoneNumber == normalizedPhone,
            cancellationToken);

        if (phoneExists)
        {
            return OperationResult<AppUserEntity>.Fail("Số điện thoại này đã được sử dụng cho tài khoản khác.");
        }

        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.NormalizedEmail = normalizedEmail;
        user.PhoneNumber = model.Phone.Trim();
        user.NormalizedPhoneNumber = normalizedPhone;
        user.PlayArea = model.PlayArea.Trim();
        user.IsPhoneVerified = !string.IsNullOrWhiteSpace(user.PhoneNumber);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult<AppUserEntity>.Success(user, "Đã cập nhật hồ sơ cá nhân thành công.");
    }

    public async Task<OperationResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return OperationResult.Fail("Không tìm thấy tài khoản đang đăng nhập.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            return OperationResult.Fail("Mật khẩu hiện tại không chính xác.");
        }

        if (newPassword.Length < 8)
        {
            return OperationResult.Fail("Mật khẩu mới phải có ít nhất 8 ký tự.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

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
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult.Success("Đã cập nhật tùy chọn nhận thông báo.");
    }

    public async Task<OperationResult<AppUserEntity>> GetOrCreateExternalUserAsync(string email, string fullName, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = AccountValueNormalizer.NormalizeEmail(email);

        var user = await _context.Users.FirstOrDefaultAsync(
            item => item.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (user is not null)
        {
            if (!user.IsActive)
            {
                return OperationResult<AppUserEntity>.Fail("Tài khoản của bạn đang bị khóa. Vui lòng liên hệ quản trị viên.");
            }

            user.LastSignInAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return OperationResult<AppUserEntity>.Success(user, "Đăng nhập thành công.");
        }

        // Create new user as Player
        var newUser = new AppUserEntity
        {
            Id = Guid.NewGuid().ToString("N"),
            FullName = fullName.Trim(),
            Email = email.Trim(),
            NormalizedEmail = normalizedEmail,
            PhoneNumber = string.Empty,
            NormalizedPhoneNumber = string.Empty,
            Role = AppRoles.Player,
            PlayArea = "Chưa cập nhật",
            JoinedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPhoneVerified = false,
            IsActive = true
        };

        // Set a random complex password hash
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, Guid.NewGuid().ToString("N"));

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        return OperationResult<AppUserEntity>.Success(newUser, "Tạo tài khoản người chơi mới thành công.");
    }
}
