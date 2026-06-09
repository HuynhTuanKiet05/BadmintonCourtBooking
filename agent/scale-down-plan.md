# Scale-down Plan: CourtBook 2 Roles

> **Phiên bản:** 1.0
> **Ngày lập:** 09/06/2026
> **Tech stack:** ASP.NET Core MVC (.NET 9), EF Core, SQL Server (LocalDB), Cookie Authentication
> **Branch đề xuất:** `scale-down-2-roles`

---

## 1. Mục tiêu cuối cùng

Sau khi scale-down, dự án CourtBook chỉ còn **2 vai trò**:

| Vai trò | Mô tả |
|---------|--------|
| **Admin** | Quản lý toàn bộ hệ thống: venue, court, booking, user. Thay thế hoàn toàn trách nhiệm của Owner cũ. |
| **Player** | Đăng ký, đăng nhập, tìm sân, đặt sân, xem lịch đặt, hủy booking. |

**Không còn:**
- Flow "chủ sân tự đăng venue chờ duyệt".
- Flow "owner approve/reject booking".
- Trang dành cho chủ sân.
- Role Owner trong database, UI, auth, seed data.

**Vẫn giữ nguyên:**
- Toàn bộ flow đặt sân của Player.
- Admin dashboard hiện tại (mở rộng thêm quyền quản lý venue/court/booking).
- Public venue search, detail, slot matrix.
- Cookie authentication custom.

---

## 2. Phạm vi giữ lại

| Module | Mô tả |
|--------|--------|
| Auth admin/player | Login, register, profile cho Player. Login cho Admin (không cho Admin tự đăng ký). |
| Public venue search | Trang tìm sân (`Venue/Index`) – anonymous + tất cả role. |
| Venue detail | Trang chi tiết sân + slot matrix (`Venue/Detail`, `Venue/GetSlots`). |
| Booking flow | Player đặt sân (`Venue/BookSlot`). |
| Player booking history | Xem lịch đặt + hủy (`Booking/Index`, `Booking/Cancel`). |
| Admin dashboard | Tổng quan hệ thống (`Admin/Index`). |
| Admin venue management | Admin quản lý venue/court (logic chuyển từ Owner sang). |
| Admin booking management | Admin xác nhận/hủy booking pending (logic chuyển từ Owner sang). |
| Admin user management | Lock/unlock user. |

---

## 3. Phạm vi loại bỏ

| Phần loại bỏ | File/Module liên quan |
|-------------|----------------------|
| Owner role constant | `Models/AppRoles.cs` – xóa `Owner` constant |
| OwnerController | `Controllers/OwnerController.cs` – xóa toàn bộ |
| Owner views | `Views/Owner/Dashboard.cshtml`, `Views/Owner/Venues.cshtml` – xóa thư mục |
| IOwnerDashboardService | `Services/IOwnerDashboardService.cs` – xóa |
| OwnerDashboardService | `Services/OwnerDashboardService.cs` – xóa (sau khi merge logic cần thiết sang Admin) |
| OwnerManagementViewModels | `Models/OwnerManagementViewModels.cs` – xóa |
| Owner demo account | `Data/DemoDataConstants.cs` – xóa các constant `DemoOwner*` |
| Owner login/register tab | `Views/Account/Login.cshtml`, `Register.cshtml` – xóa tab chủ sân |
| Owner demo button | `Views/Shared/_DemoAccountsPanel.cshtml` – xóa button chủ sân demo |
| Owner nav links | `Views/Shared/_Layout.cshtml` – xóa "Dành cho chủ sân" |
| Owner footer links | `Views/Shared/_Footer.cshtml` – xóa link chủ sân |
| Owner section on Home | `Views/Home/Index.cshtml` – xóa section "Dành cho chủ sân" |
| OwnerUserId (database) | `VenueEntity.OwnerUserId` – drop FK + column |
| Venue approval flow kiểu owner gửi | Seeder không còn PendingVenues từ owner |
| Owner-specific JS | `wwwroot/js/owner-dashboard.js` – xóa |
| OwnerBooking model | `Models/Booking.cs` – xóa class `OwnerBooking` |
| Owner seed users | Seeder – chuyển owner users thành Player |
| OwnerName/OwnerPhone columns | Rename thành `ContactName`/`ContactPhone` |
| MockData owner-related | `Models/MockData.cs` – xóa `OwnerBookings`, `PendingVenues`, `GetDashboardViewModel()` |
| DashboardViewModel | `Models/DashboardViewModel.cs` – xóa (model cũ cho mock) |
| AdminViewModel | `Models/AdminViewModel.cs` – xóa (model cũ cho mock) |

---

## 4. Role matrix mới

| Chức năng | Anonymous | Player | Admin |
|-----------|:---------:|:------:|:-----:|
| Xem trang chủ | ✅ | ✅ | ✅ |
| Tìm sân (Venue/Index) | ✅ | ✅ | ✅ |
| Xem chi tiết sân (Venue/Detail) | ✅ | ✅ | ✅ |
| Xem slot matrix (Venue/GetSlots) | ✅ | ✅ | ✅ |
| Đăng ký tài khoản | ✅ | ❌ | ❌ |
| Đăng nhập | ✅ | ❌ | ❌ |
| Đặt sân (Venue/BookSlot) | ❌ | ✅ | ❌ |
| Xem lịch đặt (Booking/Index) | ❌ | ✅ | ❌ |
| Hủy booking (Booking/Cancel) | ❌ | ✅ | ❌ |
| Xem profile (Account/Profile) | ❌ | ✅ | ✅ |
| Admin dashboard (Admin/Index) | ❌ | ❌ | ✅ |
| Admin quản lý venue/court | ❌ | ❌ | ✅ |
| Admin xác nhận/hủy booking | ❌ | ❌ | ✅ |
| Admin lock/unlock user | ❌ | ❌ | ✅ |
| Admin tạo venue mới | ❌ | ❌ | ✅ |
| Admin tạo court mới | ❌ | ❌ | ✅ |

---

## 5. Mapping role cũ sang role mới

### Owner → Loại bỏ hoàn toàn

| Khía cạnh | Xử lý |
|-----------|--------|
| Quyền quản lý venue/court | Chuyển cho Admin |
| Quyền approve/reject booking | Chuyển cho Admin |
| Owner users trong DB | **Chuyển Role thành Player** (không xóa để giữ lịch sử booking) |
| OwnerUserId trên Venue | Drop column – Admin quản lý toàn bộ venue, không cần FK owner |
| OwnerName/OwnerPhone trên Venue | Rename thành `ContactName`/`ContactPhone` – đây là thông tin liên hệ sân, không phải thông tin chủ sân |

### Admin → Giữ nguyên + mở rộng

Admin nhận toàn bộ quyền quản lý mà Owner đang làm:
- Quản lý danh sách venue (CRUD).
- Quản lý sân con / court (CRUD).
- Xem danh sách booking.
- Xác nhận (approve) hoặc từ chối (reject) booking pending.
- Lock/unlock user.

### Player → Giữ nguyên

Flow đặt sân không thay đổi. Booking vẫn tạo với status `Pending`, Admin sẽ xác nhận thay Owner.

### Xử lý Owner users cũ trong DB

**Phương án chọn: Chuyển `Role` từ `Owner` thành `Player`**

| Tiêu chí | Ưu điểm | Nhược điểm |
|----------|---------|------------|
| Giữ dữ liệu | Không mất user, booking liên kết vẫn valid | Owner users giờ thành Player, có thể confuse nếu xem DB trực tiếp |
| An toàn | Không vi phạm FK constraint | Cần update seed data tương ứng |
| Đơn giản | 1 lệnh SQL UPDATE | Không có |

**Lý do không chọn xóa:** Các owner user cũ có thể đã tạo booking (qua `ResolveBookingUserId` trong seeder, ví dụ `user-owner-linh`, `user-owner-tuan-anh`). Xóa user sẽ khiến FK `Bookings.PlayerUserId` bị set null (do `OnDelete(SetNull)`) – làm mất liên kết booking-player. Chuyển role an toàn hơn.

---

## 6. Route map mới

### Routes giữ lại

