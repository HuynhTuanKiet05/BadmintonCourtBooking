# Kế hoạch implement chi tiết từng Phase

> **Tham chiếu:** [scale-down-plan.md](file:///d:/DoAN_LTW/BadmintonCourtBooking/agent/scale-down-plan.md)
> **Nguyên tắc:** Sau mỗi phase → `dotnet build` phải pass. Không skip phase. Không xóa file Owner trước khi Admin có đủ chức năng thay thế.

---

## Phase 1: Tạo branch và baseline

### Mục tiêu
Đảm bảo project hiện tại build và chạy ổn định trước khi sửa bất kỳ thứ gì.

### Việc cần làm

| # | Việc | Lệnh / Thao tác |
|---|------|-----------------|
| 1 | Tạo branch | `git checkout -b scale-down-2-roles` |
| 2 | Build project | `dotnet build` tại thư mục `BadmintonCourtBooking/` |
| 3 | Chạy app | `dotnet run` → mở browser `https://localhost:xxxx` |
| 4 | Kiểm tra baseline | Login thử Player demo, Owner demo, Admin demo – đều hoạt động |
| 5 | Ghi nhận migrations | Chạy `dotnet ef migrations list` → phải có `InitialCreate`, `AddAuthenticationAndUsers` |

### Cách test

```
✅ dotnet build → Build succeeded, 0 Error(s)
✅ dotnet run → App khởi động, seed data chạy không lỗi
✅ Mở browser → trang chủ hiển thị bình thường
✅ Login Player demo (player@courtbook.local / Player@123) → vào Home
✅ Login Owner demo (owner@courtbook.local / Owner@123) → vào Owner/Dashboard
✅ Login Admin demo (admin@courtbook.local / Admin@123) → vào Admin/Index
```

### Kết quả mong đợi
Project build pass, app chạy ổn, tất cả 3 role đều login được. Đây là baseline để so sánh sau khi sửa.

---

## Phase 2: Sửa role ở tầng app (code-only, CHƯA đụng DB)

### Mục tiêu
Xóa toàn bộ reference đến Owner role trên UI và controller/service logic. **KHÔNG sửa entity, DbContext, hay tạo migration** ở phase này.

### Ngữ cảnh quan trọng
- Phase này chỉ sửa **code tầng trên** (models, views, controllers, services).
- Database vẫn giữ nguyên schema cũ (OwnerUserId, OwnerName, OwnerPhone vẫn tồn tại).
- OwnerController, OwnerDashboardService **chưa xóa** ở phase này (xóa ở Phase 7).
- Mục đích: loại bỏ Owner khỏi tầm nhìn của user, nhưng code Owner vẫn compile được.

### Việc cần làm (theo thứ tự)

#### 2.1 — `Models/AppRoles.cs`
**Hành động:** Xóa Owner constant và tất cả case Owner trong 3 method helper.

**Cụ thể:**
- Xóa dòng `public const string Owner = "Owner";` (L:6)
- Xóa case `"owner" => Owner` trong `FromSelection()` (L:11)
- Xóa case `Owner => "owner"` trong `ToSelection()` (L:18)
- Xóa case `Owner => "Chủ sân"` trong `ToDisplayLabel()` (L:25)

**Lưu ý:** Sau khi xóa, `OwnerController.cs` (L:9) dùng `[Authorize(Roles = AppRoles.Owner)]` sẽ **lỗi compile**. Phải sửa tạm thành `[Authorize(Roles = "Owner")]` string literal để build pass. Controller này sẽ bị xóa hoàn toàn ở Phase 7.

**Tương tự:** Các file sau cũng reference `AppRoles.Owner`:
- `AdminDashboardService.cs` L:65 `user.Role == AppRoles.Owner` → đổi thành `user.Role == "Owner"` tạm
- `AdminManagementViewModels.cs` L:55 `AppRoles.Owner` → đổi thành `"Owner"` tạm
- `AdminManagementViewModels.cs` L:63 `Role == AppRoles.Owner` → đổi thành `Role == "Owner"` tạm

Các file trên sẽ được refactor đúng cách ở Phase 5-6, ở đây chỉ cần build pass.

#### 2.2 — `Models/AccountViewModel.cs`
**Hành động:** Chặn không cho chọn/gửi role `owner` khi login/register.

**Cụ thể:**
- `LoginViewModel` (L:22): Đổi regex `"^(player|owner)$"` → `"^(player)$"`
- `LoginViewModel` (L:23): Giữ default `Role = "player"`
- `RegisterViewModel` (L:61): Đổi regex `"^(player|owner)$"` → `"^(player)$"`
- `RegisterViewModel` (L:62): Giữ default `Role = "player"`

#### 2.3 — `Views/Account/Login.cshtml`
**Hành động:** Xóa tab "Chủ sân" trong giao diện login.

**Cụ thể:**
- Tìm block HTML chứa tab "Chủ sân" / "owner" tab button và xóa nó.
- Tìm panel/form dành riêng cho Owner login (nếu có) và xóa.
- Hidden input `Role` phải fixed value `"player"`.
- Xóa block "Owner Warning Alert" (tìm comment `<!-- Owner Warning Alert -->` ở L:590).
- **Quan trọng:** File rất lớn (46KB, ~700 dòng). Cần cẩn thận chỉ xóa đúng block, không xóa nhầm block Player hay Admin.

#### 2.4 — `Views/Account/Register.cshtml`
**Hành động:** Xóa tab "Chủ sân" trong giao diện đăng ký.

**Cụ thể:**
- Tương tự Login – xóa tab Owner, xóa Owner Warning Alert (L:593).
- Hidden input `Role` phải fixed value `"player"`.
- Chỉ giữ 1 form đăng ký duy nhất cho Player.

#### 2.5 — `Views/Shared/_DemoAccountsPanel.cshtml`
**Hành động:** Xóa nút "Chủ sân demo".

**Cụ thể:**
- Xóa toàn bộ `<button>` block từ L:30 đến L:41 (button có `data-demo-role="owner"`).
- Chỉ giữ lại 2 button: "Người chơi demo" và "Quản trị viên demo".
- Sửa text mô tả L:8: bỏ "owner" khỏi danh sách → `"Dùng để chạy demo player và admin..."`.

#### 2.6 — `Views/Shared/_Layout.cshtml`
**Hành động:** Xóa link "Dành cho chủ sân" trên navigation.

**Cụ thể:**
- **Desktop nav** (L:54-61): Xóa toàn bộ block:
  ```razor
  @if (User.IsInRole("Owner"))
  {
      <a asp-controller="Owner" ...>Dành cho chủ sân</a>
  }
  else if (!(User.Identity?.IsAuthenticated ?? false))
  {
      <a asp-controller="Account" asp-action="Login" asp-route-role="owner" ...>Dành cho chủ sân</a>
  }
  ```
- **Mobile nav** (L:145-152): Xóa tương tự block `@if (User.IsInRole("Owner"))` và `else if`.
- **Role label** (L:79): Xóa `"Owner" => "Chủ cụm sân"` trong switch expression.

#### 2.7 — `Views/Shared/_Footer.cshtml`
**Hành động:** Xóa column "Chủ sân" trong footer.

**Cụ thể:**
- Xóa toàn bộ block từ L:31 đến L:37 (Column 3: Chủ sân), bao gồm:
  ```html
  <div>
      <h4 class="footer-heading">Chủ sân</h4>
      <a asp-controller="Owner" ...>Đăng ký chủ sân</a>
      <a asp-controller="Owner" ...>Quản lý cụm sân</a>
      <a href="#" ...>Bảng giá dịch vụ</a>
  </div>
  ```

#### 2.8 — `Views/Home/Index.cshtml`
**Hành động:** Xóa section "Dành cho chủ sân" và link Owner trên trang chủ.

**Cụ thể:**
- Xóa link "Đăng ký làm chủ sân" trong hero CTA (L:28-30):
  ```html
  <a asp-controller="Owner" asp-action="Dashboard" class="btn btn-outline btn-lg" ...>
      Đăng ký làm chủ sân
  </a>
  ```
- Xóa toàn bộ section "For Owners" (L:114-174):
  ```html
  <!-- For Owners Section -->
  <section class="owner-section"> ... </section>
  ```
- Sửa text hero (L:22): bỏ "giúp chủ sân tiếp cận khách hàng" → text phù hợp chỉ player.

#### 2.9 — `Controllers/AccountController.cs`
**Hành động:** Xóa redirect Owner sau login.

**Cụ thể:**
- Method `RedirectAfterAuthentication()` (L:255-269): Xóa case `AppRoles.Owner`:
  ```csharp
  // XÓA dòng này:
  AppRoles.Owner => RedirectToAction("Dashboard", "Owner"),
  ```
  Lưu ý: Vì `AppRoles.Owner` đã bị xóa ở 2.1, dòng này dùng string literal `"Owner"` tạm.
  Sau Phase 2 hoàn tất, Owner login sẽ redirect về Home (fall-through `_ => RedirectToAction("Index", "Home")`).

#### 2.10 — `Services/AccountService.cs`
**Hành động:** Xóa logic liên quan Owner trong register và update profile.

**Cụ thể:**
- `RegisterAsync()` L:82-83: Xóa dòng `PlayArea = role == AppRoles.Owner ? "TP.HCM" : "Quận 11, TP.HCM"` → thay bằng `PlayArea = "Quận 11, TP.HCM"`
- `RegisterAsync()` L:96-98: Xóa message chủ sân:
  ```csharp
  // XÓA ternary và chỉ giữ:
  return OperationResult<AppUserEntity>.Success(user, "Tạo tài khoản thành công! Chào mừng bạn đến với CourtBook.");
  ```
- `UpdateProfileAsync()` L:176-185: Xóa toàn bộ block sync OwnerName/OwnerPhone lên Venue:
  ```csharp
  // XÓA:
  if (user.Role == AppRoles.Owner)
  {
      var venues = await _context.Venues.Where(item => item.OwnerUserId == userId).ToListAsync(...);
      foreach (var venue in venues) { ... }
  }
  ```

#### 2.11 — `Data/DemoDataConstants.cs`
**Hành động:** Xóa constants cho Owner demo.

**Cụ thể:**
- Xóa 6 dòng (L:10-15):
  ```csharp
  public const string DemoOwnerUserId = "user-owner-demo";
  public const string DemoOwnerName = "Anh Hoàng Nam";
  public const string DemoOwnerEmail = "owner@courtbook.local";
  public const string DemoOwnerPhone = "0905123456";
  public const string DemoOwnerPassword = "Owner@123";
  public const string DemoOwnerVenueId = "v1";
  ```
- **Lưu ý:** Seeder (`ApplicationDbContextSeeder.cs`) reference các constant này. Seeder sẽ **lỗi compile** sau khi xóa. Để build pass ở Phase 2, có 2 cách:
  - **Cách A (khuyến nghị):** Chuyển các constant sang string literal tạm trong seeder (hardcode giá trị). Seeder sẽ được refactor hoàn toàn ở Phase 8.
  - **Cách B:** Giữ lại constants ở Phase 2, chỉ xóa ở Phase 8 khi refactor seeder.

  **Khuyến nghị chọn Cách B** để giảm rủi ro compile error lan.

### Cách test Phase 2

```
✅ dotnet build → Build succeeded, 0 Error(s)
✅ dotnet run → App khởi động, seed không lỗi
✅ Mở / → Không còn section "Dành cho chủ sân" trên trang chủ
✅ Mở / → Không còn nút "Đăng ký làm chủ sân" trên hero
✅ Navigation desktop → Không còn link "Dành cho chủ sân"
✅ Navigation mobile → Không còn link "Dành cho chủ sân"
✅ Footer → Không còn column "Chủ sân"
✅ /Account/Login → Chỉ còn tab Player (không có tab Chủ sân)
✅ /Account/Register → Chỉ còn tab Player (không có tab Chủ sân)
✅ /Account/Login → Panel demo chỉ còn 2 button: Player + Admin
✅ Login Player demo → Redirect về Home (pass)
✅ Login Admin demo → Redirect về /Admin (pass)
✅ Đăng ký tài khoản mới → Chỉ có role Player, đăng ký thành công
✅ /Owner/Dashboard truy cập trực tiếp bằng URL → Vẫn hoạt động (chưa xóa, sẽ xóa Phase 7)
✅ Admin dashboard → Vẫn hiển thị (có thể còn text "chủ sân", sẽ sửa Phase 5-6)
```

---

## Phase 3: Sửa database entities và DbContext

### Mục tiêu
Cập nhật entity classes và DbContext mapping để phản ánh schema mới (không còn OwnerUserId, rename OwnerName/OwnerPhone, xóa UserSnapshots). **Chưa tạo migration**, chỉ sửa code C#.

### Ngữ cảnh quan trọng
- Sau phase này, app sẽ **KHÔNG chạy được** (runtime error) vì DB schema chưa khớp entity. Đây là expected.
- Chỉ cần `dotnet build` pass, không cần `dotnet run`.
- Phase 4 sẽ tạo migration để đồng bộ DB.

### Việc cần làm

#### 3.1 — `Data/Entities/AppUserEntity.cs`
- Xóa navigation property (L:24):
  ```csharp
  // XÓA:
  public ICollection<VenueEntity> OwnedVenues { get; set; } = new List<VenueEntity>();
  ```

#### 3.2 — `Data/Entities/VenueEntity.cs`
- Xóa `OwnerUserId` (L:12): `public string? OwnerUserId { get; set; }`
- Rename `OwnerName` (L:13) → `ContactName`
- Rename `OwnerPhone` (L:14) → `ContactPhone`
- Xóa navigation property `OwnerUser` (L:24): `public AppUserEntity? OwnerUser { get; set; }`
- Đổi default Status (L:21): `VenueStatus.PendingApproval` → `VenueStatus.Approved`

**Lưu ý:** Sau khi rename, tất cả file reference `OwnerName`, `OwnerPhone` trên VenueEntity sẽ lỗi compile. Các file cần sửa đồng thời:
- `ApplicationDbContextSeeder.cs` – reference `OwnerName`, `OwnerPhone`, `OwnerUserId`, `OwnerUser`
- `AdminDashboardService.cs` – reference `venue.OwnerName`, `venue.OwnerPhone`, `venue.OwnerUserId`
- `OwnerDashboardService.cs` – reference `OwnerUserId`, `OwnerName`, `OwnerPhone`
- `VenueCatalogService.cs` – reference `record.OwnerPhone`
- `Models/Venue.cs` – property `OwnerPhone`

**Chiến lược để build pass:** Sửa đồng thời tất cả reference trong Phase 3. Cụ thể:
- `VenueCatalogService.cs` L:144: `record.OwnerPhone` → `record.ContactPhone`
- `Models/Venue.cs` L:10: `OwnerPhone` → `ContactPhone`
- `AdminDashboardService.cs`: tất cả `venue.OwnerName` → `venue.ContactName`, `venue.OwnerPhone` → `venue.ContactPhone`, xóa `venue.OwnerUserId` references
- `OwnerDashboardService.cs`: tương tự đổi tên + xóa `OwnerUserId` references (file này sẽ bị xóa ở Phase 7, nhưng phải compile ở Phase 3)
- `ApplicationDbContextSeeder.cs`: đổi `OwnerName` → `ContactName`, `OwnerPhone` → `ContactPhone`, xóa `OwnerUserId` assignments
- `Views/Venue/Detail.cshtml` L:54: `Model.Venue.OwnerPhone` → `Model.Venue.ContactPhone`
- `Views/Admin/Index.cshtml`: tất cả `venue.OwnerName` → `venue.ContactName`, `booking.OwnerName` → xóa/đổi
- `AdminManagementViewModels.cs`: đổi `OwnerName` → `ContactName` trong các ViewModel

#### 3.3 — `Data/Entities/UserSnapshotEntity.cs`
- **Xóa file** hoàn toàn.

#### 3.4 — `Data/ApplicationDbContext.cs`
- Xóa `DbSet<UserSnapshotEntity>` (L:12)
- Xóa `using` nếu cần
- Trong `OnModelCreating()`:
  - Xóa Venue → OwnerUser relationship (L:51-54):
    ```csharp
    // XÓA:
    entity.HasOne(x => x.OwnerUser)
        .WithMany(x => x.OwnedVenues)
        .HasForeignKey(x => x.OwnerUserId)
        .OnDelete(DeleteBehavior.SetNull);
    ```
  - Rename mapping (L:42-43):
    ```csharp
    // ĐỔI:
    entity.Property(x => x.OwnerName)  → entity.Property(x => x.ContactName)
    entity.Property(x => x.OwnerPhone) → entity.Property(x => x.ContactPhone)
    ```
  - Xóa toàn bộ `UserSnapshotEntity` mapping block (L:85-91)

### Cách test Phase 3

```
✅ dotnet build → Build succeeded, 0 Error(s)
   (Tất cả compile error từ rename phải được sửa đồng thời)
⚠️ dotnet run → EXPECTED LỖI runtime (DB schema chưa khớp entity)
   Đây là bình thường – Phase 4 sẽ tạo migration để fix.
```

---

## Phase 4: Tạo migration scale-down

### Mục tiêu
Tạo EF Core migration để đồng bộ database schema với entity mới. Thêm data migration SQL để chuyển Owner users sang Player và update VenueStatus.

### Việc cần làm

#### 4.1 — Tạo migration
```bash
cd BadmintonCourtBooking
dotnet ef migrations add ScaleDownToAdminPlayerRoles
```

EF sẽ tự generate migration file với các thao tác:
- Drop FK `FK_Venues_Users_OwnerUserId`
- Drop index `IX_Venues_OwnerUserId`
- Drop column `Venues.OwnerUserId`
- Rename column `Venues.OwnerName` → `Venues.ContactName`
- Rename column `Venues.OwnerPhone` → `Venues.ContactPhone`
- Drop table `UserSnapshots`

#### 4.2 — Review migration file
Mở file migration vừa tạo trong `Migrations/` và kiểm tra:
- ✅ Có `RenameColumn` (không phải drop + add, sẽ mất dữ liệu)
- ✅ Có `DropForeignKey`
- ✅ Có `DropIndex`
- ✅ Có `DropColumn` cho `OwnerUserId`
- ✅ Có `DropTable` cho `UserSnapshots`

#### 4.3 — Thêm data migration SQL
Mở file migration, tìm method `Up()`, thêm **ở ĐẦU** method, trước các schema changes:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // ===== DATA MIGRATION (chạy TRƯỚC schema changes) =====
    migrationBuilder.Sql("UPDATE [Users] SET [Role] = 'Player' WHERE [Role] = 'Owner'");
    migrationBuilder.Sql("UPDATE [Venues] SET [Status] = 'Approved' WHERE [Status] = 'PendingApproval'");

    // ... (giữ nguyên các EF auto-generated statements bên dưới) ...
}
```

#### 4.4 — Apply migration
```bash
dotnet ef database update
```

### Cách test Phase 4

```
✅ dotnet ef migrations add → Tạo file migration thành công
✅ Review migration file → Có RenameColumn (không phải drop+add)
✅ dotnet ef database update → Migration apply thành công
✅ dotnet run → App khởi động, seed chạy không lỗi
✅ Kiểm tra DB trực tiếp (SSMS hoặc Azure Data Studio):
   - Bảng Users: không còn user nào có Role = 'Owner'
   - Bảng Venues: không còn cột OwnerUserId
   - Bảng Venues: cột đã đổi thành ContactName, ContactPhone
   - Bảng Venues: không còn venue nào có Status = 'PendingApproval'
   - Bảng UserSnapshots: không tồn tại
