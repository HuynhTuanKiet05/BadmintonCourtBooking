# Kế hoạch phân chia nhiệm vụ dự án BadmintonCourtBooking

Tài liệu này dùng cho nhánh `scale-down-2-roles`, sau khi dự án đã được rút gọn còn 2 vai trò chính: `Admin` và `Player`.

Mục tiêu của file này là chia việc rõ ràng cho 2 thành viên, tránh chồng chéo file, đảm bảo lịch sử commit hợp lý và giúp người còn lại pull code về test tiếp dễ dàng.

## 1. Tổng quan kỹ thuật

| Hạng mục | Công nghệ / phạm vi |
|---|---|
| Framework | ASP.NET Core MVC, .NET 9 |
| Database | SQL Server LocalDB |
| ORM | Entity Framework Core 9 |
| Authentication | ASP.NET Core Identity, Google/Facebook external login |
| Role hiện tại | `Admin`, `Player` |
| UI | Razor Views, Bootstrap, CSS custom, JavaScript thuần |
| Branch làm việc | `scale-down-2-roles` |

Các vai trò cũ như `Owner` không còn nằm trong phạm vi hiện tại. Mọi chức năng quản trị sân, duyệt sân, duyệt booking và khóa/mở khóa người dùng thuộc về `Admin`. Người chơi dùng `Player`.

## 2. Nguyên tắc làm việc chung

1. Mỗi người chỉ sửa chính các file thuộc module của mình.
2. File dùng chung như `Program.cs`, `ApplicationDbContext.cs`, migrations, layout chung phải ghi rõ lý do trong commit message.
3. Trước khi commit luôn chạy tối thiểu:

```bash
dotnet build
dotnet ef migrations has-pending-model-changes
```

4. Trước khi push luôn pull/fetch nhánh mới nhất:

```bash
git fetch origin scale-down-2-roles
git status
```

5. Không commit các thư mục local/cache:

```text
.build-cache/
.tools/
.vs/
bin/
obj/
```

## 3. Trạng thái commit hiện tại

Commit 1 của Kiệt và Trí đã được tách để đưa code nền lên trước. Từ các commit tiếp theo, mỗi người tiếp tục theo phạm vi bên dưới để lịch sử commit rõ người, rõ module.

Commit mới nhất đang có trên `scale-down-2-roles`:

```text
80d5ce2 Chuyển xác thực sang ASP.NET Core Identity
```

Commit này là commit hạ tầng dùng chung, đã đưa auth về ASP.NET Core Identity và cần cả 2 người dựa trên nền này để làm tiếp.

## 4. Phân chia theo thành viên

### 4.1. Huỳnh Tuấn Kiệt - Player, Auth, Booking, Home

Kiệt phụ trách toàn bộ luồng người chơi: đăng nhập, đăng ký, hồ sơ, trang chủ, lịch đặt sân của tôi và hành vi tạo/hủy booking từ phía player.

#### File sở hữu chính

| Nhóm | File / thư mục |
|---|---|
| Controller | `Controllers/AccountController.cs`, `Controllers/BookingController.cs`, `Controllers/HomeController.cs` |
| Identity wrapper | `Areas/Identity/Pages/Account/**` |
| Service | `Services/AccountService.cs`, `Services/IAccountService.cs`, `Services/BookingService.cs`, `Services/IBookingService.cs` |
| Current user | `Services/CurrentUserService.cs`, `Services/ICurrentUserService.cs`, `Services/RequireActiveUserFilter.cs`, `Services/AppUserClaimsPrincipalFactory.cs` |
| Model | `Models/AccountViewModel.cs`, `Models/AppRoles.cs`, `Models/Booking.cs`, `Models/CreateBookingInputModel.cs`, `Models/ErrorViewModel.cs` |
| View | `Views/Account/**`, `Views/Booking/**`, `Views/Home/**` |
| Shared UI liên quan user | `Views/Shared/_Layout.cshtml`, `Views/Shared/_Toast.cshtml`, `Views/Shared/_EmptyBookingsState.cshtml`, `Views/Shared/_StatusBadge.cshtml` |

#### Lộ trình commit của Kiệt

