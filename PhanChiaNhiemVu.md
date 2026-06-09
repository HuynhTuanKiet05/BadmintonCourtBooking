# 📋 Kế hoạch phân chia nhiệm vụ dự án BadmintonCourtBooking

## Mục tiêu
Chia đều các chức năng của dự án cho **2 thành viên** để đẩy code lên Git, đảm bảo mỗi người có **lượt commit đều nhau** và hợp lý.

---

## 📊 Tổng quan dự án

**Kiến trúc:** ASP.NET Core MVC + Entity Framework Core + SQL Server

| Tầng | Mô tả |
|------|--------|
| **Controllers** | 5 controller (Home, Account, Venue, Booking, Admin) |
| **Services** | 7 service chính + interfaces |
| **Models/Entities** | 12 model + 4 entity |
| **Views** | 6 thư mục view (Home, Account, Venue, Booking, Admin, Shared) |
| **Data** | DbContext, Seeder, Migrations |
| **Frontend** | 2 CSS + 3 JS + Shared partials |
| **Config** | Program.cs, appsettings |

---

## 👥 Phân chia nhiệm vụ

---

### 🟢 Thành viên 1: HuynhTuanKiet05 — Module Người dùng & Đặt sân

> **Phụ trách:** Xác thực, Hồ sơ người dùng, Trang chủ, Đặt lịch sân, Giao diện chung

---

#### ✅ Commit 1: Khởi tạo dự án & cấu hình cơ bản
**Message:** `feat: Khởi tạo project ASP.NET Core MVC`

| File | Mô tả |
|------|--------|
| `BadmintonCourtBooking.sln` | Solution file |
| `BadmintonCourtBooking/BadmintonCourtBooking.csproj` | Project file + NuGet packages |
| `BadmintonCourtBooking/Program.cs` | Cấu hình DI, Authentication, Middleware |
| `BadmintonCourtBooking/appsettings.json` | Connection string, cấu hình |
| `BadmintonCourtBooking/appsettings.Development.json` | Cấu hình dev |
| `.gitignore`, `.gitattributes` | Git config |

---

#### ✅ Commit 2: Tầng Data — Entity & DbContext
**Message:** `feat: Thêm Entity models và DbContext`

| File | Mô tả |
|------|--------|
| `Data/Entities/AppUserEntity.cs` | Entity người dùng |
| `Data/Entities/BookingEntity.cs` | Entity đặt sân |
| `Data/ApplicationDbContext.cs` | DbContext chính |
| `Data/ApplicationDbContextFactory.cs` | Design-time factory |

---

#### ✅ Commit 3: Migration khởi tạo database
**Message:** `feat: Thêm migration khởi tạo database`

| File | Mô tả |
|------|--------|
| `Migrations/20260526091514_InitialCreate.cs` | Migration tạo bảng Venues, Courts, Bookings |
| `Migrations/20260526091514_InitialCreate.Designer.cs` | Designer file |

---

#### ✅ Commit 4: Xác thực & đăng nhập
**Message:** `feat: Thêm chức năng đăng nhập, đăng ký, cookie authentication`

| File | Mô tả |
|------|--------|
| `Controllers/AccountController.cs` | Controller xử lý Login, Register, Logout, ExternalLogin |
| `Services/IAccountService.cs` | Interface account service |
| `Services/AccountService.cs` | Logic đăng nhập, đăng ký, hash password |
| `Services/AccountValueNormalizer.cs` | Chuẩn hóa email/phone |
| `Models/AccountViewModel.cs` | ViewModel đăng nhập/đăng ký/hồ sơ |
| `Models/AppRoles.cs` | Định nghĩa vai trò Admin/Player |

---

#### ✅ Commit 5: Giao diện đăng nhập & đăng ký
**Message:** `feat: Thêm giao diện trang đăng nhập và đăng ký`

| File | Mô tả |
|------|--------|
| `Views/Account/Login.cshtml` | Trang đăng nhập (form, social login) |
| `Views/Account/Register.cshtml` | Trang đăng ký (form validation) |
| `Views/Account/AccessDenied.cshtml` | Trang từ chối truy cập |

---

#### ✅ Commit 6: Hồ sơ người dùng & đổi mật khẩu
**Message:** `feat: Thêm trang hồ sơ cá nhân, đổi mật khẩu, cài đặt thông báo`