| Route | Controller/Action | Ghi chú |
|-------|------------------|---------|
| `/` | `Home/Index` | Giữ, xóa section "Dành cho chủ sân" |
| `/Venue` | `Venue/Index` | Giữ nguyên |
| `/Venue/Detail/{id}` | `Venue/Detail` | Giữ, `OwnerPhone` đổi thành `ContactPhone` |
| `/Venue/GetSlots` | `Venue/GetSlots` | Giữ nguyên |
| `/Venue/BookSlot` | `Venue/BookSlot` | Giữ nguyên |
| `/Booking` | `Booking/Index` | Giữ nguyên |
| `/Booking/Cancel` | `Booking/Cancel` | Giữ nguyên |
| `/Account/Login` | `Account/Login` | Giữ, xóa tab Owner |
| `/Account/Register` | `Account/Register` | Giữ, xóa tab Owner, chỉ cho đăng ký Player |
| `/Account/Profile` | `Account/Profile` | Giữ nguyên |
| `/Account/Logout` | `Account/Logout` | Giữ nguyên |
| `/Account/AccessDenied` | `Account/AccessDenied` | Giữ nguyên |
| `/Admin` | `Admin/Index` | Giữ, mở rộng |
| `/Admin/ApproveVenue` | `Admin/ApproveVenue` | Giữ nguyên |
| `/Admin/RejectVenue` | `Admin/RejectVenue` | Giữ nguyên |
| `/Admin/LockUser` | `Admin/LockUser` | Giữ nguyên |
| `/Admin/UnlockUser` | `Admin/UnlockUser` | Giữ nguyên |
| `/Admin/ApproveBooking` | `Admin/ApproveBooking` | **MỚI** – chuyển từ Owner |
| `/Admin/RejectBooking` | `Admin/RejectBooking` | **MỚI** – chuyển từ Owner |
| `/Admin/CreateVenue` | `Admin/CreateVenue` | **MỚI** – chuyển từ Owner |
| `/Admin/UpdateVenue` | `Admin/UpdateVenue` | **MỚI** – chuyển từ Owner |
| `/Admin/CreateCourt` | `Admin/CreateCourt` | **MỚI** – chuyển từ Owner |
| `/Admin/UpdateCourt` | `Admin/UpdateCourt` | **MỚI** – chuyển từ Owner |
| `/Admin/Venues` | `Admin/Venues` | **MỚI** – trang quản lý venue/court |

### Routes xóa hoặc redirect

| Route | Hành động |
|-------|-----------|
| `/Owner/Dashboard` | **Xóa** – redirect về `/Admin` nếu admin, hoặc `/` nếu player |
| `/Owner/Venues` | **Xóa** |
| `/Owner/CreateVenue` | **Xóa** |
| `/Owner/UpdateVenue` | **Xóa** |
| `/Owner/CreateCourt` | **Xóa** |
| `/Owner/UpdateCourt` | **Xóa** |
| `/Owner/ApproveBooking` | **Xóa** |
| `/Owner/RejectBooking` | **Xóa** |

---

## 7. Database redesign plan

### 7.1 Schema hiện tại

#### Bảng `Users`
| Cột | Kiểu | Ghi chú |
|-----|------|---------|
| Id | string (PK) | |
| FullName | nvarchar(120) | |
| Email | nvarchar(160) | |
| NormalizedEmail | nvarchar(160) | Unique index |
| PhoneNumber | nvarchar(30) | |
| NormalizedPhoneNumber | nvarchar(30) | Unique index |
| PasswordHash | nvarchar(512) | |
| **Role** | nvarchar(20) | **Hiện có: Player, Owner, Admin** |
| PlayArea | nvarchar(120) | |
| IsActive | bit | |
| IsPhoneVerified | bit | |
| ReceiveBookingConfirm | bit | |
| ReceivePlayReminder | bit | |
| ReceivePromo | bit | |
| JoinedAt | datetime2 | |
| UpdatedAt | datetime2 | |
| LastSignInAt | datetime2? | |

#### Bảng `Venues`
| Cột | Kiểu | Ghi chú |
|-----|------|---------|
| Id | string (PK) | |
| Name | nvarchar(160) | |
| District | nvarchar(80) | |
| Address | nvarchar(240) | |
| OpenHours | nvarchar(40) | |
| **OwnerUserId** | string? (FK → Users.Id) | **Cần drop** |
| **OwnerName** | nvarchar(120) | **Cần rename → ContactName** |
| **OwnerPhone** | nvarchar(30) | **Cần rename → ContactPhone** |
| Description | nvarchar(2000) | |
| Highlight | nvarchar(200)? | |
| Rating | float | |
| Reviews | int | |
| ResponseFast | bit | |
| HasSlotsToday | bit | |
| **Status** | nvarchar(40) | **Enum: Approved, PendingApproval, Rejected** |
| CreatedAt | datetime2 | |
| UpdatedAt | datetime2 | |

FK: `Venues.OwnerUserId` → `Users.Id` (SetNull)
Index: `IX_Venues_OwnerUserId`

#### Bảng `Courts` – Không thay đổi
#### Bảng `Bookings` – Không thay đổi
#### Bảng `UserSnapshots` – Đánh giá cần xóa

### 7.2 Schema mục tiêu

#### Bảng `Users` – Sau scale-down
| Thay đổi | Chi tiết |
|----------|---------|
| Role | Chỉ còn giá trị `Admin` hoặc `Player`. Không có `Owner`. |
| Dữ liệu | Tất cả users có `Role = 'Owner'` → update thành `Role = 'Player'` |

#### Bảng `Venues` – Sau scale-down
| Thay đổi | Chi tiết |
|----------|---------|
| Drop `OwnerUserId` | Xóa FK + index + column |
| Rename `OwnerName` → `ContactName` | Thông tin liên hệ sân, không còn gắn với chủ sân |
| Rename `OwnerPhone` → `ContactPhone` | Hotline/SĐT liên hệ sân |
| Status | **Giữ enum `VenueStatus` hiện tại nhưng đổi ý nghĩa** (xem bên dưới) |

**Quyết định về VenueStatus:**

> **Khuyến nghị: Hướng A – Giữ VenueStatus enum hiện tại, đổi ý nghĩa**

| Giá trị cũ | Ý nghĩa cũ | Ý nghĩa mới |
|-------------|-------------|--------------|
| `Approved` | Owner gửi, admin duyệt → công khai | Admin quản lý → đang hiển thị công khai |
| `PendingApproval` | Owner gửi chờ admin duyệt | **Loại bỏ khỏi seed và UI.** Migration sẽ update `PendingApproval` → `Approved`. |
| `Rejected` | Admin từ chối hồ sơ owner | Admin ẩn venue → đang ẩn khỏi public |

**Lý do chọn hướng A thay vì tạo enum mới (Active/Hidden):**
1. Không cần sửa kiểu dữ liệu trong DB (Status lưu dạng string, giá trị `Approved`/`Rejected` vẫn valid).
2. Không cần tạo migration đổi giá trị enum.
3. Ít rủi ro vỡ logic query hiện tại (`VenueStatus.Approved` đã dùng khắp nơi).
4. Phù hợp đồ án: demo dễ, ít bug, dễ giải thích.

#### Bảng `Courts` – Không thay đổi

#### Bảng `Bookings` – Không thay đổi
- `PlayerUserId` FK giữ nguyên.
- Booking không phụ thuộc Owner – chỉ liên kết Player + Court.

#### Bảng `UserSnapshots` – Xóa bảng

**Lý do xóa:**
- Bảng này chỉ chứa dữ liệu mock (`RecentUsers` trong `MockData.cs`) dùng cho admin dashboard cũ (mock).
- Admin dashboard hiện tại đã query real-time từ bảng `Users` (xem `AdminDashboardService.GetDashboardAsync()`).
- Bảng không có FK nào trỏ tới, xóa an toàn.
- Giảm bớt code maintain (entity, seeder, DbContext mapping).

### 7.3 Migration đề xuất

**Tên migration:** `ScaleDownToAdminPlayerRoles`