| Commit | Message đề xuất | Nội dung |
|---|---|---|
| K1 | `feat: khoi tao project mvc va cau hinh nen` | Solution, project, cấu hình app, cấu trúc MVC ban đầu. |
| K2 | `feat: them xac thuc nguoi dung bang identity` | AccountController, AccountService, Identity pages, claim factory, login/register/logout. |
| K3 | `feat: them giao dien dang nhap dang ky va ho so` | UI login/register/profile/access denied, validation, toast auth. |
| K4 | `feat: them trang chu va dieu huong theo vai tro` | Home page, navbar theo role, link Login/Register/Profile/Logout. |
| K5 | `feat: them luong dat san cho nguoi choi` | BookingService tạo/hủy booking, BookingController, input model booking. |
| K6 | `feat: them trang lich dat san cua toi` | View lịch booking của player, trạng thái upcoming/completed/cancelled. |
| K7 | `fix: dong bo active user va quyen truy cap player` | RequireActiveUserFilter, CurrentUserService, xử lý user bị khóa. |
| K8 | `test: kiem tra luong player auth va booking` | Smoke test login player, đặt sân, hủy sân, profile. |

#### Checklist nghiệm thu của Kiệt

- Player đăng ký tài khoản mới được.
- Player đăng nhập bằng email hoặc số điện thoại được.
- Player đăng xuất được.
- Player vào được `Lịch của tôi`.
- Player đặt được slot còn trống.
- Player không đặt được slot trùng hoặc slot đã qua.
- Player hủy được booking hợp lệ.
- User bị khóa bị sign out và không thao tác tiếp được.

### 4.2. Đức Trí - Venue, Admin, Data, Frontend hỗ trợ

Trí phụ trách dữ liệu sân bãi, danh sách/chi tiết sân, dashboard admin, quản lý cụm sân/sân con, duyệt booking, khóa/mở khóa user và phần CSS/JS hỗ trợ các màn hình venue/admin.

#### File sở hữu chính

| Nhóm | File / thư mục |
|---|---|
| Controller | `Controllers/VenueController.cs`, `Controllers/AdminController.cs` |
| Service | `Services/VenueCatalogService.cs`, `Services/IVenueCatalogService.cs`, `Services/AdminDashboardService.cs`, `Services/IAdminDashboardService.cs` |
| Helper service | `Services/OperationResult.cs`, `Services/PresentationFormatter.cs` |
| Data | `Data/ApplicationDbContextSeeder.cs`, `Data/DemoDataConstants.cs`, `Data/Entities/VenueEntity.cs`, `Data/Entities/CourtEntity.cs` |
| Model | `Models/Venue.cs`, `Models/Court.cs`, `Models/VenueStatus.cs`, `Models/VenueIndexViewModel.cs`, `Models/VenueDetailViewModel.cs`, `Models/AdminManagementViewModels.cs`, `Models/MockData.cs` |
| View | `Views/Venue/**`, `Views/Admin/**` |
| Shared UI liên quan sân/admin | `Views/Shared/_VenueCard.cshtml`, `Views/Shared/_CourtLines.cshtml`, `Views/Shared/_DemoAccountsPanel.cshtml`, `Views/Shared/_ValidationScriptsPartial.cshtml` |
| Frontend | `wwwroot/css/site.css`, `wwwroot/css/courtbook.css`, `wwwroot/js/site.js`, `wwwroot/js/venue-booking.js`, `wwwroot/js/venue-filter.js` |

#### Lộ trình commit của Trí

| Commit | Message đề xuất | Nội dung |
|---|---|---|
| T1 | `feat: them entity san bai va model venue` | VenueEntity, CourtEntity, Venue/Court model, VenueStatus. |
| T2 | `feat: them du lieu mau san va tai khoan demo` | Seeder, DemoDataConstants, MockData, dữ liệu sân/booking/demo user. |
| T3 | `feat: them danh sach san va bo loc` | VenueController index, VenueCatalogService search/filter, VenueIndexViewModel, view danh sách sân. |
| T4 | `feat: them chi tiet san va lich slot` | Detail view, slot matrix, `venue-booking.js`, chọn ngày/slot. |
| T5 | `feat: them dashboard quan tri admin` | AdminController index, AdminDashboardService, AdminManagementViewModels, view dashboard. |
| T6 | `feat: them quan ly cum san va san con` | View/Admin/Venues, tạo/sửa cụm sân, tạo/sửa sân con. |
| T7 | `feat: them duyet booking va khoa mo user` | Admin duyệt/từ chối booking, duyệt/ẩn venue, khóa/mở khóa player. |
| T8 | `style: hoan thien css va javascript giao dien` | CSS responsive, card sân, toast, filter, admin UI polish. |
| T9 | `test: kiem tra luong admin venue va booking` | Smoke test admin login, dashboard, venues, approve/reject, lock/unlock. |

