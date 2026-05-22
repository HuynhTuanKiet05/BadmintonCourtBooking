export type SlotStatus = "available" | "booked" | "pending";
export type BookingStatus = "confirmed" | "pending" | "completed" | "cancelled";

export interface Court {
  id: string;
  name: string;
  pricePerHour: number;
  note?: string;
}

export interface Venue {
  id: string;
  name: string;
  district: string;
  address: string;
  openHours: string;
  priceFrom: number;
  courts: Court[];
  highlight?: string;
  rating: number;
  reviews: number;
  responseFast?: boolean;
  hasSlotsToday?: boolean;
  description: string;
}

export const venues: Venue[] = [
  {
    id: "v1",
    name: "Sân Cầu Lông Phú Thọ",
    district: "Quận 11, TP.HCM",
    address: "176 Lữ Gia, P.15, Q.11",
    openHours: "06:00 – 22:00",
    priceFrom: 90000,
    rating: 4.7,
    reviews: 218,
    responseFast: true,
    hasSlotsToday: true,
    highlight: "Còn 4 khung giờ đẹp hôm nay",
    description:
      "Cụm sân lâu đời tại Phú Thọ, sàn gỗ, đèn LED chống chói, phù hợp đánh đôi sau giờ làm.",
    courts: [
      { id: "c1", name: "Sân 1", pricePerHour: 90000 },
      { id: "c2", name: "Sân 2", pricePerHour: 90000 },
      { id: "c3", name: "Sân 3", pricePerHour: 110000, note: "Sàn mới" },
      { id: "c4", name: "Sân VIP", pricePerHour: 150000, note: "Điều hòa" },
    ],
  },
  {
    id: "v2",
    name: "Tân Bình Badminton Hub",
    district: "Quận Tân Bình, TP.HCM",
    address: "23 Bàu Cát 2, P.14, Tân Bình",
    openHours: "05:30 – 23:00",
    priceFrom: 100000,
    rating: 4.5,
    reviews: 142,
    hasSlotsToday: true,
    highlight: "Mới mở – ưu đãi khung sáng",
    description: "6 sân tiêu chuẩn, có quầy nước và khu thay đồ riêng.",
    courts: [
      { id: "c1", name: "Sân A1", pricePerHour: 100000 },
      { id: "c2", name: "Sân A2", pricePerHour: 100000 },
      { id: "c3", name: "Sân A3", pricePerHour: 100000 },
      { id: "c4", name: "Sân B1", pricePerHour: 120000 },
    ],
  },
  {
    id: "v3",
    name: "Sân Cầu Lông Bình Thạnh",
    district: "Quận Bình Thạnh, TP.HCM",
    address: "85 Nơ Trang Long, P.13",
    openHours: "06:00 – 22:30",
    priceFrom: 80000,
    rating: 4.3,
    reviews: 96,
    responseFast: true,
    description: "Sân cộng đồng, giá tốt cho nhóm bạn cuối tuần.",
    courts: [
      { id: "c1", name: "Sân 1", pricePerHour: 80000 },
      { id: "c2", name: "Sân 2", pricePerHour: 80000 },
      { id: "c3", name: "Sân 3", pricePerHour: 95000 },
    ],
  },
  {
    id: "v4",
    name: "Quận 10 Sport Center",
    district: "Quận 10, TP.HCM",
    address: "12 Thành Thái, P.14, Q.10",
    openHours: "06:00 – 22:00",
    priceFrom: 120000,
    rating: 4.8,
    reviews: 305,
    hasSlotsToday: true,
    highlight: "Chủ sân phản hồi nhanh",
    responseFast: true,
    description: "Trung tâm thể thao đa năng, có chỗ gửi xe rộng.",
    courts: [
      { id: "c1", name: "Sân 1", pricePerHour: 120000 },
      { id: "c2", name: "Sân 2", pricePerHour: 120000 },
      { id: "c3", name: "Sân VIP", pricePerHour: 180000, note: "Sàn thi đấu" },
    ],
  },
  {
    id: "v5",
    name: "Sân Cầu Lông Gò Vấp",
    district: "Quận Gò Vấp, TP.HCM",
    address: "210 Quang Trung, P.10",
    openHours: "06:30 – 22:00",
    priceFrom: 75000,
    rating: 4.1,
    reviews: 64,
    description: "Sân bình dân, đông khách buổi tối.",
    courts: [
      { id: "c1", name: "Sân 1", pricePerHour: 75000 },
      { id: "c2", name: "Sân 2", pricePerHour: 75000 },
    ],
  },
  {
    id: "v6",
    name: "Thủ Đức Smash Arena",
    district: "TP. Thủ Đức",
    address: "45 Võ Văn Ngân, P. Linh Chiểu",
    openHours: "06:00 – 23:00",
    priceFrom: 110000,
    rating: 4.6,
    reviews: 188,
    hasSlotsToday: true,
    description: "Mái cao 9m, gió tốt, phù hợp giải phong trào.",
    courts: [
      { id: "c1", name: "Sân 1", pricePerHour: 110000 },
      { id: "c2", name: "Sân 2", pricePerHour: 110000 },
      { id: "c3", name: "Sân 3", pricePerHour: 110000 },
      { id: "c4", name: "Sân 4", pricePerHour: 130000 },
    ],
  },
];