✅ dotnet ef migrations list → Có 3 migration: InitialCreate, AddAuthenticationAndUsers, ScaleDownToAdminPlayerRoles
```

---

## Phase 5: Chuyển logic Owner sang Admin

### Mục tiêu
Admin nhận toàn bộ quyền quản lý venue/court/booking mà Owner đang làm. Tạo actions mới trong AdminController, mở rộng AdminDashboardService, tạo view Admin/Venues.

### Ngữ cảnh quan trọng
- **KHÔNG xóa OwnerController** ở phase này. Xóa ở Phase 7.
- Mục đích: xây xong Admin thay thế, rồi mới gỡ Owner.
- Logic port từ `OwnerDashboardService` → `AdminDashboardService`. Khác biệt chính: Admin không filter theo `OwnerUserId` (admin quản lý toàn bộ).

### Việc cần làm

#### 5.1 — `Models/AdminManagementViewModels.cs`
Thêm các ViewModel mới cho Admin quản lý venue/court:

- `AdminVenueManagementViewModel` – tương tự `OwnerVenueManagementViewModel`:
  - `AdminName` (string)
  - `SelectedVenueId` (string)
  - `Venues` (List)
  - `HasVenues` (bool)

- `AdminVenueDetailViewModel` – tương tự `OwnerVenueViewModel`:
  - Id, Name, District, Address, OpenTime, CloseTime, Description, Status, PriceFrom, ActiveCourtCount, TotalCourtCount
  - `Courts` (List<AdminCourtDetailViewModel>)
  - StatusLabel, StatusTone property

- `AdminCourtDetailViewModel` – tương tự `OwnerCourtViewModel`:
  - Id, VenueId, Name, PricePerHour, Note, IsActive

- `AdminVenueInputModel` – tương tự `OwnerVenueInputModel`:
  - Id?, SelectedVenueId?, Name, District, Address, OpenTime, CloseTime, Description
  - Có DataAnnotation validation

- `AdminCourtInputModel` – tương tự `OwnerCourtInputModel`:
  - Id?, SelectedVenueId?, VenueId, Name, PricePerHour, Note, IsActive
  - Có DataAnnotation validation

Sửa các ViewModel hiện có:
- `AdminDashboardViewModel`: xóa `TotalOwners`, `PendingVenueCount`, `PendingVenues`
- `AdminUserOverviewViewModel`: xóa `OwnedVenueCount`. Sửa `ActivitySummary` chỉ hiện booking count. Xóa case `AppRoles.Owner` trong `RoleTone`.
- `AdminVenueOverviewViewModel`: đổi `OwnerName` → `ContactName`
- `AdminBookingOverviewViewModel`: đổi/xóa `OwnerName`
- `AdminPendingVenueViewModel`: **xóa class** (không còn pending venue flow)

#### 5.2 — `Services/IAdminDashboardService.cs`
Thêm methods mới:

```csharp
// Booking management
Task<OperationResult> ApproveBookingAsync(string id, CancellationToken cancellationToken = default);
Task<OperationResult> RejectBookingAsync(string id, CancellationToken cancellationToken = default);