| # | Thao tác | Bảng | Cột/Object | Lý do | Rủi ro |
|---|---------|------|-----------|-------|--------|
| 1 | **UPDATE DATA** | Users | Role | Chuyển tất cả `Role = 'Owner'` → `'Player'` | Thấp – chỉ update giá trị, không thay đổi schema |
| 2 | **UPDATE DATA** | Venues | Status | Chuyển tất cả `Status = 'PendingApproval'` → `'Approved'` | Thấp – pending venues sẽ hiển thị public ngay |
| 3 | **DROP FK** | Venues | FK_Venues_Users_OwnerUserId | Không còn owner relationship | Thấp – EF Core mapping sẽ đồng bộ |
| 4 | **DROP INDEX** | Venues | IX_Venues_OwnerUserId | Index không cần khi cột bị xóa | Không |
| 5 | **DROP COLUMN** | Venues | OwnerUserId | Venue không còn thuộc owner | **Trung bình** – Phải sửa tất cả query/include OwnerUserId trước |
| 6 | **RENAME COLUMN** | Venues | OwnerName → ContactName | Clean schema – venue contact info, không phải owner info | **Trung bình** – Phải sửa entity, view, service, seeder đồng loạt |
| 7 | **RENAME COLUMN** | Venues | OwnerPhone → ContactPhone | Clean schema – hotline sân | **Trung bình** – Tương tự rename ContactName |
| 8 | **DROP TABLE** | UserSnapshots | (toàn bảng) | Không còn sử dụng | Thấp – không có FK nào trỏ tới |

### 7.4 Data migration

#### Users – Role Owner xử lý
```sql
-- Trong migration Up()
UPDATE [Users] SET [Role] = 'Player' WHERE [Role] = 'Owner';
```
- **Lý do:** Không xóa user để giữ liên kết booking (PlayerUserId).
- **Rủi ro:** Không có. Unique indexes trên Email/Phone không bị ảnh hưởng vì không thay đổi giá trị đó.

#### Venues – OwnerUserId xử lý
```sql
-- Trong migration Up(): EF Core tự generate DROP FK, DROP INDEX, DROP COLUMN
-- Không cần migrate dữ liệu vì column bị xóa hoàn toàn
```

#### Venues – OwnerName/OwnerPhone rename
```sql
-- EF Core sẽ generate RenameColumn
EXEC sp_rename 'Venues.OwnerName', 'ContactName', 'COLUMN';
EXEC sp_rename 'Venues.OwnerPhone', 'ContactPhone', 'COLUMN';
```
- **Dữ liệu:** Giữ nguyên giá trị hiện có. Ví dụ: "Anh Hoàng Nam" → vẫn là "Anh Hoàng Nam" trong `ContactName`.
- **Ý nghĩa:** Giờ hiểu là "người liên hệ tại sân" thay vì "chủ sân".

#### Venues – VenueStatus PendingApproval
```sql
-- Trong migration Up()
UPDATE [Venues] SET [Status] = 'Approved' WHERE [Status] = 'PendingApproval';
```
- **Lý do:** Không còn flow pending approval. Tất cả venue seed cũ nên hiển thị.
- **Phương án thay thế:** Update thành `Rejected` (ẩn) nếu muốn thận trọng hơn. Nhưng vì đây là đồ án demo, hiển thị hết sẽ cho kết quả demo tốt hơn.

#### Bookings – Không ảnh hưởng
- `PlayerUserId` FK vẫn giữ nguyên.
- Booking liên kết Court → Venue, không liên kết trực tiếp đến Owner.
- Message "Từ chối bởi chủ sân" trong `CancelReason` → nên cập nhật seed thành "Từ chối bởi quản trị viên" nhưng **không bắt buộc trong migration** – có thể sửa trong seeder.

#### UserSnapshots – Xóa bảng
```sql
-- Trong migration Up(): EF Core tự generate DROP TABLE
DROP TABLE [UserSnapshots];
```

### 7.5 EF Core changes

#### Entity sửa

| Entity | Thay đổi |
|--------|---------|
| `AppUserEntity` | Xóa navigation property `OwnedVenues` |
| `VenueEntity` | Xóa `OwnerUserId`, xóa `OwnerUser`. Rename `OwnerName` → `ContactName`, `OwnerPhone` → `ContactPhone` |
| `UserSnapshotEntity` | **Xóa file** |

#### AppUserEntity sau sửa
```csharp
public class AppUserEntity
{
    // ... giữ nguyên tất cả property ...
    // XÓA: public ICollection<VenueEntity> OwnedVenues { get; set; }
    public ICollection<BookingEntity> PlayerBookings { get; set; } = new List<BookingEntity>();
}
```

#### VenueEntity sau sửa
```csharp
public class VenueEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string OpenHours { get; set; } = string.Empty;
    // XÓA: public string? OwnerUserId { get; set; }
    public string ContactName { get; set; } = string.Empty;   // ĐỔI TÊN
    public string ContactPhone { get; set; } = string.Empty;  // ĐỔI TÊN
    public string Description { get; set; } = string.Empty;
    public string? Highlight { get; set; }
    public double Rating { get; set; }
    public int Reviews { get; set; }
    public bool ResponseFast { get; set; }
    public bool HasSlotsToday { get; set; }
    public VenueStatus Status { get; set; } = VenueStatus.Approved; // ĐỔI DEFAULT
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    // XÓA: public AppUserEntity? OwnerUser { get; set; }
    public ICollection<CourtEntity> Courts { get; set; } = new List<CourtEntity>();
}
```

#### DbContext mapping sửa

Trong `ApplicationDbContext.OnModelCreating()`:

**Venues mapping – XÓA:**
```csharp
// XÓA toàn bộ block này:
entity.Property(x => x.OwnerName).HasMaxLength(120).IsRequired();
entity.Property(x => x.OwnerPhone).HasMaxLength(30).IsRequired();
entity.HasOne(x => x.OwnerUser)
    .WithMany(x => x.OwnedVenues)
    .HasForeignKey(x => x.OwnerUserId)
    .OnDelete(DeleteBehavior.SetNull);
```

**Venues mapping – THÊM:**
```csharp
entity.Property(x => x.ContactName).HasMaxLength(120).IsRequired();
entity.Property(x => x.ContactPhone).HasMaxLength(30).IsRequired();
```

**UserSnapshots – XÓA toàn bộ:**
```csharp
// XÓA: public DbSet<UserSnapshotEntity> UserSnapshots => Set<UserSnapshotEntity>();
// XÓA: modelBuilder.Entity<UserSnapshotEntity>(...) block
```

#### Migration command

```bash
# Bước 1: Sửa entity + DbContext trước
# Bước 2: Tạo migration
dotnet ef migrations add ScaleDownToAdminPlayerRoles --project BadmintonCourtBooking

# Bước 3: Review file migration thủ công
# Bước 4: Thêm SQL data migration vào Up() nếu EF không tự generate

# Bước 5: Apply migration
dotnet ef database update --project BadmintonCourtBooking
```

