using System.Globalization;

namespace BadmintonCourtBooking.Models;

public static class MockData
{
    // ── Vietnamese culture for currency formatting ──
    private static readonly CultureInfo ViCulture = new("vi-VN");

    // ── Time Slots ──
    public static readonly List<string> TimeSlots = new()
    {
        "06:00", "07:00", "08:00", "09:00", "10:00", "11:00",
        "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00"
    };

    // ── Venues ──
    public static readonly List<Venue> Venues = new()
    {
        new Venue
        {
            Id = "v1",
            Name = "Sân Cầu Lông Phú Thọ",
            District = "Quận 11",
            Address = "176 Lữ Gia, P.15",
            OpenHours = "06:00 – 22:00",
            PriceFrom = 90_000,
            Rating = 4.7,
            Reviews = 218,
            ResponseFast = true,
            HasSlotsToday = true,
            Highlight = "Còn 4 khung giờ đẹp hôm nay",
            Description = "Sân cầu lông chất lượng cao tại Quận 11 với 4 sân tiêu chuẩn, sàn nhựa chống trượt và hệ thống đèn LED hiện đại.",
            Courts = new List<Court>
            {
                new() { Id = "c1", Name = "Sân 1", PricePerHour = 90_000 },
                new() { Id = "c2", Name = "Sân 2", PricePerHour = 90_000 },
                new() { Id = "c3", Name = "Sân 3", PricePerHour = 110_000, Note = "Sàn mới" },
                new() { Id = "c4", Name = "Sân VIP", PricePerHour = 150_000, Note = "Điều hòa" }
            }
        },
        new Venue
        {
            Id = "v2",
            Name = "Tân Bình Badminton Hub",
            District = "Quận Tân Bình",
            Address = "23 Bàu Cát 2, P.14",
            OpenHours = "05:30 – 23:00",
            PriceFrom = 100_000,
            Rating = 4.5,
            Reviews = 142,
            ResponseFast = false,
            HasSlotsToday = true,
            Highlight = "Mới mở – ưu đãi khung sáng",
            Description = "Trung tâm cầu lông mới mở tại Tân Bình với trang thiết bị hiện đại và nhiều ưu đãi hấp dẫn cho khung giờ sáng.",
            Courts = new List<Court>
            {
                new() { Id = "c1", Name = "Sân A1", PricePerHour = 100_000 },
                new() { Id = "c2", Name = "Sân A2", PricePerHour = 100_000 },
                new() { Id = "c3", Name = "Sân A3", PricePerHour = 100_000 },
                new() { Id = "c4", Name = "Sân B1", PricePerHour = 120_000 }
            }
        },
        new Venue
        {
            Id = "v3",
            Name = "Sân Cầu Lông Bình Thạnh",
            District = "Quận Bình Thạnh",
            Address = "85 Nơ Trang Long, P.13",
            OpenHours = "06:00 – 22:30",
            PriceFrom = 80_000,
            Rating = 4.3,
            Reviews = 96,
            ResponseFast = true,
            HasSlotsToday = false,
            Highlight = null,
            Description = "Sân cầu lông bình dân tại Bình Thạnh, phù hợp cho người chơi phong trào với mức giá hợp lý.",
            Courts = new List<Court>
            {
                new() { Id = "c1", Name = "Sân 1", PricePerHour = 80_000 },
                new() { Id = "c2", Name = "Sân 2", PricePerHour = 80_000 },
                new() { Id = "c3", Name = "Sân 3", PricePerHour = 95_000 }
            }
        },
        new Venue
        {
            Id = "v4",
            Name = "Quận 10 Sport Center",
            District = "Quận 10",
            Address = "12 Thành Thái, P.14",
            OpenHours = "06:00 – 22:00",
            PriceFrom = 120_000,
            Rating = 4.8,
            Reviews = 305,
            ResponseFast = true,
            HasSlotsToday = true,
            Highlight = "Chủ sân phản hồi nhanh",
            Description = "Trung tâm thể thao cao cấp tại Quận 10 với sân thi đấu đạt chuẩn quốc tế và dịch vụ chuyên nghiệp.",
            Courts = new List<Court>
            {
                new() { Id = "c1", Name = "Sân 1", PricePerHour = 120_000 },
                new() { Id = "c2", Name = "Sân 2", PricePerHour = 120_000 },
                new() { Id = "c3", Name = "Sân VIP", PricePerHour = 180_000, Note = "Sàn thi đấu" }
            }
        },
        new Venue
        {
            Id = "v5",
            Name = "Sân Cầu Lông Gò Vấp",
            District = "Quận Gò Vấp",
            Address = "210 Quang Trung, P.10",
            OpenHours = "06:30 – 22:00",
            PriceFrom = 75_000,
            Rating = 4.1,
            Reviews = 64,
            ResponseFast = false,
            HasSlotsToday = false,
            Highlight = null,
            Description = "Sân cầu lông giá rẻ tại Gò Vấp, thích hợp cho buổi tập luyện nhẹ nhàng cuối tuần.",
            Courts = new List<Court>
            {
                new() { Id = "c1", Name = "Sân 1", PricePerHour = 75_000 },
                new() { Id = "c2", Name = "Sân 2", PricePerHour = 75_000 }
            }
        },
        new Venue
        {
            Id = "v6",
            Name = "Thủ Đức Smash Arena",
            District = "TP. Thủ Đức",
            Address = "45 Võ Văn Ngân, P. Linh Chiểu",
            OpenHours = "06:00 – 23:00",
            PriceFrom = 110_000,
            Rating = 4.6,
            Reviews = 188,
            ResponseFast = false,
            HasSlotsToday = true,
            Highlight = null,
            Description = "Sân cầu lông hiện đại tại Thủ Đức với không gian rộng rãi và tiện ích đầy đủ.",
            Courts = new List<Court>
            {
                new() { Id = "c1", Name = "Sân 1", PricePerHour = 110_000 },
                new() { Id = "c2", Name = "Sân 2", PricePerHour = 110_000 },
                new() { Id = "c3", Name = "Sân 3", PricePerHour = 110_000 },
                new() { Id = "c4", Name = "Sân 4", PricePerHour = 130_000 }
            }
        }
    };