// Venue management
Task<AdminVenueManagementViewModel> GetVenueManagementAsync(string? selectedVenueId = null, CancellationToken cancellationToken = default);
Task<OperationResult<string>> CreateVenueAsync(AdminVenueInputModel model, CancellationToken cancellationToken = default);
Task<OperationResult<string>> UpdateVenueAsync(AdminVenueInputModel model, CancellationToken cancellationToken = default);
Task<OperationResult<string>> CreateCourtAsync(AdminCourtInputModel model, CancellationToken cancellationToken = default);
Task<OperationResult<string>> UpdateCourtAsync(AdminCourtInputModel model, CancellationToken cancellationToken = default);
```

#### 5.3 — `Services/AdminDashboardService.cs`
Implement các method mới. Port logic từ `OwnerDashboardService` với khác biệt:

**ApproveBookingAsync / RejectBookingAsync:**
- Port từ `OwnerDashboardService.ApproveBookingAsync()` / `RejectBookingAsync()`.
- **Khác:** Không cần `FindOwnerBookingAsync()` (filter theo owner). Thay bằng query trực tiếp `_context.Bookings.FirstOrDefaultAsync(b => b.Id == id)`.
- Đổi `CancelReason = "Từ chối bởi chủ sân"` → `"Từ chối bởi quản trị viên"`.

**GetVenueManagementAsync:**
- Port từ `OwnerDashboardService.GetVenueManagementAsync()`.
- **Khác:** Không filter `venue.OwnerUserId == currentUser.UserId`. Load tất cả venues.
- Dùng `ContactName`/`ContactPhone` thay vì `OwnerName`/`OwnerPhone`.

**CreateVenueAsync:**
- Port từ `OwnerDashboardService.CreateVenueAsync()`.
- **Khác:** Không set `OwnerUserId`. Set `Status = VenueStatus.Approved` (admin tạo thì active luôn). Set `ContactName`/`ContactPhone` từ input hoặc admin info.
- Thêm fields `ContactName`, `ContactPhone` vào `AdminVenueInputModel`.

**UpdateVenueAsync / CreateCourtAsync / UpdateCourtAsync:**
- Port tương tự, bỏ filter owner.

Cập nhật `GetDashboardAsync()`:
- Xóa `TotalOwners` (L:65)
- Xóa `venuesByOwner` dictionary (L:49-52)
- Xóa `OwnedVenueCount` assignment (L:107)
- Đổi `venue.OwnerName` → `venue.ContactName` (L:90, 122, 139)
- Đổi `venue.OwnerPhone` → `venue.ContactPhone` (L:91)
- Xóa `PendingVenues` list mapping (L:83-97) – không còn pending venue flow
- Đổi `PendingVenueCount` → 0 hoặc xóa

#### 5.4 — `Controllers/AdminController.cs`
Thêm actions mới:

```csharp
// Venue management page
public async Task<IActionResult> Venues(string? selectedVenueId, CancellationToken cancellationToken)