**Quan trọng:** Sau khi EF tạo migration tự động, phải **review thủ công** file migration và thêm các lệnh SQL sau vào `Up()` **trước** khi drop column/table:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // DATA MIGRATION - thực hiện TRƯỚC khi thay đổi schema
    migrationBuilder.Sql("UPDATE [Users] SET [Role] = 'Player' WHERE [Role] = 'Owner'");
    migrationBuilder.Sql("UPDATE [Venues] SET [Status] = 'Approved' WHERE [Status] = 'PendingApproval'");

    // ... EF auto-generated schema changes (drop FK, index, column, rename, drop table) ...
}
```

#### Seed update

- Xóa toàn bộ owner seed accounts trong `BuildSeedAccounts()`.
- Chuyển các owner seed accounts mà có booking liên kết thành Player.
- Xóa `SyncVenueOwnersAsync()`.
- Xóa `MapPendingVenue()` – không còn pending venue seed.
- Xóa `ResolveOwnerUserId()`.
- Xóa `ResolveOwner()` – thay bằng seed `ContactName`/`ContactPhone` trực tiếp.
- Xóa `SeedUserSnapshotsAsync()`.
- Cập nhật `MapApprovedVenue()` để dùng `ContactName`/`ContactPhone`.
- Xóa reference `DemoDataConstants.DemoOwner*`.
- Cập nhật `MockData.OwnerBookings` – bỏ hoặc chuyển thành booking bình thường.
- Xóa `MockData.PendingVenues`.

### 7.6 Rollback plan

#### Rollback migration
```bash
# Quay về migration trước đó
dotnet ef database update AddAuthenticationAndUsers --project BadmintonCourtBooking
```

#### Rủi ro mất dữ liệu khi rollback

| Thao tác | Có thể rollback? | Dữ liệu mất |
|----------|:-----------------:|-------------|
| UPDATE Users.Role Owner→Player | ✅ Nếu ghi vào Down() | Không mất, nhưng cần `Down()` chạy ngược |
| UPDATE Venues.Status PendingApproval→Approved | ⚠️ Nếu ghi vào Down() | Không biết venue nào từng là PendingApproval |
| DROP FK Venues.OwnerUserId | ✅ EF tự generate | Không mất |
| DROP INDEX IX_Venues_OwnerUserId | ✅ EF tự generate | Không mất |
| DROP COLUMN Venues.OwnerUserId | ⚠️ Partial | **Giá trị OwnerUserId bị mất vĩnh viễn** |
| RENAME OwnerName→ContactName | ✅ sp_rename ngược | Không mất |
| RENAME OwnerPhone→ContactPhone | ✅ sp_rename ngược | Không mất |
| DROP TABLE UserSnapshots | ⚠️ | **Dữ liệu UserSnapshots bị mất** (nhưng chỉ là demo mock data, có thể seed lại) |

#### Down() migration cần viết
```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    // 1. Tạo lại bảng UserSnapshots
    // 2. Rename ContactName → OwnerName, ContactPhone → OwnerPhone
    // 3. Thêm lại column OwnerUserId (nullable)
    // 4. Tạo lại FK + index
    // 5. UPDATE Users SET Role = 'Owner' WHERE ... (KHÔNG thể biết chính xác user nào từng là Owner)

    // LƯU Ý: Sau rollback, OwnerUserId sẽ NULL cho tất cả venues
    // và không thể khôi phục giá trị gốc.
    // Khuyến nghị: BACKUP database trước khi chạy migration lên.
}
```

#### Khuyến nghị an toàn
1. **Backup database trước khi chạy migration:**
   ```bash
   # LocalDB backup
   sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "BACKUP DATABASE [CourtBook] TO DISK='D:\backup\CourtBook_pre_scaledown.bak'"
   ```
   Hoặc đơn giản: copy file `.mdf` trong thư mục user.

2. **Chạy migration trên DB test trước**, không chạy thẳng trên DB chính.

3. **Giữ migration file Down() đầy đủ** để có thể rollback.

---

## 8. File-by-file impact

### Controllers

| File | Hành động | Lý do | Rủi ro |
|------|----------|-------|--------|
| `Controllers/HomeController.cs` | Giữ nguyên | Không có logic Owner | Không |
| `Controllers/AccountController.cs` | **Sửa nhẹ** | Xóa redirect `AppRoles.Owner → Owner/Dashboard` ở `RedirectAfterAuthentication()`. Xóa logic sync OwnerName/OwnerPhone khi update profile. | Thấp |
| `Controllers/VenueController.cs` | Giữ nguyên | Không có logic Owner trực tiếp | Không |
| `Controllers/BookingController.cs` | Giữ nguyên | Không có logic Owner | Không |
| `Controllers/AdminController.cs` | **Sửa vừa** | Thêm action: `ApproveBooking`, `RejectBooking`, `Venues`, `CreateVenue`, `UpdateVenue`, `CreateCourt`, `UpdateCourt`. Tận dụng logic từ `OwnerDashboardService` (refactor thành Admin). | **Trung bình** – cần test kỹ |
| `Controllers/OwnerController.cs` | **Xóa** | Toàn bộ role Owner bị loại | Thấp – xóa sau khi chắc chắn AdminController đã có đủ action |

### Services

| File | Hành động | Lý do | Rủi ro |
|------|----------|-------|--------|
| `Services/AccountService.cs` | **Sửa nhẹ** | Xóa logic `role == AppRoles.Owner` trong `RegisterAsync()` (L:82-83, L:96-98). Xóa block sync OwnerName/OwnerPhone trong `UpdateProfileAsync()` (L:176-185). | Thấp |
| `Services/BookingService.cs` | **Sửa nhẹ** | Đổi message "Chủ sân sẽ xác nhận" → "Quản trị viên sẽ xác nhận" (L:108). | Không |
| `Services/VenueCatalogService.cs` | **Sửa nhẹ** | `MapVenue()` đổi `record.OwnerPhone` → `record.ContactPhone` (L:144). | Thấp |
| `Services/AdminDashboardService.cs` | **Sửa vừa** | Xóa logic đếm `TotalOwners`. Xóa reference `OwnerName`/`OwnerPhone` → đổi sang `ContactName`/`ContactPhone`. Xóa `venuesByOwner` dictionary. Thêm logic approve/reject booking (port từ `OwnerDashboardService`). Thêm logic CRUD venue/court (port từ `OwnerDashboardService`). Xóa `OwnedVenueCount` từ `AdminUserOverviewViewModel`. | **Trung bình** |
| `Services/IAdminDashboardService.cs` | **Sửa vừa** | Thêm interface methods cho venue/court CRUD + booking approve/reject. | Thấp |
| `Services/OwnerDashboardService.cs` | **Xóa** | Logic cần thiết đã merge sang AdminDashboardService | Thấp – xóa sau khi build pass |
| `Services/IOwnerDashboardService.cs` | **Xóa** | Interface không còn dùng | Thấp |
| `Services/CurrentUserService.cs` | Giữ nguyên | Không có logic Owner-specific | Không |
| `Services/RequireActiveUserFilter.cs` | Giữ nguyên | Kiểm tra IsActive cho tất cả role, không phân biệt Owner | Không |
| `Services/PresentationFormatter.cs` | Giữ nguyên | Utility class, không có logic Owner | Không |
| `Services/AccountValueNormalizer.cs` | Giữ nguyên | Utility class | Không |
| `Services/OperationResult.cs` | Giữ nguyên | Generic result class | Không |
| `Services/IBookingService.cs` | Giữ nguyên | Interface không đề cập Owner | Không |
| `Services/IVenueCatalogService.cs` | Giữ nguyên | Interface không đề cập Owner | Không |
| `Services/ICurrentUserService.cs` | Giữ nguyên | | Không |

### Models

| File | Hành động | Lý do | Rủi ro |
|------|----------|-------|--------|
| `Models/AppRoles.cs` | **Sửa** | Xóa `Owner` constant. Xóa case `Owner` trong `FromSelection`, `ToSelection`, `ToDisplayLabel`. | Thấp |
| `Models/AccountViewModel.cs` | **Sửa** | `LoginViewModel.Role`: đổi regex validation thành `"^(player)$"`. `RegisterViewModel.Role`: đổi regex validation thành `"^(player)$"`, xóa `owner` option. | Thấp |
| `Models/VenueStatus.cs` | **Sửa nhẹ** | Xóa `PendingApproval` khỏi enum (hoặc giữ lại để backward compat nhưng không dùng). **Khuyến nghị giữ lại enum value** nhưng thêm `[Obsolete]` attribute để tránh lỗi khi deserialize dữ liệu cũ. | Thấp |
| `Models/Venue.cs` | **Sửa nhẹ** | Đổi `OwnerPhone` → `ContactPhone` | Thấp |
| `Models/Booking.cs` | **Sửa** | Xóa class `OwnerBooking` | Thấp |
| `Models/OwnerManagementViewModels.cs` | **Xóa** | Toàn bộ viewmodel dành cho Owner | Thấp – xóa sau khi AdminController dùng viewmodel riêng |
| `Models/AdminManagementViewModels.cs` | **Sửa vừa** | Xóa `TotalOwners` từ `AdminDashboardViewModel`. Xóa `OwnedVenueCount` từ `AdminUserOverviewViewModel`. Đổi `OwnerName` → `ContactName` trong các venue VM. Thêm viewmodel cho admin venue/court management nếu cần. Xóa `AdminPendingVenueViewModel` (không còn pending venue flow kiểu owner gửi). | Trung bình |
| `Models/AdminViewModel.cs` | **Xóa** | Model mock cũ, không còn dùng (AdminDashboardViewModel đã thay thế) | Không |
| `Models/DashboardViewModel.cs` | **Xóa** | Model mock cũ cho Owner dashboard | Không |
| `Models/MockData.cs` | **Sửa vừa** | Xóa `OwnerBookings` list. Xóa `PendingVenues` list. Xóa `RecentUsers` list (hoặc cập nhật không còn "Chủ sân"). Xóa `GetDashboardViewModel()`. Xóa `GetAdminViewModel()`. Cập nhật `Venues` nếu có reference Owner. | Trung bình |
| `Models/VenueDetailViewModel.cs` | Giữ nguyên | Không đề cập Owner | Không |
| `Models/VenueIndexViewModel.cs` | Giữ nguyên | Không đề cập Owner | Không |
| `Models/CreateBookingInputModel.cs` | Giữ nguyên | Không đề cập Owner | Không |
| `Models/Court.cs` | Giữ nguyên | | Không |
| `Models/ErrorViewModel.cs` | Giữ nguyên | | Không |

### Data

| File | Hành động | Lý do | Rủi ro |
|------|----------|-------|--------|
| `Data/Entities/AppUserEntity.cs` | **Sửa** | Xóa `OwnedVenues` navigation property | Thấp |
| `Data/Entities/VenueEntity.cs` | **Sửa** | Xóa `OwnerUserId`, `OwnerUser`. Rename `OwnerName`→`ContactName`, `OwnerPhone`→`ContactPhone`. Đổi default `Status` sang `Approved`. | Trung bình |
| `Data/Entities/CourtEntity.cs` | Giữ nguyên | | Không |
| `Data/Entities/BookingEntity.cs` | Giữ nguyên | | Không |
| `Data/Entities/UserSnapshotEntity.cs` | **Xóa** | Bảng bị drop | Thấp |
| `Data/ApplicationDbContext.cs` | **Sửa** | Xóa `DbSet<UserSnapshotEntity>`. Xóa UserSnapshot mapping. Xóa Venue→OwnerUser relationship. Đổi `OwnerName`→`ContactName`, `OwnerPhone`→`ContactPhone` trong mapping. | Trung bình |
| `Data/ApplicationDbContextFactory.cs` | Giữ nguyên | Design-time factory, không có logic Owner | Không |
| `Data/ApplicationDbContextSeeder.cs` | **Sửa nặng** | Xóa owner seed accounts (hoặc chuyển Role thành Player). Xóa `SyncVenueOwnersAsync()`. Xóa `SeedUserSnapshotsAsync()`. Xóa `MapPendingVenue()`. Xóa `ResolveOwnerUserId()`. Cập nhật `ResolveOwner()` → hàm resolve ContactName/ContactPhone. Cập nhật `MapApprovedVenue()`. Cập nhật `MockData.OwnerBookings` reference. Xóa `MockData.PendingVenues` reference. | **Cao** – seeder phức tạp, cần test kỹ |
| `Data/DemoDataConstants.cs` | **Sửa** | Xóa `DemoOwner*` constants | Thấp |

### Views

| File | Hành động | Lý do | Rủi ro |
|------|----------|-------|--------|
| `Views/Shared/_Layout.cshtml` | **Sửa** | Xóa `User.IsInRole("Owner")` blocks (desktop + mobile nav). Xóa link "Dành cho chủ sân" cho anonymous. Xóa case `"Owner" => "Chủ cụm sân"` trong role label. | Thấp |
| `Views/Shared/_DemoAccountsPanel.cshtml` | **Sửa** | Xóa button "Chủ sân demo" | Thấp |
| `Views/Shared/_Footer.cshtml` | **Sửa** | Xóa link "Đăng ký chủ sân", "Quản lý cụm sân" | Thấp |
| `Views/Account/Login.cshtml` | **Sửa** | Xóa tab "Chủ sân". Role hidden field fixed = "player". Xóa Owner warning alert. | Trung bình – file lớn (46KB) |
| `Views/Account/Register.cshtml` | **Sửa** | Xóa tab "Chủ sân". Role hidden field fixed = "player". Xóa Owner warning alert. | Trung bình – file lớn (47KB) |
| `Views/Account/Profile.cshtml` | Giữ nguyên | Không có logic Owner-specific | Không |
| `Views/Account/AccessDenied.cshtml` | Giữ nguyên | | Không |
| `Views/Admin/Index.cshtml` | **Sửa vừa** | Xóa "chủ sân" references. Đổi `OwnerName` → `ContactName`. Xóa pending venue section kiểu "owner gửi". Thêm section approve/reject booking nếu cần. Thêm link "Quản lý cụm sân" nếu tách action `Admin/Venues`. Xóa column "Owner" trong bảng booking. | **Trung bình** |
| `Views/Owner/Dashboard.cshtml` | **Xóa** | Toàn bộ Owner view | Thấp |
| `Views/Owner/Venues.cshtml` | **Xóa** (sau khi port UI sang Admin) | Logic quản lý venue/court cần port sang Admin view | **Trung bình** – cần tạo `Views/Admin/Venues.cshtml` trước |
| `Views/Venue/Index.cshtml` | Giữ nguyên | Không có logic Owner | Không |
| `Views/Venue/Detail.cshtml` | **Sửa nhẹ** | Đổi `Model.Venue.OwnerPhone` → `Model.Venue.ContactPhone` (L:54) | Thấp |
| `Views/Booking/Index.cshtml` | Giữ nguyên | Không có logic Owner | Không |
| `Views/Home/Index.cshtml` | **Sửa** | Xóa section "Dành cho chủ sân" (L:114-145+). Xóa link Owner/Dashboard. | Thấp |

### Static files

| File | Hành động | Lý do | Rủi ro |
|------|----------|-------|--------|
| `wwwroot/js/owner-dashboard.js` | **Xóa** | Revenue chart cho Owner dashboard, không còn dùng | Thấp |
| `wwwroot/js/venue-booking.js` | Giữ nguyên | Logic booking slot, không liên quan Owner | Không |
| `wwwroot/js/venue-filter.js` | Giữ nguyên | Logic filter venue, không liên quan Owner | Không |
| `wwwroot/js/site.js` | **Kiểm tra** | Có thể có logic demo account fill cho Owner tab → xóa nếu có | Thấp |

### Cấu hình

| File | Hành động | Lý do | Rủi ro |
|------|----------|-------|--------|
| `Program.cs` | **Sửa** | Xóa `AddScoped<IOwnerDashboardService, OwnerDashboardService>()` (L:34) | Thấp |
| `appsettings.json` | Giữ nguyên | | Không |
| `appsettings.Development.json` | Giữ nguyên | | Không |

### Extensions

| File | Hành động | Lý do | Rủi ro |
|------|----------|-------|--------|
| `Extensions/ModelStateDictionaryExtensions.cs` | Giữ nguyên | Utility extension | Không |

---

## 9. Implementation phases

### Phase 1: Tạo branch và baseline
- [ ] Tạo branch mới: `scale-down-2-roles`
- [ ] Build project: `dotnet build`
- [ ] Chạy app: `dotnet run` → kiểm tra app hoạt động bình thường
- [ ] Ghi nhận danh sách migration hiện tại: `InitialCreate`, `AddAuthenticationAndUsers`
- [ ] Backup database (copy file `.mdf` hoặc dùng `sqlcmd`)

**Mục tiêu:** Baseline pass – project build và chạy được trước khi sửa.

---

### Phase 2: Sửa role ở tầng app (code-only, chưa đụng DB)

**Thứ tự sửa:**

1. `Models/AppRoles.cs`:
   - Xóa `public const string Owner = "Owner";`
   - Xóa case `"owner"` trong `FromSelection()` – để fall-through về Player
   - Xóa case `Owner` trong `ToSelection()` và `ToDisplayLabel()`

2. `Models/AccountViewModel.cs`:
   - `LoginViewModel.Role`: đổi regex `"^(player|owner)$"` → `"^(player)$"`
   - `RegisterViewModel.Role`: đổi regex `"^(player|owner)$"` → `"^(player)$"`

3. `Models/VenueStatus.cs`:
   - Giữ `PendingApproval` trong enum (để migration Up() có thể reference) nhưng thêm comment `// Deprecated – không còn dùng sau scale-down`