    // ── My Bookings (Customer) ──
    public static readonly List<Booking> MyBookings = new()
    {
        new Booking
        {
            Id = "b1",
            Venue = "Sân Cầu Lông Phú Thọ",
            Court = "Sân VIP",
            Date = "Thứ 5, 21/05/2026",
            Time = "19:00 – 20:00",
            Total = 150_000,
            Status = BookingStatus.Confirmed,
            When = "upcoming"
        },
        new Booking
        {
            Id = "b2",
            Venue = "Quận 10 Sport Center",
            Court = "Sân 2",
            Date = "CN, 24/05/2026",
            Time = "17:00 – 19:00",
            Total = 240_000,
            Status = BookingStatus.Pending,
            When = "upcoming"
        },
        new Booking
        {
            Id = "b3",
            Venue = "Thủ Đức Smash Arena",
            Court = "Sân 3",
            Date = "Thứ 7, 10/05/2026",
            Time = "18:00 – 19:00",
            Total = 110_000,
            Status = BookingStatus.Completed,
            When = "completed"
        },
        new Booking
        {
            Id = "b4",
            Venue = "Tân Bình Badminton Hub",
            Court = "Sân A1",
            Date = "Thứ 3, 06/05/2026",
            Time = "20:00 – 21:00",
            Total = 100_000,
            Status = BookingStatus.Completed,
            When = "completed"
        },
        new Booking
        {
            Id = "b5",
            Venue = "Sân Cầu Lông Bình Thạnh",
            Court = "Sân 1",
            Date = "Thứ 2, 28/04/2026",
            Time = "07:00 – 08:00",
            Total = 80_000,
            Status = BookingStatus.Cancelled,
            When = "cancelled"
        }
    };

    // ── Owner Bookings ──
    public static readonly List<OwnerBooking> OwnerBookings = new()
    {
        new OwnerBooking
        {
            Id = "ob1",
            Customer = "Nguyễn Minh Khoa",
            Phone = "0905***221",
            Court = "Sân 1",
            Time = "Hôm nay · 18:00",
            Total = 90_000,
            Status = BookingStatus.Pending
        },
        new OwnerBooking
        {
            Id = "ob2",
            Customer = "Trần Phương Linh",
            Phone = "0987***104",
            Court = "Sân VIP",
            Time = "Hôm nay · 19:00",
            Total = 150_000,
            Status = BookingStatus.Confirmed
        },
        new OwnerBooking
        {
            Id = "ob3",
            Customer = "Lê Quốc Bảo",
            Phone = "0912***876",
            Court = "Sân 2",
            Time = "Hôm nay · 20:00",
            Total = 90_000,
            Status = BookingStatus.Pending
        },
        new OwnerBooking
        {
            Id = "ob4",
            Customer = "Phạm Hồng Nhung",
            Phone = "0934***012",
            Court = "Sân 3",
            Time = "Mai · 07:00",
            Total = 110_000,
            Status = BookingStatus.Confirmed
        },
        new OwnerBooking
        {
            Id = "ob5",
            Customer = "Đặng Tuấn Anh",
            Phone = "0976***455",
            Court = "Sân 1",
            Time = "Mai · 17:00",
            Total = 90_000,
            Status = BookingStatus.Cancelled
        }
    };