// Venue CRUD
[HttpPost] public async Task<IActionResult> CreateVenue(AdminVenueInputModel model, ...)
[HttpPost] public async Task<IActionResult> UpdateVenue(AdminVenueInputModel model, ...)

// Court CRUD
[HttpPost] public async Task<IActionResult> CreateCourt(AdminCourtInputModel model, ...)
[HttpPost] public async Task<IActionResult> UpdateCourt(AdminCourtInputModel model, ...)

// Booking management
[HttpPost] public async Task<IActionResult> ApproveBooking(string id, ...)
[HttpPost] public async Task<IActionResult> RejectBooking(string id, ...)
```

Pattern: copy từ `OwnerController`, đổi service call sang `_adminDashboardService`, đổi redirect targets.

#### 5.5 — `Views/Admin/Venues.cshtml`
Tạo file mới. Port UI từ `Views/Owner/Venues.cshtml`:
- Đổi tất cả `asp-controller="Owner"` → `asp-controller="Admin"`
- Đổi model type `OwnerVenueManagementViewModel` → `AdminVenueManagementViewModel`
- Đổi `OwnerName` → `AdminName` hoặc label chung "Quản trị viên"
- Xóa sidebar link "Dashboard chủ sân" → thay bằng link "Dashboard Admin"
- Xóa text "Chờ duyệt" / "Gửi duyệt lại" → Admin tạo venue luôn Active

#### 5.6 — `Views/Admin/Index.cshtml`
Sửa dashboard:
- Thêm nút approve/reject booking vào bảng booking (cột action).
- Thêm link "Quản lý cụm sân" trong sidebar hoặc navigation.
- Xóa section "Pending Venues chờ duyệt" (không còn flow owner gửi).
- Xóa text "chủ sân" trong các label.
- Đổi `@venue.OwnerName` → `@venue.ContactName`.
- Xóa cột "Owner" trong bảng booking.

### Cách test Phase 5

```
✅ dotnet build → Build succeeded
✅ dotnet run → App chạy
✅ Login Admin → /Admin → Dashboard hiển thị (không còn "chủ sân" count)
✅ /Admin/Venues → Hiển thị danh sách venue, có thể chọn venue xem chi tiết
✅ /Admin/Venues → Tạo venue mới → Venue xuất hiện trong danh sách, status Approved
✅ /Admin/Venues → Sửa venue → Thông tin cập nhật
✅ /Admin/Venues → Tạo court mới → Court xuất hiện trong venue
✅ /Admin/Venues → Sửa court → Thông tin cập nhật
✅ Player đặt sân → Booking status Pending
✅ Admin /Admin → Thấy booking Pending, click Approve → Status chuyển Confirmed
✅ Admin /Admin → Thấy booking Pending, click Reject → Status chuyển Cancelled
```

---

## Phase 6: Cập nhật services/views còn lại

### Mục tiêu
Sửa các file còn sót reference Owner mà chưa được sửa ở Phase 3/5.

### Việc cần làm

#### 6.1 — `Services/BookingService.cs`
- L:108: Đổi message:
  ```csharp
  // CŨ: "Chủ sân sẽ xác nhận trong ít phút."
  // MỚI: "Quản trị viên sẽ xác nhận trong ít phút."
  ```

#### 6.2 — `Services/AdminDashboardService.cs`
- Kiểm tra message trong `RejectVenueAsync()`:
  ```csharp
  // CŨ: "Chủ sân có thể cập nhật lại hồ sơ trước khi gửi duyệt lại."
  // MỚI: "Venue đã được ẩn khỏi trang public."
  ```

#### 6.3 — Grep final check
Chạy grep toàn bộ project tìm reference "Owner" còn sót (ngoại trừ OwnerController, OwnerDashboardService, Owner views – sẽ xóa Phase 7):

```bash
grep -rn "Owner" --include="*.cs" --include="*.cshtml" BadmintonCourtBooking/
```

Sửa tất cả reference còn sót.

### Cách test Phase 6

```
✅ dotnet build → Build succeeded
✅ Player đặt sân → Message hiện "Quản trị viên sẽ xác nhận..."
✅ Admin reject venue → Message không còn nói "chủ sân"
✅ grep "Owner" → Chỉ còn trong:
   - OwnerController.cs (sẽ xóa Phase 7)
   - OwnerDashboardService.cs (sẽ xóa Phase 7)
   - IOwnerDashboardService.cs (sẽ xóa Phase 7)
   - OwnerManagementViewModels.cs (sẽ xóa Phase 7)
   - Views/Owner/ (sẽ xóa Phase 7)
   - owner-dashboard.js (sẽ xóa Phase 7)
   - MockData.cs OwnerBookings (sẽ xóa Phase 7)
   - Booking.cs OwnerBooking class (sẽ xóa Phase 7)
   - Program.cs DI registration (sẽ xóa Phase 7)