4. `Views/Account/Login.cshtml`:
   - Xóa tab "Chủ sân" trong UI tab selector
   - Hidden field `Role` fixed = "player"
   - Xóa Owner warning alert section

5. `Views/Account/Register.cshtml`:
   - Xóa tab "Chủ sân" trong UI tab selector
   - Hidden field `Role` fixed = "player"
   - Xóa Owner warning alert section

6. `Views/Shared/_DemoAccountsPanel.cshtml`:
   - Xóa button "Chủ sân demo" (L:30-41)

7. `Views/Shared/_Layout.cshtml`:
   - Xóa `@if (User.IsInRole("Owner"))` blocks (L:54-57 desktop, L:145-148 mobile)
   - Xóa `else if` anonymous "Dành cho chủ sân" link (L:58-61 desktop, L:149-152 mobile)
   - Xóa case `"Owner" => "Chủ cụm sân"` (L:79)

8. `Views/Shared/_Footer.cshtml`:
   - Xóa link "Đăng ký chủ sân" (L:34) và "Quản lý cụm sân" (L:35)

9. `Views/Home/Index.cshtml`:
   - Xóa section "For Owners" (L:114-145+)
   - Xóa link Owner/Dashboard (L:28-29)

10. `Controllers/AccountController.cs`:
    - `RedirectAfterAuthentication()`: xóa case `AppRoles.Owner` (L:265)