| File | Mô tả |
|------|--------|
| `Views/Account/Profile.cshtml` | Giao diện hồ sơ, đổi mật khẩu, thông báo |
| `Services/ICurrentUserService.cs` | Interface lấy user hiện tại |
| `Services/CurrentUserService.cs` | Lấy thông tin user từ Claims |
| `Services/RequireActiveUserFilter.cs` | Filter kiểm tra user bị khóa |

---

#### ✅ Commit 7: Trang chủ & đặt nhanh
**Message:** `feat: Thêm trang chủ với sân nổi bật và đặt nhanh`

| File | Mô tả |
|------|--------|
| `Controllers/HomeController.cs` | Controller trang chủ + hero slot matrix |
| `Views/Home/Index.cshtml` | Giao diện trang chủ, sân nổi bật, đặt nhanh |
| `Views/Home/Privacy.cshtml` | Trang chính sách |
| `Models/ErrorViewModel.cs` | Model lỗi |

---

#### ✅ Commit 8: Đặt sân & lịch đặt
**Message:** `feat: Thêm chức năng đặt sân và xem lịch đặt của tôi`

| File | Mô tả |
|------|--------|
| `Controllers/BookingController.cs` | Controller xem/hủy booking |
| `Services/IBookingService.cs` | Interface booking service |
| `Services/BookingService.cs` | Logic tạo/hủy booking, check trùng |
| `Views/Booking/Index.cshtml` | Giao diện "Lịch của tôi" |
| `Models/CreateBookingInputModel.cs` | Input model đặt sân |
| `Models/Booking.cs` | Model booking |

---

#### ✅ Commit 9: Layout, Footer & Shared partials
**Message:** `feat: Thêm layout chung, footer, toast và partial views`

| File | Mô tả |
|------|--------|
| `Views/Shared/_Layout.cshtml` | Layout chính (navbar, logo, responsive menu) |
| `Views/Shared/_Footer.cshtml` | Footer trang web |
| `Views/Shared/_Toast.cshtml` | Toast notification |
| `Views/Shared/_EmptyBookingsState.cshtml` | Trạng thái trống |
| `Views/Shared/_StatusBadge.cshtml` | Badge trạng thái |
| `Views/Shared/Error.cshtml` | Trang lỗi |
| `Views/_ViewImports.cshtml` | Import chung |
| `Views/_ViewStart.cshtml` | ViewStart |

---

---

### 🔵 Thành viên 2: Đức Trí — Module Quản trị & Sân bãi

> **Phụ trách:** Quản trị Admin, Quản lý cụm sân/sân con, Danh sách & chi tiết sân, Giao diện CSS/JS, Dữ liệu mẫu

---

#### ✅ Commit 1: Entity sân bãi
**Message:** `feat: Thêm Entity cụm sân và sân con`

| File | Mô tả |
|------|--------|
| `Data/Entities/VenueEntity.cs` | Entity cụm sân |
| `Data/Entities/CourtEntity.cs` | Entity sân con |
| `Models/Venue.cs` | Model cụm sân |
| `Models/Court.cs` | Model sân con |
| `Models/VenueStatus.cs` | Enum trạng thái sân |

---

#### ✅ Commit 2: Migration xác thực & vai trò
**Message:** `feat: Thêm migration hệ thống xác thực và vai trò người dùng`

| File | Mô tả |
|------|--------|
| `Migrations/20260526095517_AddAuthenticationAndUsers.cs` | Migration thêm bảng Users |
| `Migrations/20260526095517_AddAuthenticationAndUsers.Designer.cs` | Designer file |
| `Migrations/20260609062323_ScaleDownToAdminPlayerRoles.cs` | Migration rút gọn vai trò |
| `Migrations/20260609062323_ScaleDownToAdminPlayerRoles.Designer.cs` | Designer file |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | DB snapshot |

---

#### ✅ Commit 3: Dữ liệu mẫu (Seed Data)
**Message:** `feat: Thêm dữ liệu mẫu cho sân, booking và tài khoản demo`

| File | Mô tả |
|------|--------|
| `Data/ApplicationDbContextSeeder.cs` | Seed data sân, booking, users |
| `Data/DemoDataConstants.cs` | Hằng số dữ liệu demo |
| `Models/MockData.cs` | Dữ liệu mock cho dev |

---

#### ✅ Commit 4: Danh sách sân & tìm kiếm/lọc
**Message:** `feat: Thêm trang danh sách sân với bộ lọc quận huyện và tìm kiếm`