export const timeSlots = [
  "06:00", "07:00", "08:00", "09:00", "10:00", "11:00",
  "14:00", "15:00", "16:00", "17:00", "18:00", "19:00",
  "20:00", "21:00",
];

export function buildSlotMatrix(venue: Venue, dateKey: string) {
  const seedBase = dateKey.split("").reduce((a, c) => a + c.charCodeAt(0), 0);
  const matrix: Record<string, Record<string, SlotStatus>> = {};
  venue.courts.forEach((c, ci) => {
    matrix[c.id] = {};
    timeSlots.forEach((t, ti) => {
      const v = (seedBase + ci * 7 + ti * 13) % 10;
      let s: SlotStatus = "available";
      if (v < 3) s = "booked";
      else if (v === 3) s = "pending";
      // Peak hours more likely booked
      if ((t === "18:00" || t === "19:00" || t === "20:00") && v < 6) s = "booked";
      matrix[c.id][t] = s;
    });
  });
  return matrix;
}

export const myBookings = [
  {
    id: "b1",
    venue: "Sân Cầu Lông Phú Thọ",
    court: "Sân VIP",
    date: "Thứ 5, 21/05/2026",
    time: "19:00 – 20:00",
    total: 150000,
    status: "confirmed" as BookingStatus,
    when: "upcoming",
  },
  {
    id: "b2",
    venue: "Quận 10 Sport Center",
    court: "Sân 2",
    date: "CN, 24/05/2026",
    time: "17:00 – 19:00",
    total: 240000,
    status: "pending" as BookingStatus,
    when: "upcoming",
  },
  {
    id: "b3",
    venue: "Thủ Đức Smash Arena",
    court: "Sân 3",
    date: "Thứ 7, 10/05/2026",
    time: "18:00 – 19:00",
    total: 110000,
    status: "completed" as BookingStatus,
    when: "completed",
  },
  {
    id: "b4",
    venue: "Tân Bình Badminton Hub",
    court: "Sân A1",
    date: "Thứ 3, 06/05/2026",
    time: "20:00 – 21:00",
    total: 100000,
    status: "completed" as BookingStatus,
    when: "completed",
  },
  {
    id: "b5",
    venue: "Sân Cầu Lông Bình Thạnh",
    court: "Sân 1",
    date: "Thứ 2, 28/04/2026",
    time: "07:00 – 08:00",
    total: 80000,
    status: "cancelled" as BookingStatus,
    when: "cancelled",
  },
];

export const ownerBookings = [
  { id: "ob1", customer: "Nguyễn Minh Khoa", phone: "0905***221", court: "Sân 1", time: "Hôm nay · 18:00", total: 90000, status: "pending" as BookingStatus },
  { id: "ob2", customer: "Trần Phương Linh", phone: "0987***104", court: "Sân VIP", time: "Hôm nay · 19:00", total: 150000, status: "confirmed" as BookingStatus },
  { id: "ob3", customer: "Lê Quốc Bảo", phone: "0912***876", court: "Sân 2", time: "Hôm nay · 20:00", total: 90000, status: "pending" as BookingStatus },
  { id: "ob4", customer: "Phạm Hồng Nhung", phone: "0934***012", court: "Sân 3", time: "Mai · 07:00", total: 110000, status: "confirmed" as BookingStatus },
  { id: "ob5", customer: "Đặng Tuấn Anh", phone: "0976***455", court: "Sân 1", time: "Mai · 17:00", total: 90000, status: "cancelled" as BookingStatus },
];

export const revenueData = [
  { day: "T2", value: 1.8 },
  { day: "T3", value: 2.1 },
  { day: "T4", value: 2.6 },
  { day: "T5", value: 2.2 },
  { day: "T6", value: 3.4 },
  { day: "T7", value: 4.1 },
  { day: "CN", value: 3.7 },
];

export const pendingVenues = [
  { id: "pv1", name: "Sân Cầu Lông An Phú", owner: "Trần Văn Hùng", district: "Quận 2", courts: 4, submitted: "18/05/2026" },
  { id: "pv2", name: "Bình Tân Sport Hub", owner: "Lê Thị Mai", district: "Bình Tân", courts: 6, submitted: "17/05/2026" },
  { id: "pv3", name: "Sân Hoàng Hoa Thám", owner: "Phạm Quốc Việt", district: "Tân Bình", courts: 3, submitted: "16/05/2026" },
];

export function formatVND(v: number) {
  return v.toLocaleString("vi-VN") + "₫";
}