11. `Services/AccountService.cs`:
    - `RegisterAsync()`: xóa `role == AppRoles.Owner` logic (L:82-83, L:96-98)
    - `UpdateProfileAsync()`: xóa block sync venue OwnerName/OwnerPhone (L:176-185)

12. `Data/DemoDataConstants.cs`:
    - Xóa tất cả `DemoOwner*` constants (L:10-15)

**Build check:** `dotnet build` phải pass. App chạy được nhưng Owner login sẽ bị redirect về Home (không còn Owner option trên UI).

---

### Phase 3: Sửa database entities và DbContext

1. `Data/Entities/AppUserEntity.cs`:
   - Xóa property `OwnedVenues` (L:24)

2. `Data/Entities/VenueEntity.cs`:
   - Xóa `OwnerUserId` (L:12)
   - Xóa `OwnerUser` (L:24)
   - Rename `OwnerName` → `ContactName` (L:13)
   - Rename `OwnerPhone` → `ContactPhone` (L:14)
   - Đổi default `Status` từ `VenueStatus.PendingApproval` → `VenueStatus.Approved` (L:21)

3. `Data/Entities/UserSnapshotEntity.cs`:
   - **Xóa file**

4. `Data/ApplicationDbContext.cs`:
   - Xóa `public DbSet<UserSnapshotEntity> UserSnapshots` (L:12)
   - Xóa toàn bộ `modelBuilder.Entity<UserSnapshotEntity>()` block (L:85-91)
   - Xóa Venue → OwnerUser relationship mapping (L:51-54)
   - Đổi `entity.Property(x => x.OwnerName)` → `entity.Property(x => x.ContactName)` (L:42)
   - Đổi `entity.Property(x => x.OwnerPhone)` → `entity.Property(x => x.ContactPhone)` (L:43)

**Build check:** `dotnet build` phải pass. Lúc này app sẽ **lỗi runtime** nếu chạy vì DB chưa migrate – đó là expected.

---

### Phase 4: Tạo migration scale-down

1. Tạo migration:
   ```bash
   dotnet ef migrations add ScaleDownToAdminPlayerRoles --project BadmintonCourtBooking
   ```

2. **Review migration file thủ công.** EF sẽ tự generate:
   - Drop FK `FK_Venues_Users_OwnerUserId`
   - Drop index `IX_Venues_OwnerUserId`
   - Drop column `Venues.OwnerUserId`
   - Rename column `Venues.OwnerName` → `Venues.ContactName`
   - Rename column `Venues.OwnerPhone` → `Venues.ContactPhone`
   - Drop table `UserSnapshots`

3. **Thêm data migration SQL** vào `Up()` method – ĐẶT Ở ĐẦU, trước schema changes:
   ```csharp
   // Data migration
   migrationBuilder.Sql("UPDATE [Users] SET [Role] = 'Player' WHERE [Role] = 'Owner'");
   migrationBuilder.Sql("UPDATE [Venues] SET [Status] = 'Approved' WHERE [Status] = 'PendingApproval'");
   ```

4. **Kiểm tra Down()** method – đảm bảo có rollback cho rename column.

5. Apply migration:
   ```bash
   dotnet ef database update --project BadmintonCourtBooking
   ```

**Build check:** Migration pass, database schema updated.

---

### Phase 5: Chuyển logic Owner sang Admin

**Thứ tự quan trọng – sửa từ service lên controller:**

1. `Services/IAdminDashboardService.cs` – thêm methods:
   ```csharp
   // Booking management (port từ IOwnerDashboardService)
   Task<OperationResult> ApproveBookingAsync(string id, CancellationToken cancellationToken = default);
   Task<OperationResult> RejectBookingAsync(string id, CancellationToken cancellationToken = default);

   // Venue management (port từ IOwnerDashboardService, đổi scope từ owner sang admin)
   Task<AdminVenueManagementViewModel> GetVenueManagementAsync(string? selectedVenueId = null, CancellationToken cancellationToken = default);
   Task<OperationResult<string>> CreateVenueAsync(AdminVenueInputModel model, CancellationToken cancellationToken = default);
   Task<OperationResult<string>> UpdateVenueAsync(AdminVenueInputModel model, CancellationToken cancellationToken = default);
   Task<OperationResult<string>> CreateCourtAsync(AdminCourtInputModel model, CancellationToken cancellationToken = default);
   Task<OperationResult<string>> UpdateCourtAsync(AdminCourtInputModel model, CancellationToken cancellationToken = default);
   ```

2. `Models/AdminManagementViewModels.cs` – thêm viewmodels:
   - `AdminVenueManagementViewModel` (tương tự `OwnerVenueManagementViewModel` nhưng không filter theo owner)
   - `AdminVenueInputModel` (tương tự `OwnerVenueInputModel`)
   - `AdminCourtInputModel` (tương tự `OwnerCourtInputModel`)
   - `AdminVenueViewModel` (tương tự `OwnerVenueViewModel`)
   - `AdminCourtViewModel` (tương tự `OwnerCourtViewModel`)
   - Xóa `TotalOwners`, `OwnedVenueCount`, `AdminPendingVenueViewModel`
   - Đổi `OwnerName` → `ContactName` trong các existing VMs

3. `Services/AdminDashboardService.cs` – implement methods:
   - **Approve/Reject Booking:** Port logic từ `OwnerDashboardService.ApproveBookingAsync()` và `RejectBookingAsync()`. **Khác biệt:** Admin không cần check `OwnerUserId` – admin có quyền xử lý tất cả booking.
   - **Venue CRUD:** Port logic từ `OwnerDashboardService.CreateVenueAsync()`, `UpdateVenueAsync()`, `CreateCourtAsync()`, `UpdateCourtAsync()`. **Khác biệt:** Không filter theo `OwnerUserId`. Admin thêm venue trực tiếp với status `Approved`. Không set `OwnerUserId`. Dùng `ContactName`/`ContactPhone`.
   - Cập nhật `GetDashboardAsync()`: xóa `TotalOwners`, xóa `venuesByOwner`, đổi `OwnerName`→`ContactName`, đổi `OwnerPhone`→`ContactPhone`.
   - Đổi message "Chủ sân có thể cập nhật lại hồ sơ" thành message phù hợp Admin.

4. `Controllers/AdminController.cs` – thêm actions:
   ```csharp
   // Booking
   [HttpPost] ApproveBooking(string id, ...)
   [HttpPost] RejectBooking(string id, ...)

   // Venue management
   [HttpGet] Venues(string? selectedVenueId, ...)
   [HttpPost] CreateVenue(AdminVenueInputModel model, ...)
   [HttpPost] UpdateVenue(AdminVenueInputModel model, ...)
   [HttpPost] CreateCourt(AdminCourtInputModel model, ...)
   [HttpPost] UpdateCourt(AdminCourtInputModel model, ...)
   ```

5. Tạo `Views/Admin/Venues.cshtml`:
   - Port UI từ `Views/Owner/Venues.cshtml`
   - Đổi controller references từ `Owner` → `Admin`
   - Xóa "Chờ duyệt" status UI
   - Đổi `OwnerName` → `AdminName` hoặc tên chung

**Build check:** `dotnet build` pass.

---

### Phase 6: Cập nhật các service/view còn lại

1. `Services/VenueCatalogService.cs`:
   - `MapVenue()` L:144: đổi `record.OwnerPhone` → `record.ContactPhone`

2. `Models/Venue.cs`:
   - Đổi `OwnerPhone` → `ContactPhone` (L:10)