```

---

## Phase 7: Gỡ Owner khỏi code

### Mục tiêu
Xóa toàn bộ file và code liên quan Owner. Chỉ thực hiện SAU KHI Phase 5-6 hoàn tất và Admin đã có đủ chức năng thay thế.

### Việc cần làm (thứ tự quan trọng)

#### 7.1 — `Program.cs`
- Xóa DI registration (L:34):
  ```csharp
  builder.Services.AddScoped<IOwnerDashboardService, OwnerDashboardService>();
  ```

#### 7.2 — Xóa files
Xóa theo thứ tự (controller → service → model → view → JS):

| # | File xóa | Lý do |
|---|---------|-------|
| 1 | `Controllers/OwnerController.cs` | Controller Owner |
| 2 | `Services/OwnerDashboardService.cs` | Service Owner |
| 3 | `Services/IOwnerDashboardService.cs` | Interface Owner |
| 4 | `Models/OwnerManagementViewModels.cs` | ViewModels Owner |
| 5 | `Models/DashboardViewModel.cs` | Mock model cũ |
| 6 | `Models/AdminViewModel.cs` | Mock model cũ |
| 7 | `Views/Owner/Dashboard.cshtml` | View Owner |
| 8 | `Views/Owner/Venues.cshtml` | View Owner |
| 9 | Xóa thư mục `Views/Owner/` | Thư mục trống |
| 10 | `wwwroot/js/owner-dashboard.js` | JS Owner |

#### 7.3 — `Models/Booking.cs`
- Xóa class `OwnerBooking` (L:21-30). Giữ lại `Booking`, `BookingStatus`, `SlotStatus`.

#### 7.4 — `Models/MockData.cs`
- Xóa `OwnerBookings` list (L:211-263)
- Xóa `PendingVenues` list (L:278-307) và class `PendingVenue` (nếu định nghĩa trong file)
- Xóa `RecentUsers` list (L:310-317) – hoặc cập nhật bỏ "Chủ sân"
- Xóa `GetDashboardViewModel()` (L:386-397)
- Xóa `GetAdminViewModel()` (L:402-413)
- Xóa class `RecentUser`, `PendingVenue` nếu không còn dùng

### Cách test Phase 7

```
✅ dotnet build → Build succeeded, 0 Error(s)
✅ dotnet run → App chạy không lỗi
✅ Truy cập /Owner/Dashboard bằng URL → 404 Not Found (controller đã xóa)
✅ Truy cập /Owner/Venues bằng URL → 404 Not Found
✅ grep -rn "OwnerController" → 0 kết quả
✅ grep -rn "IOwnerDashboardService" → 0 kết quả
✅ grep -rn "OwnerDashboardService" → 0 kết quả (trừ migration snapshot nếu có)
✅ Không còn file nào trong Views/Owner/
✅ Không còn file owner-dashboard.js
✅ Player flow vẫn hoạt động bình thường
✅ Admin flow vẫn hoạt động bình thường
```

---

## Phase 8: Seeder/demo data

### Mục tiêu
Refactor seeder để không còn seed Owner users, không còn pending venues, seed data clean cho 2-role system.

### Ngữ cảnh quan trọng
- Seeder hiện tại seed 11 users (1 admin, 1 player demo, 1 owner demo, 8 supporting users).
- Sau scale-down: chỉ seed Admin + Player users.
- Owner users cũ đã được migration chuyển thành Player (Phase 4). Seeder cần phản ánh điều này.

### Việc cần làm

#### 8.1 — `Data/DemoDataConstants.cs`
Nếu chưa xóa ở Phase 2 (Cách B), xóa bây giờ:
- Xóa `DemoOwnerUserId`, `DemoOwnerName`, `DemoOwnerEmail`, `DemoOwnerPhone`, `DemoOwnerPassword`, `DemoOwnerVenueId`

#### 8.2 — `Data/ApplicationDbContextSeeder.cs`
Refactor toàn bộ:

**`SeedAsync()`:**
- Xóa call `SeedUserSnapshotsAsync()` (bảng đã drop)
- Xóa call `SyncVenueOwnersAsync()` (không còn OwnerUserId)
- Giữ: `SeedUsersAsync()`, `SeedVenuesAsync()`, `SeedBookingsAsync()`, `SyncBookingPlayersAsync()`

**`BuildSeedAccounts()`:**
- Xóa owner demo account entry (L:390-399)
- Các owner users phụ (L:412-420): chuyển role thành `AppRoles.Player`:
  - `user-owner-vu` → Player
  - `user-owner-hai` → Player
  - `user-owner-hung` → Player
  - `user-owner-mai` → Player
  - `user-owner-viet` → Player
  - `user-owner-linh` → Player (cần giữ vì có booking liên kết)
  - `user-owner-tuan-anh` → Player (cần giữ vì có booking liên kết)

**`SeedVenuesAsync()`:**
- Xóa `MockData.PendingVenues` reference (L:80): không seed pending venues
- Xóa block seed courts cho pending venues (L:96-110)

**Xóa methods:**
- `SyncVenueOwnersAsync()` – không còn OwnerUserId
- `SeedUserSnapshotsAsync()` – bảng đã drop
- `MapPendingVenue()` – không còn pending venue
- `ResolveOwnerUserId()` – không còn cần

**`MapApprovedVenue()`:**
- Xóa `OwnerUserId = ResolveOwnerUserId(venue.Id)` → không set
- Đổi `OwnerName = ownerName` → `ContactName = contactName`
- Đổi `OwnerPhone = ownerPhone` → `ContactPhone = contactPhone`

**`ResolveOwner()`:**
- Rename thành `ResolveContact()` hoặc giữ tên nhưng return `(contactName, contactPhone)`

**Booking seed (L:169-188):**
- Block `MockData.OwnerBookings` đã bị xóa ở Phase 7 → xóa toàn bộ foreach block này
- Hoặc chuyển thành booking bình thường nếu muốn giữ demo data nhiều hơn
- Đổi `DemoDataConstants.DemoOwnerVenueId` → hardcode `"v1"` (vì constant đã xóa)
- Đổi `DemoDataConstants.DemoOwnerUserId` reference → xóa hoặc thay bằng Player user Id

**`ResolveBookingUserId()`:**
- Giữ nguyên logic mapping tên → userId (vẫn cần cho `SyncBookingPlayersAsync`)

### Cách test Phase 8

```
✅ dotnet build → Build succeeded
✅ Xóa database local hoàn toàn:
   dotnet ef database drop --force