    // ── Revenue Data ──
    public static readonly List<RevenueDataPoint> RevenueData = new()
    {
        new RevenueDataPoint { Day = "T2", Value = 1.8 },
        new RevenueDataPoint { Day = "T3", Value = 2.1 },
        new RevenueDataPoint { Day = "T4", Value = 2.6 },
        new RevenueDataPoint { Day = "T5", Value = 2.2 },
        new RevenueDataPoint { Day = "T6", Value = 3.4 },
        new RevenueDataPoint { Day = "T7", Value = 4.1 },
        new RevenueDataPoint { Day = "CN", Value = 3.7 }
    };

    // ── Pending Venues (Admin) ──
    public static readonly List<PendingVenue> PendingVenues = new()
    {
        new PendingVenue
        {
            Id = "pv1",
            Name = "Sân Cầu Lông An Phú",
            Owner = "Trần Văn Hùng",
            District = "Quận 2",
            Courts = 4,
            Submitted = "18/05/2026"
        },
        new PendingVenue
        {
            Id = "pv2",
            Name = "Bình Tân Sport Hub",
            Owner = "Lê Thị Mai",
            District = "Bình Tân",
            Courts = 6,
            Submitted = "17/05/2026"
        },
        new PendingVenue
        {
            Id = "pv3",
            Name = "Sân Hoàng Hoa Thám",
            Owner = "Phạm Quốc Việt",
            District = "Tân Bình",
            Courts = 3,
            Submitted = "16/05/2026"
        }
    };

    // ── Recent Users (Admin) ──
    public static readonly List<RecentUser> RecentUsers = new()
    {
        new RecentUser { Name = "Nguyễn Minh Khoa", JoinDate = "20/05/2026", Role = "Khách hàng" },
        new RecentUser { Name = "Trần Phương Linh", JoinDate = "19/05/2026", Role = "Chủ sân" },
        new RecentUser { Name = "Lê Quốc Bảo", JoinDate = "18/05/2026", Role = "Khách hàng" },
        new RecentUser { Name = "Phạm Hồng Nhung", JoinDate = "17/05/2026", Role = "Khách hàng" },
        new RecentUser { Name = "Đặng Tuấn Anh", JoinDate = "16/05/2026", Role = "Chủ sân" }
    };

    // ── Helpers ──

    /// <summary>
    /// Formats an integer as Vietnamese dong, e.g. 150000 -> "150.000₫"
    /// </summary>
    public static string FormatVND(int value)
    {
        return value.ToString("N0", ViCulture) + "₫";
    }

    /// <summary>
    /// Builds a slot matrix for a venue on a given date key.
    /// Returns Dictionary&lt;courtName, Dictionary&lt;timeSlot, SlotStatus&gt;&gt;
    /// Uses deterministic pseudo-random based on dateKey hash.
    /// </summary>
    public static Dictionary<string, Dictionary<string, SlotStatus>> BuildSlotMatrix(Venue venue, string dateKey)
    {
        var matrix = new Dictionary<string, Dictionary<string, SlotStatus>>();
        int hash = GetSimpleHash(dateKey);

        foreach (var court in venue.Courts)
        {
            var courtSlots = new Dictionary<string, SlotStatus>();
            int courtHash = hash + GetSimpleHash(court.Id);

            foreach (var slot in TimeSlots)
            {
                int slotHash = Math.Abs(courtHash + GetSimpleHash(slot));
                int mod = slotHash % 10;

                // ~40% Available, ~40% Booked, ~20% Pending
                SlotStatus status;
                if (mod < 4)
                    status = SlotStatus.Available;
                else if (mod < 8)
                    status = SlotStatus.Booked;
                else
                    status = SlotStatus.Pending;

                courtSlots[slot] = status;
            }

            matrix[court.Name] = courtSlots;
        }

        return matrix;
    }

    /// <summary>
    /// Simple deterministic hash for a string (consistent across runs).
    /// </summary>
    private static int GetSimpleHash(string input)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in input)
            {
                hash = hash * 31 + c;
            }
            return Math.Abs(hash);
        }
    }

    /// <summary>
    /// Returns a populated DashboardViewModel for the owner dashboard.
    /// </summary>
    public static DashboardViewModel GetDashboardViewModel()
    {
        return new DashboardViewModel
        {
            TodayBookings = 12,
            PendingCount = 3,
            Revenue = 2_340_000,
            ActiveCourts = "4/4",
            RevenueData = RevenueData,
            RecentBookings = OwnerBookings
        };
    }

    /// <summary>
    /// Returns a populated AdminViewModel for the admin dashboard.
    /// </summary>
    public static AdminViewModel GetAdminViewModel()
    {
        return new AdminViewModel
        {
            TotalOwners = 24,
            TotalCustomers = 1_250,
            TotalVenues = 18,
            MonthlyBookings = 3_420,
            PendingVenues = PendingVenues,
            RecentUsers = RecentUsers
        };
    }
}