| File | Mô tả |
|------|--------|
| `Controllers/VenueController.cs` | Controller danh sách/chi tiết/đặt sân |
| `Services/IVenueCatalogService.cs` | Interface venue service |
| `Services/VenueCatalogService.cs` | Logic tìm kiếm, lọc, slot matrix |
| `Models/VenueIndexViewModel.cs` | ViewModel danh sách sân |
| `Views/Venue/Index.cshtml` | Giao diện danh sách sân |

---

#### ✅ Commit 5: Chi tiết sân & lịch đặt
**Message:** `feat: Thêm trang chi tiết cụm sân với lịch đặt theo ngày`

| File | Mô tả |
|------|--------|
| `Views/Venue/Detail.cshtml` | Giao diện chi tiết sân + bảng slot |
| `Models/VenueDetailViewModel.cs` | ViewModel chi tiết sân |
| `wwwroot/js/venue-booking.js` | JS xử lý chọn slot, đổi ngày, đặt sân |
| `Views/Shared/_CourtLines.cshtml` | SVG đường kẻ sân |

---

#### ✅ Commit 6: Trang quản trị Admin (Dashboard)
**Message:** `feat: Thêm trang quản trị Admin với dashboard thống kê`

| File | Mô tả |
|------|--------|
| `Controllers/AdminController.cs` | Controller admin (duyệt sân, booking, user) |
| `Services/IAdminDashboardService.cs` | Interface admin service |
| `Services/AdminDashboardService.cs` | Logic dashboard, duyệt, khóa user |
| `Models/AdminManagementViewModels.cs` | ViewModels quản trị |
| `Views/Admin/Index.cshtml` | Giao diện dashboard admin |

---

#### ✅ Commit 7: Quản lý cụm sân (Admin)
**Message:** `feat: Thêm trang quản lý cụm sân và sân con cho Admin`

| File | Mô tả |
|------|--------|
| `Views/Admin/Venues.cshtml` | Giao diện quản lý sân (CRUD) |
| `Views/Shared/_VenueCard.cshtml` | Card sân dùng chung |
| `Views/Shared/_DemoAccountsPanel.cshtml` | Panel tài khoản demo |

---

#### ✅ Commit 8: Giao diện CSS toàn bộ
**Message:** `feat: Thêm toàn bộ stylesheet cho giao diện web`

| File | Mô tả |
|------|--------|
| `wwwroot/css/site.css` | CSS chính (layout, navbar, cards, forms...) |
| `wwwroot/css/courtbook.css` | CSS bổ sung (booking, admin, responsive...) |
| `Views/Shared/_Layout.cshtml.css` | CSS scoped cho layout |
| `wwwroot/favicon.ico` | Favicon |

---

#### ✅ Commit 9: JavaScript chức năng & tiện ích
**Message:** `feat: Thêm JavaScript xử lý filter sân, toast và tương tác`

| File | Mô tả |
|------|--------|
| `wwwroot/js/site.js` | JS chung (toast, animations, navbar) |
| `wwwroot/js/venue-filter.js` | JS bộ lọc sân |
| `Views/Shared/_ValidationScriptsPartial.cshtml` | Scripts validation |
| `Services/OperationResult.cs` | Model kết quả thao tác |
| `Services/PresentationFormatter.cs` | Format hiển thị ngày/giờ |
| `Extensions/ModelStateDictionaryExtensions.cs` | Extension lấy lỗi validation |

---

## 📈 Bảng tổng kết

| Tiêu chí | HuynhTuanKiet05 | Đức Trí |
|----------|:---:|:---:|
| **Số commit** | 9 | 9 |
| **Module chính** | Account, Home, Booking, Layout | Venue, Admin, CSS/JS, Data |
| **Controllers** | 3 (Home, Account, Booking) | 2 (Venue, Admin) |
| **Services** | 5 file | 6 file |
| **Views** | ~10 file | ~10 file |
| **Models** | 5 file | 7 file |
| **Data/Migrations** | 4 file | 8 file |
| **Frontend (CSS/JS)** | — | 6 file |
| **Tổng ~file** | ~28 | ~32 |

---

## 🔧 Cách đẩy code lên Git

Cả 2 người đều commit trực tiếp trên nhánh `main`. Trước khi commit luôn pull về trước:

```bash
git pull origin main
git add <các file thuộc commit>
git commit -m "feat: <message>"
git push origin main
```

**Lưu ý:** Nên commit xen kẽ giữa 2 người để git history trông tự nhiên (ví dụ: Kiệt commit sáng, Trí commit chiều).