✅ dotnet run → App khởi động:
   - Migration chạy tự động (tạo DB mới từ đầu)
   - Seeder chạy không lỗi
   - Seed data tạo thành công
✅ Kiểm tra DB:
   - Bảng Users: chỉ có Admin + Player roles
   - Bảng Venues: tất cả status Approved, có ContactName/ContactPhone
   - Bảng Courts: seed đầy đủ
   - Bảng Bookings: seed đầy đủ, PlayerUserId valid
   - Không có bảng UserSnapshots
✅ Login Player demo → hoạt động
✅ Login Admin demo → hoạt động
✅ /Account/Login → Panel demo chỉ có Player + Admin
✅ Player đặt sân → thành công
✅ Admin xem booking → thấy booking
```

---

## Phase 9: Final verification

### Mục tiêu
Test toàn diện, đảm bảo không còn reference Owner, tất cả flow hoạt động.

### Việc cần làm

#### 9.1 — Grep verification
```bash
# Tìm tất cả reference "Owner" còn sót
grep -rn "Owner" --include="*.cs" --include="*.cshtml" --include="*.js" BadmintonCourtBooking/

# Kết quả chấp nhận được: CHỈ trong migration files (snapshot, migration history)
# Không chấp nhận: bất kỳ file nào khác
```

#### 9.2 — Fresh database test
```bash
dotnet ef database drop --force
dotnet run
```

#### 9.3 — Existing database migration test
```bash
# Tạo DB với migration cũ, rồi migrate lên
dotnet ef database update AddAuthenticationAndUsers
# Chạy app (sẽ tự migrate và seed)
dotnet run
```

### Checklist test thủ công cuối cùng

| # | Test case | Bước thực hiện | Expected |
|---|-----------|---------------|----------|
| 1 | Anonymous xem trang chủ | Mở `/` | Hiển thị hero, venues nổi bật. KHÔNG CÓ section chủ sân. |
| 2 | Anonymous tìm sân | Mở `/Venue` | Danh sách venue Approved hiển thị. |
| 3 | Anonymous xem chi tiết sân | Click venue → `/Venue/Detail/v1` | Chi tiết sân + slot matrix + SĐT liên hệ (ContactPhone). |
| 4 | Anonymous truy cập booking | Mở `/Booking` | Redirect về login. |
| 5 | Đăng ký Player | `/Account/Register` | Chỉ 1 form Player, KHÔNG CÓ tab chủ sân. Đăng ký thành công. |
| 6 | Login Player demo | `/Account/Login` → dùng demo button | Login thành công, redirect về Home. |
| 7 | Player đặt sân | Venue detail → chọn slot → đặt | Booking tạo, status Pending. Message "Quản trị viên sẽ xác nhận". |
| 8 | Player xem lịch | `/Booking` | Hiển thị booking vừa đặt. |
| 9 | Player hủy booking | Click hủy | Status chuyển Cancelled. |
| 10 | Player xem profile | `/Account/Profile` | Hiển thị thông tin cá nhân. |
| 11 | Login Admin demo | `/Account/Login` → dùng demo button | Login thành công, redirect về `/Admin`. |
| 12 | Admin dashboard | `/Admin` | Hiển thị tổng quan. KHÔNG CÓ text "chủ sân". |
| 13 | Admin approve booking | Dashboard → click Approve | Booking status → Confirmed. |
| 14 | Admin reject booking | Dashboard → click Reject | Booking status → Cancelled. |
| 15 | Admin quản lý venue | `/Admin/Venues` | Hiển thị danh sách venue, có thể sửa. |
| 16 | Admin tạo venue | Form tạo venue mới | Venue tạo, status Approved. |
| 17 | Admin tạo court | Form tạo court | Court thêm vào venue. |
| 18 | Admin approve/reject venue | Dashboard | Venue public hoặc ẩn. |
| 19 | Admin lock user | Dashboard → lock player | Player không thể login. |
| 20 | Navigation check | Kiểm tra tất cả nav links | KHÔNG CÓ link Owner ở header, mobile menu, footer. |
| 21 | Demo panel check | Login page | Chỉ có 2 button demo: Player + Admin. |
| 22 | URL Owner trực tiếp | Nhập `/Owner/Dashboard` | 404 Not Found. |
| 23 | Fresh DB | Xóa DB → chạy lại | App chạy, seed pass. |
| 24 | Register Admin | Thử đăng ký role admin | Bị chặn (không cho tự đăng ký admin). |

### Acceptance criteria cuối cùng

```
✅ dotnet build → 0 errors, 0 warnings (hoặc warnings không liên quan)
✅ Migration chạy pass trên cả fresh DB và existing DB
✅ Seed chạy pass
✅ Không còn link/text/button Owner trên UI
✅ Không còn tab Owner trên login/register
✅ Không còn route /Owner/* hoạt động
✅ Player booking flow hoàn chỉnh
✅ Admin quản lý venue/court/booking hoàn chỉnh
✅ grep "Owner" chỉ ra kết quả trong migration files
```

---

## Tóm tắt thứ tự an toàn

```
Phase 1: Baseline          → Verify build + run
Phase 2: Xóa Owner UI      → Chỉ sửa views/models/controllers, DB không đổi
Phase 3: Sửa Entities      → Sửa C# entities + DbContext, build only
Phase 4: Migration          → Tạo + apply migration, DB schema update
Phase 5: Port logic→Admin  → Xây Admin thay thế Owner
Phase 6: Cleanup refs       → Sửa message/text còn sót
Phase 7: Xóa Owner files   → Xóa controller/service/views/JS
Phase 8: Refactor Seeder    → Clean seed data
Phase 9: Final test         → Grep + manual test toàn diện
```

> **Nguyên tắc vàng:** Mỗi phase kết thúc bằng `dotnet build` pass. Phase 4+ phải `dotnet run` pass. Không bao giờ xóa Owner files (Phase 7) trước khi Admin đã có đủ chức năng (Phase 5).