3. `Views/Venue/Detail.cshtml`:
   - Đổi `Model.Venue.OwnerPhone` → `Model.Venue.ContactPhone` (L:54)

4. `Views/Admin/Index.cshtml`:
   - Xóa "chủ sân" text references
   - Đổi `@venue.OwnerName` → `@venue.ContactName`
   - Đổi `@venue.OwnerPhone` → `@venue.ContactPhone`
   - Xóa `@booking.OwnerName` column trong bảng booking
   - Xóa `@Model.TotalOwners` references
   - Thêm link tới `Admin/Venues` nếu cần
   - Thêm button approve/reject booking vào booking table

5. `Services/BookingService.cs`:
   - Đổi message L:108: "Chủ sân sẽ xác nhận" → "Quản trị viên sẽ xác nhận"

**Build check:** `dotnet build` pass.

---

### Phase 7: Gỡ Owner khỏi code

**Chỉ thực hiện sau khi Phase 5-6 hoàn tất và build pass.**

1. Xóa `Controllers/OwnerController.cs`
2. Xóa `Services/OwnerDashboardService.cs`
3. Xóa `Services/IOwnerDashboardService.cs`
4. Xóa `Models/OwnerManagementViewModels.cs`
5. Xóa `Models/DashboardViewModel.cs`
6. Xóa `Models/AdminViewModel.cs`
7. Xóa thư mục `Views/Owner/` (Dashboard.cshtml, Venues.cshtml)
8. Xóa `wwwroot/js/owner-dashboard.js`
9. Xóa class `OwnerBooking` trong `Models/Booking.cs`
10. `Program.cs`: xóa `AddScoped<IOwnerDashboardService, OwnerDashboardService>()` (L:34)
11. `Models/MockData.cs`:
    - Xóa `OwnerBookings` list
    - Xóa `PendingVenues` list
    - Xóa `RecentUsers` list (hoặc cập nhật không còn "Chủ sân")
    - Xóa `GetDashboardViewModel()`
    - Xóa `GetAdminViewModel()`
    - Xóa `PendingVenue` class nếu không còn reference

**Build check:** `dotnet build` pass. Không còn reference đến Owner.

---

### Phase 8: Seeder/demo data

1. `Data/DemoDataConstants.cs`:
   - Xóa hoàn toàn `DemoOwner*` constants (đã xóa ở Phase 2, kiểm tra lại)

2. `Data/ApplicationDbContextSeeder.cs`:
   - `BuildSeedAccounts()`:
     - Xóa Owner demo account entry (L:390-399)
     - Chuyển các owner user cần giữ (vì có booking) sang `AppRoles.Player`:
       - `user-owner-vu`, `user-owner-hai`, `user-owner-hung`, `user-owner-mai`, `user-owner-viet` → có thể **xóa** vì không có booking liên kết trực tiếp (OwnerUserId bị drop, venue không cần owner nữa)
       - `user-owner-linh`, `user-owner-tuan-anh` → **chuyển sang Player** vì `ResolveBookingUserId()` trỏ đến họ
       - `user-player-bao`, `user-player-nhung` → giữ nguyên (đã là Player)
   - Xóa `SyncVenueOwnersAsync()` hoàn toàn
   - Xóa `SeedUserSnapshotsAsync()` hoàn toàn
   - Xóa `SeedAsync()` call đến 2 method trên
   - Xóa `MapPendingVenue()` – không seed pending venue nữa
   - Xóa `MockData.PendingVenues` reference trong `SeedVenuesAsync()`
   - Cập nhật `ResolveOwner()` → rename thành `ResolveContact()`, return `(contactName, contactPhone)`
   - Xóa `ResolveOwnerUserId()` hoàn toàn
   - Cập nhật `MapApprovedVenue()`:
     - Xóa `OwnerUserId = ResolveOwnerUserId(venue.Id)`
     - Đổi `OwnerName = ownerName` → `ContactName = contactName`
     - Đổi `OwnerPhone = ownerPhone` → `ContactPhone = contactPhone`
   - Cập nhật booking seed:
     - `MockData.OwnerBookings` reference cần xử lý:
       - Nếu đã xóa `OwnerBookings` → xóa toàn bộ block seed OwnerBookings (L:169-188)
       - Hoặc chuyển thành booking bình thường (tạo mock booking cho player demo)
     - Đổi `CancelReason = "Từ chối bởi chủ sân"` → `"Từ chối bởi quản trị viên"` (L:186)
   - Xóa `DemoDataConstants.DemoOwnerVenueId` reference
   - Xóa `DemoDataConstants.DemoOwnerUserId` reference

3. `Models/MockData.cs`:
   - Xóa `OwnerBookings` list
   - Xóa `PendingVenues` list và `PendingVenue` class
   - Cập nhật `RecentUsers` – xóa "Chủ sân" role references
   - Xóa `GetDashboardViewModel()`
   - Xóa `GetAdminViewModel()`

**Test:** Xóa database local → chạy app → seed phải pass → app hoạt động.

---

### Phase 9: Test manual

| # | Test case | Mô tả | Expected |
|---|-----------|-------|----------|
| 1 | Anonymous xem sân | Mở `/Venue` | Hiển thị danh sách venue approved |
| 2 | Anonymous xem chi tiết | Mở `/Venue/Detail/v1` | Hiển thị chi tiết + slot matrix + số liên hệ (ContactPhone) |
| 3 | Player đăng ký | Tạo tài khoản mới | Chỉ có 1 tab "Người chơi", đăng ký thành công |
| 4 | Player đăng nhập | Login với demo account | Đăng nhập thành công, redirect về Home |
| 5 | Player đặt sân | Chọn venue → slot → đặt | Booking tạo thành công, status Pending |
| 6 | Player xem lịch | Mở `/Booking` | Hiển thị booking vừa đặt |
| 7 | Player hủy sân | Hủy booking | Status chuyển Cancelled |
| 8 | Admin đăng nhập | Login admin demo | Redirect về `/Admin` |
| 9 | Admin xem dashboard | Mở `/Admin` | Hiển thị tổng quan (không còn "chủ sân") |
| 10 | Admin approve booking | Approve booking pending | Status chuyển Confirmed |
| 11 | Admin reject booking | Reject booking pending | Status chuyển Cancelled |
| 12 | Admin quản lý venue | Mở `/Admin/Venues` | Hiển thị danh sách venue, có thể edit |
| 13 | Admin tạo venue | Tạo venue mới | Venue tạo thành công, status Approved |
| 14 | Admin tạo court | Thêm court vào venue | Court thêm thành công |
| 15 | Không còn Owner | Kiểm tra UI | Không còn link, tab, menu Owner |
| 16 | Fresh DB | Xóa DB → chạy lại app | Migration + seed pass, app hoạt động |
| 17 | Existing DB | Migrate DB cũ lên | Migration pass, dữ liệu giữ nguyên |

---

## 10. Acceptance criteria

- [ ] `dotnet build` pass – không lỗi compile
- [ ] Migration `ScaleDownToAdminPlayerRoles` chạy pass
- [ ] Database tạo mới từ đầu (delete + migrate) pass
- [ ] Database cũ migrate lên pass (apply migration trên DB hiện tại)
- [ ] App chạy không lỗi runtime
- [ ] Không còn link "Dành cho chủ sân" trên navigation
- [ ] Không còn tab "Chủ sân" trên trang đăng ký/đăng nhập
- [ ] Không còn button "Chủ sân demo" trên demo panel
- [ ] Không còn Owner demo account trong constants
- [ ] Không còn route `/Owner/*` hoạt động (404 hoặc redirect)
- [ ] Player booking vẫn hoạt động đầy đủ (đặt, xem, hủy)
- [ ] Admin đăng nhập vào dashboard thành công
- [ ] Admin approve/reject booking pending thành công
- [ ] Admin quản lý venue/court (xem, tạo, sửa) thành công
- [ ] Admin approve/reject venue (công khai/ẩn) thành công
- [ ] Seeder chạy không lỗi khi DB trống
- [ ] Seeder không lỗi khi DB đã có dữ liệu
- [ ] Không còn reference `AppRoles.Owner` trong code (grep verify)
- [ ] Không còn `OwnerUserId` trong DB schema
- [ ] Không còn bảng `UserSnapshots` trong DB

---

## 11. Risk list

