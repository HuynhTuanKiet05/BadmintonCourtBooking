import { Button } from "./ui/button";
import { Card } from "./ui/card";
import { Badge } from "./ui/badge";
import { ArrowRight, CalendarCheck, Clock, MapPin, Search, ShieldCheck, Sparkles, Users } from "lucide-react";
import { CourtLines } from "./Shared";
import { formatVND, venues } from "./data";
import { ViewKey } from "./Header";

export function LandingPage({ onNavigate }: { onNavigate: (v: ViewKey, id?: string) => void }) {
  const featured = venues.slice(0, 3);
  return (
    <main>
      {/* Hero */}
      <section className="relative overflow-hidden border-b border-slate-200 bg-gradient-to-b from-emerald-50/60 via-white to-white">
        <div className="absolute inset-0 pointer-events-none text-emerald-700/10">
          <CourtLines className="absolute -right-20 top-10 w-[680px] h-[360px]" />
        </div>
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 py-14 lg:py-20 grid lg:grid-cols-2 gap-10 items-center">
          <div>
            <Badge variant="outline" className="bg-white border-emerald-200 text-emerald-800 mb-4">
              <Sparkles className="w-3 h-3 mr-1" /> Nền tảng đặt sân cầu lông
            </Badge>
            <h1 className="text-3xl sm:text-4xl lg:text-[44px] leading-tight text-slate-900 tracking-tight">
              Đặt sân cầu lông dễ hơn,<br />quản lý sân gọn hơn.
            </h1>
            <p className="mt-4 text-slate-600 max-w-xl">
              CourtBook giúp người chơi tìm khung giờ trống chỉ trong vài chạm và giúp chủ sân
              tiếp cận khách hàng online mà không cần đầu tư hệ thống riêng.
            </p>
            <div className="mt-7 flex flex-wrap gap-3">
              <Button size="lg" onClick={() => onNavigate("find")} className="bg-emerald-700 hover:bg-emerald-800">
                <Search className="w-4 h-4 mr-2" /> Tìm sân ngay
              </Button>
              <Button size="lg" variant="outline" onClick={() => onNavigate("owner")} className="border-amber-300 text-amber-800 hover:bg-amber-50">
                Đăng ký làm chủ sân
              </Button>
            </div>
            <div className="mt-8 flex items-center gap-6 text-sm text-slate-600">
              <div className="flex items-center gap-2"><Users className="w-4 h-4 text-emerald-700" /> 12.000+ người chơi</div>
              <div className="flex items-center gap-2"><ShieldCheck className="w-4 h-4 text-emerald-700" /> 180+ cụm sân đã duyệt</div>
            </div>
          </div>

          {/* Mock booking card */}
          <div className="relative">
            <div className="absolute -top-6 -left-6 w-32 h-32 bg-amber-100 rounded-2xl rotate-6" />
            <div className="absolute -bottom-8 -right-4 w-28 h-28 bg-emerald-200/60 rounded-full" />
            <Card className="relative p-5 shadow-xl border-slate-200">
              <div className="flex items-start justify-between">
                <div>
                  <div className="text-slate-900">Sân Cầu Lông Phú Thọ</div>
                  <div className="text-xs text-slate-500 flex items-center gap-1 mt-1">
                    <MapPin className="w-3 h-3" /> Quận 11, TP.HCM
                  </div>
                </div>
                <Badge className="bg-emerald-100 text-emerald-800 hover:bg-emerald-100">Còn 4 slot</Badge>
              </div>
              <div className="mt-4 text-xs text-slate-500">Hôm nay · Thứ 4, 20/05/2026</div>
              <div className="mt-3 grid grid-cols-4 gap-2">
                {["17:00", "18:00", "19:00", "20:00"].map((t, i) => (
                  <button
                    key={t}
                    className={`py-2 rounded-md text-sm border ${
                      i === 1
                        ? "bg-emerald-700 text-white border-emerald-700"
                        : i === 2
                        ? "bg-slate-100 text-slate-400 border-slate-200 cursor-not-allowed"
                        : "bg-white border-slate-200 hover:border-emerald-400"
                    }`}
                  >
                    {t}
                  </button>
                ))}
              </div>
              <div className="mt-4 flex items-center justify-between border-t border-dashed border-slate-200 pt-4">
                <div>
                  <div className="text-xs text-slate-500">Sân VIP · 18:00 – 19:00</div>
                  <div className="text-slate-900">{formatVND(150000)}</div>
                </div>
                <Button className="bg-emerald-700 hover:bg-emerald-800">Đặt sân</Button>
              </div>
            </Card>
          </div>
        </div>
      </section>

      {/* How it works */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 py-16">
        <div className="flex items-end justify-between mb-8">
          <div>
            <h2 className="text-slate-900">Đặt sân chỉ với 3 bước</h2>
            <p className="text-slate-600 mt-1">Không gọi điện, không chờ tin nhắn xác nhận chậm.</p>
          </div>
        </div>
        <div className="grid md:grid-cols-3 gap-4">
          {[
            { icon: Search, title: "Chọn sân gần bạn", desc: "Lọc theo khu vực, giá, giờ mở cửa và trạng thái slot." },
            { icon: Clock, title: "Chọn khung giờ trống", desc: "Bảng slot real-time, biết ngay khung nào đẹp." },
            { icon: CalendarCheck, title: "Xác nhận & đến sân", desc: "Nhận thông báo, chủ sân giữ chỗ cho bạn." },
          ].map((s, i) => (
            <Card key={i} className="p-6 border-slate-200">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-emerald-50 text-emerald-700 grid place-items-center">
                  <s.icon className="w-5 h-5" />
                </div>
                <div className="text-xs text-slate-500">Bước {i + 1}</div>
              </div>
              <div className="mt-4 text-slate-900">{s.title}</div>
              <p className="text-slate-600 text-sm mt-1">{s.desc}</p>
            </Card>
          ))}
        </div>
      </section>

      {/* For owners */}
      <section className="bg-slate-50 border-y border-slate-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 py-16 grid lg:grid-cols-2 gap-10 items-center">
          <div>
            <Badge variant="outline" className="bg-amber-50 border-amber-200 text-amber-800 mb-3">Dành cho chủ sân</Badge>
            <h2 className="text-slate-900 text-2xl">Quản lý cụm sân, theo dõi booking,<br />tăng khách online.</h2>
            <p className="text-slate-600 mt-3 max-w-lg">
              Bảng điều khiển gọn nhẹ cho chủ sân không rành công nghệ. Bạn vẫn nhận booking như
              bình thường, chỉ là đỡ vất vả hơn nhiều.
            </p>
            <div className="mt-6 grid sm:grid-cols-2 gap-3">
              {[
                "Quản lý nhiều cụm sân, nhiều sân con",
                "Xác nhận / hủy booking trong 1 chạm",
                "Báo cáo doanh thu theo tuần",
                "Khách thấy bạn trên trang Tìm sân",
              ].map((t) => (
                <div key={t} className="flex items-start gap-2 text-sm text-slate-700">
                  <div className="w-1.5 h-1.5 rounded-full bg-emerald-600 mt-2" /> {t}
                </div>
              ))}
            </div>
            <Button className="mt-7 bg-emerald-700 hover:bg-emerald-800" onClick={() => onNavigate("owner")}>
              Vào trang chủ sân <ArrowRight className="w-4 h-4 ml-2" />
            </Button>
          </div>
          <Card className="p-5 border-slate-200">
            <div className="flex items-center justify-between">
              <div className="text-slate-900">Tổng quan hôm nay</div>
              <Badge variant="outline" className="border-slate-200 text-slate-500">20/05/2026</Badge>
            </div>
            <div className="grid grid-cols-3 gap-3 mt-4">
              {[
                { l: "Booking", v: "23" },
                { l: "Chờ duyệt", v: "4" },
                { l: "Doanh thu", v: "2.4tr" },
              ].map((s) => (
                <div key={s.l} className="rounded-lg bg-slate-50 border border-slate-200 p-3">
                  <div className="text-xs text-slate-500">{s.l}</div>
                  <div className="text-slate-900 text-xl mt-1">{s.v}</div>
                </div>
              ))}
            </div>
            <div className="mt-4 space-y-2">
              {[
                { t: "Sân VIP · 19:00", c: "Nguyễn Minh Khoa", s: "Chờ" },
                { t: "Sân 2 · 20:00", c: "Lê Quốc Bảo", s: "Đã xác nhận" },
                { t: "Sân 1 · 21:00", c: "Phạm Hồng Nhung", s: "Đã xác nhận" },
              ].map((r) => (
                <div key={r.t} className="flex items-center justify-between text-sm border border-slate-100 rounded-md px-3 py-2">
                  <div>
                    <div className="text-slate-900">{r.t}</div>
                    <div className="text-xs text-slate-500">{r.c}</div>
                  </div>
                  <span className={`text-xs px-2 py-1 rounded ${r.s === "Chờ" ? "bg-amber-100 text-amber-800" : "bg-emerald-100 text-emerald-800"}`}>{r.s}</span>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </section>

      {/* Featured venues */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 py-16">
        <div className="flex items-end justify-between mb-6">
          <div>
            <h2 className="text-slate-900">Sân nổi bật tuần này</h2>
            <p className="text-slate-600 mt-1 text-sm">Các cụm sân được người chơi đánh giá tốt và có nhiều slot trống.</p>
          </div>
          <Button variant="ghost" onClick={() => onNavigate("find")} className="text-emerald-700">
            Xem tất cả <ArrowRight className="w-4 h-4 ml-1" />
          </Button>
        </div>
        <div className="grid md:grid-cols-3 gap-4">
          {featured.map((v) => (
            <Card key={v.id} className="overflow-hidden border-slate-200 hover:shadow-md transition-shadow cursor-pointer" onClick={() => onNavigate("venue", v.id)}>
              <div className="relative h-36 bg-emerald-700 text-emerald-300/40 overflow-hidden">
                <CourtLines className="absolute inset-0 w-full h-full" />
                <div className="absolute top-3 left-3 flex gap-2">
                  {v.hasSlotsToday && <Badge className="bg-amber-400 text-amber-900 hover:bg-amber-400">Còn slot hôm nay</Badge>}
                </div>
              </div>
              <div className="p-4">
                <div className="text-slate-900">{v.name}</div>
                <div className="text-xs text-slate-500 flex items-center gap-1 mt-1"><MapPin className="w-3 h-3" /> {v.district}</div>
                <div className="flex items-center justify-between mt-3">
                  <div className="text-sm text-slate-700">Từ {formatVND(v.priceFrom)}/giờ</div>
                  <div className="text-xs text-slate-500">★ {v.rating} · {v.reviews}</div>
                </div>
              </div>
            </Card>
          ))}
        </div>
      </section>

      <footer className="border-t border-slate-200 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 py-10 grid md:grid-cols-4 gap-6 text-sm">
          <div>
            <div className="text-slate-900">CourtBook</div>
            <p className="text-slate-500 mt-2">Nền tảng kết nối chủ sân và người chơi cầu lông tại Việt Nam.</p>
          </div>
          <div>
            <div className="text-slate-900 mb-2">Người chơi</div>
            <ul className="space-y-1 text-slate-500">
              <li>Tìm sân</li><li>Lịch sử đặt sân</li><li>Hỗ trợ khách hàng</li>
            </ul>
          </div>
          <div>
            <div className="text-slate-900 mb-2">Chủ sân</div>
            <ul className="space-y-1 text-slate-500">
              <li>Đăng ký mở sân</li><li>Bảng giá dịch vụ</li><li>Hướng dẫn quản lý</li>
            </ul>
          </div>
          <div>
            <div className="text-slate-900 mb-2">Liên hệ</div>
            <ul className="space-y-1 text-slate-500">
              <li>support@courtbook.vn</li><li>1900 xxxx</li>
            </ul>
          </div>
        </div>
        <div className="border-t border-slate-100 py-4 text-center text-xs text-slate-400">© 2026 CourtBook Platform</div>
      </footer>
    </main>
  );
}