#### Checklist nghiệm thu của Trí

- Trang danh sách sân load được và lọc/tìm kiếm được.
- Trang chi tiết sân hiển thị đúng courts và slot theo ngày.
- Slot pending/confirmed/cancelled hiển thị đúng trạng thái.
- Admin đăng nhập vào dashboard được.
- Admin xem thống kê user, sân, booking được.
- Admin tạo/sửa cụm sân và sân con được.
- Admin duyệt/từ chối booking được.
- Admin khóa/mở khóa player được, không khóa được admin.

## 5. File dùng chung cần phối hợp

Một số file không nên tự ý sửa một mình nếu thay đổi có ảnh hưởng toàn hệ thống.

| File | Chủ trì | Khi nào cần báo người còn lại |
|---|---|---|
| `Program.cs` | Kiệt | Khi đổi auth, DI, middleware, route. |
| `Data/ApplicationDbContext.cs` | Trí | Khi đổi schema, quan hệ entity, index. |
| `Data/Entities/AppUserEntity.cs` | Kiệt | Khi đổi field user, role, Identity. |
| `Data/Entities/BookingEntity.cs` | Kiệt | Khi đổi schema booking hoặc quan hệ user/court. |
| `Migrations/**` | Trí | Khi thêm/sửa migration ảnh hưởng DB. |
| `Views/Shared/_Layout.cshtml` | Kiệt | Khi đổi điều hướng, auth menu, role menu. |
| `wwwroot/css/**` | Trí | Khi đổi style dùng chung nhiều trang. |

## 6. Quy tắc commit và push

Commit message nên dùng dạng:

```text
feat: them ...
fix: sua ...
style: cap nhat ...
test: kiem tra ...
docs: cap nhat ...
```

Quy trình chuẩn:

```bash
git fetch origin scale-down-2-roles
git status
dotnet build
dotnet ef migrations has-pending-model-changes
git add <file da sua>
git commit -m "<message>"
git push origin scale-down-2-roles
```

Nếu pull về có conflict, ưu tiên giữ đúng ownership:

- Conflict ở Account/Booking/Home: Kiệt xử lý chính.
- Conflict ở Venue/Admin/Data/CSS/JS: Trí xử lý chính.
- Conflict ở migration hoặc `ApplicationDbContext.cs`: hai người xem cùng nhau.

## 7. Checklist test cuối trước khi nộp

Mỗi lần gộp xong một nhóm commit lớn, cần kiểm tra tối thiểu:

```bash
dotnet build
dotnet ef database update
dotnet ef migrations has-pending-model-changes
```

Smoke test thủ công:

| Luồng | Kết quả cần đạt |
|---|---|
| Public | Trang chủ, danh sách sân, chi tiết sân trả 200. |
| Player | Login, profile, đặt sân, xem lịch, hủy booking hoạt động. |
| Admin | Login, dashboard, quản lý sân, duyệt booking, khóa/mở user hoạt động. |
| Auth | User chưa đăng nhập bị redirect về login khi vào trang cần quyền. |
| Role | Player không vào được admin, Admin không bị khóa bởi chính admin. |

## 8. Tài khoản demo

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Player | `player@courtbook.local` | `Player@123` |
| Admin | `admin@courtbook.local` | `Admin@123` |

## 9. Ghi chú sau refactor Identity

Từ commit `80d5ce2`, hệ thống đã chuyển sang ASP.NET Core Identity:

- User lưu ở bảng `AspNetUsers`.
- Role lưu ở `AspNetRoles`.
- Mapping user-role lưu ở `AspNetUserRoles`.
- Login path chính là `/Identity/Account/Login`.
- Profile path chính là `/Identity/Account/Manage`, hiện redirect về trang profile MVC cũ.
- Không sử dụng lại cột `Role`, `NormalizedPhoneNumber`, `IsPhoneVerified` kiểu cũ trên `Users`.

Khi viết code mới, không tự hash password thủ công. Luôn dùng `UserManager`, `SignInManager`, `RoleManager` và claim hiện tại.