| # | Rủi ro | Mức độ | Giải pháp |
|---|--------|--------|-----------|
| 1 | Drop `OwnerUserId` làm lỗi query còn `.Include(x => x.OwnerUser)` | **Cao** | Grep toàn bộ codebase cho `OwnerUser`, `OwnerUserId`, `OwnedVenues` trước khi tạo migration |
| 2 | Rename `OwnerName`/`OwnerPhone` làm vỡ view đang hiển thị SĐT sân | **Trung bình** | Sửa tất cả view reference trước khi migration. Grep `OwnerName`, `OwnerPhone` |
| 3 | Xóa Owner users làm mất liên kết booking | **Trung bình** | Không xóa – chuyển Role sang Player thay vì delete |
| 4 | Seeder còn reference Owner constants đã xóa | **Cao** | Sửa seeder cùng lúc với xóa constants. Build check sau mỗi bước. |
| 5 | View còn `User.IsInRole("Owner")` | **Trung bình** | Grep toàn bộ Views folder. Đã phát hiện: _Layout.cshtml, _Footer.cshtml |
| 6 | Admin chưa có action thay Owner approve/reject booking | **Cao** | Phase 5 phải hoàn tất trước khi xóa OwnerController |
| 7 | Migration làm mất dữ liệu nếu không backup | **Cao** | Bắt buộc backup trước Phase 4. OwnerUserId values mất vĩnh viễn. |
| 8 | `PendingApproval` status còn tồn tại nhưng UI không xử lý | **Thấp** | Migration auto-update PendingApproval → Approved. Giữ enum value để backward compat. |
| 9 | `MockData.OwnerBookings` references trong seeder gây crash | **Cao** | Xóa hoặc refactor MockData trước khi xóa OwnerBooking class |
| 10 | `BookingService.CreateBookingAsync` message nói "Chủ sân" | **Thấp** | Đổi text. Không ảnh hưởng logic. |
| 11 | `owner-dashboard.js` vẫn load trên layout | **Thấp** | File JS chỉ load nếu Owner/Dashboard view có `@section Scripts`. Xóa file sau khi xóa view. |
| 12 | Admin `RejectVenueAsync()` message nói "Chủ sân có thể cập nhật lại" | **Thấp** | Đổi text message trong AdminDashboardService. |
| 13 | Login/Register view file rất lớn (~47KB), dễ sai khi sửa | **Trung bình** | Cẩn thận grep "owner" trong file, chỉ xóa các block rõ ràng. Test UI sau sửa. |
| 14 | `ApplicationDbContextSeeder.SyncVenueOwnersAsync()` crash vì column không tồn tại | **Cao** | Xóa method này TRƯỚC khi tạo migration. |
| 15 | AccountService `UpdateProfileAsync` sync OwnerName trên venue – crash vì column đổi tên | **Cao** | Xóa block này ở Phase 2. |

---

## 12. Recommended final architecture

```
Controllers/
├── HomeController.cs          (giữ nguyên)
├── VenueController.cs         (giữ nguyên, đổi OwnerPhone → ContactPhone)
├── BookingController.cs       (giữ nguyên)
├── AccountController.cs       (sửa nhẹ – xóa Owner redirect)
└── AdminController.cs         (mở rộng – thêm venue/court/booking management)

Services/
├── AccountService.cs          (sửa nhẹ – xóa Owner logic)
├── IAccountService.cs         (giữ nguyên)
├── CurrentUserService.cs      (giữ nguyên)
├── ICurrentUserService.cs     (giữ nguyên)
├── RequireActiveUserFilter.cs (giữ nguyên)
├── VenueCatalogService.cs     (sửa nhẹ – đổi field name)
├── IVenueCatalogService.cs    (giữ nguyên)
├── BookingService.cs          (sửa nhẹ – đổi message)
├── IBookingService.cs         (giữ nguyên)
├── AdminDashboardService.cs   (mở rộng – thêm venue/court/booking management)
├── IAdminDashboardService.cs  (mở rộng – thêm methods)
├── PresentationFormatter.cs   (giữ nguyên)
├── AccountValueNormalizer.cs  (giữ nguyên)
└── OperationResult.cs         (giữ nguyên)

Data/Entities/
├── AppUserEntity.cs           (xóa OwnedVenues navigation)
├── VenueEntity.cs             (xóa OwnerUserId/OwnerUser, rename fields)
├── CourtEntity.cs             (giữ nguyên)
└── BookingEntity.cs           (giữ nguyên)

Data/
├── ApplicationDbContext.cs    (xóa UserSnapshots, sửa Venue mapping)
├── ApplicationDbContextFactory.cs (giữ nguyên)
├── ApplicationDbContextSeeder.cs  (sửa nặng – xóa owner logic)
└── DemoDataConstants.cs       (xóa DemoOwner constants)

Models/
├── AppRoles.cs                (xóa Owner)
├── AccountViewModel.cs        (xóa Owner option)
├── AdminManagementViewModels.cs (mở rộng, xóa Owner references)
├── Booking.cs                 (xóa OwnerBooking class)
├── Court.cs                   (giữ nguyên)
├── CreateBookingInputModel.cs (giữ nguyên)
├── ErrorViewModel.cs          (giữ nguyên)
├── MockData.cs                (xóa owner-related data)
├── Venue.cs                   (đổi OwnerPhone → ContactPhone)
├── VenueDetailViewModel.cs    (giữ nguyên)
├── VenueIndexViewModel.cs     (giữ nguyên)
└── VenueStatus.cs             (giữ enum, deprecate PendingApproval)

Views/
├── Account/
│   ├── Login.cshtml           (xóa tab Owner)
│   ├── Register.cshtml        (xóa tab Owner)
│   ├── Profile.cshtml         (giữ nguyên)
│   └── AccessDenied.cshtml    (giữ nguyên)
├── Admin/
│   ├── Index.cshtml           (sửa – xóa Owner refs, thêm booking actions)
│   └── Venues.cshtml          (MỚI – port từ Owner/Venues)
├── Booking/
│   └── Index.cshtml           (giữ nguyên)
├── Home/
│   └── Index.cshtml           (xóa section Owner)
├── Shared/
│   ├── _Layout.cshtml         (xóa Owner nav links)
│   ├── _DemoAccountsPanel.cshtml (xóa Owner demo button)
│   ├── _Footer.cshtml         (xóa Owner links)
│   └── ... (giữ nguyên các partial còn lại)
└── Venue/
    ├── Index.cshtml           (giữ nguyên)
    └── Detail.cshtml          (đổi OwnerPhone → ContactPhone)

wwwroot/js/
├── site.js                    (kiểm tra, có thể sửa nhẹ)
├── venue-booking.js           (giữ nguyên)
└── venue-filter.js            (giữ nguyên)
(XÓA: owner-dashboard.js)

Roles:
├── Admin
└── Player

Database Tables:
├── Users          (Role chỉ còn Admin/Player)
├── Venues         (không còn OwnerUserId, đổi tên ContactName/ContactPhone)
├── Courts         (giữ nguyên)
└── Bookings       (giữ nguyên)
(XÓA: UserSnapshots)
```

---

## 13. Prompt tiếp theo để implement

Sau khi kế hoạch được duyệt, sử dụng prompt sau để bắt đầu implement từng phase:

```
Bạn là Senior ASP.NET Core MVC Developer. Hãy implement scale-down plan theo file `agent/scale-down-plan.md`.

Quy tắc:
- Implement từng phase theo thứ tự (Phase 1 → Phase 9).
- Sau mỗi phase phải `dotnet build` để verify.
- Không skip phase.
- Nếu build lỗi ở phase nào, sửa ngay tại phase đó trước khi chuyển phase tiếp.
- Chỉ xóa file Owner (Phase 7) SAU KHI Admin đã có đủ chức năng thay thế (Phase 5-6).
- Tạo migration ở Phase 4, KHÔNG tạo sớm hơn.
- Sau Phase 8, xóa DB local và chạy lại app để verify fresh database.

Bắt đầu với Phase [số phase]. Cho tôi biết khi hoàn tất mỗi phase.
```

---

> **Lưu ý cuối:** Kế hoạch này được viết với mục tiêu **phù hợp đồ án sinh viên**: demo ổn, ít bug, dễ giải thích trong vấn đáp. Không over-engineer. Ưu tiên giữ project chạy được sau mỗi phase.
