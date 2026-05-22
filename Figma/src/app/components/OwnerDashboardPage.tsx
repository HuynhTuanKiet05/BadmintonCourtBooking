import { Card } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Building2, CalendarDays, ChevronRight, Layers, TrendingUp, Wallet } from "lucide-react";
import { formatVND, ownerBookings, revenueData } from "./data";
import { StatusBadge } from "./Shared";
import { ViewKey } from "./Header";

const sidebarItems = [
  { key: "owner" as ViewKey, label: "Tổng quan", icon: TrendingUp },
  { key: "ownerVenues" as ViewKey, label: "Cụm sân của tôi", icon: Building2 },
  { key: "ownerVenues" as ViewKey, label: "Sân con", icon: Layers },
  { key: "owner" as ViewKey, label: "Booking", icon: CalendarDays },
  { key: "owner" as ViewKey, label: "Hồ sơ chủ sân", icon: Wallet },
];

export function OwnerDashboardPage({ onNavigate }: { onNavigate: (v: ViewKey) => void }) {
  return (
    <main className="max-w-7xl mx-auto px-4 sm:px-6 py-6">
      <div className="grid lg:grid-cols-[220px_1fr] gap-6">
        <aside className="hidden lg:block">
          <Card className="p-2 border-slate-200 sticky top-20">
            {sidebarItems.map((item, i) => (
              <button
                key={i}
                onClick={() => onNavigate(item.key)}
                className={`w-full flex items-center gap-2 px-3 py-2 rounded-md text-sm text-left ${
                  i === 0 ? "bg-emerald-50 text-emerald-800" : "text-slate-700 hover:bg-slate-50"
                }`}
              >
                <item.icon className="w-4 h-4" /> {item.label}
              </button>
            ))}
          </Card>
        </aside>

        <section>
          <div className="flex flex-wrap items-end justify-between gap-3 mb-5">
            <div>
              <h1 className="text-slate-900">Chào, anh Hoàng Nam</h1>
              <p className="text-sm text-slate-600 mt-1">Tổng quan hoạt động của cụm sân Phú Thọ — hôm nay 20/05/2026.</p>
            </div>
            <Button className="bg-emerald-700 hover:bg-emerald-800" onClick={() => onNavigate("ownerVenues")}>
              Quản lý cụm sân
            </Button>
          </div>

          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-3 mb-5">
            <Stat label="Booking hôm nay" value="23" delta="+12%" icon={CalendarDays} tone="emerald" />
            <Stat label="Chờ xác nhận" value="4" delta="cần xử lý" icon={CalendarDays} tone="amber" />
            <Stat label="Doanh thu dự kiến" value={formatVND(2400000)} delta="+8%" icon={Wallet} tone="emerald" />
            <Stat label="Sân đang hoạt động" value="4 / 4" delta="ổn định" icon={Layers} tone="slate" />
          </div>

          <div className="grid lg:grid-cols-[1fr_360px] gap-5">
            <Card className="p-5 border-slate-200">
              <div className="flex items-center justify-between mb-3">
                <div className="text-slate-900">Doanh thu 7 ngày qua</div>
                <Badge variant="outline" className="border-slate-200 text-slate-500">Triệu đồng</Badge>
              </div>
              <div className="h-56 flex items-end gap-3 px-2">
                {revenueData.map((d) => {
                  const max = Math.max(...revenueData.map((x) => x.value));
                  const h = (d.value / max) * 100;
                  return (
                    <div key={d.day} className="flex-1 flex flex-col items-center gap-2 group">
                      <div className="text-[11px] text-slate-500 opacity-0 group-hover:opacity-100 transition">{d.value}tr</div>
                      <div className="w-full bg-slate-100 rounded-t-md relative" style={{ height: "180px" }}>
                        <div
                          className="absolute bottom-0 left-0 right-0 bg-emerald-700 rounded-t-md transition-all group-hover:bg-emerald-600"
                          style={{ height: `${h}%` }}
                        />
                      </div>
                      <div className="text-xs text-slate-500">{d.day}</div>
                    </div>
                  );
                })}
              </div>
            </Card>

            <Card className="p-5 border-slate-200">
              <div className="text-slate-900 mb-3">Phân bổ slot hôm nay</div>
              <div className="space-y-3 text-sm">
                {[
                  { l: "Đã đặt", v: 18, pct: 78, c: "bg-emerald-600" },
                  { l: "Chờ xác nhận", v: 4, pct: 17, c: "bg-amber-500" },
                  { l: "Còn trống", v: 1, pct: 5, c: "bg-slate-300" },
                ].map((r) => (
                  <div key={r.l}>
                    <div className="flex items-center justify-between mb-1 text-xs">
                      <span className="text-slate-600">{r.l}</span>
                      <span className="text-slate-900">{r.v} ({r.pct}%)</span>
                    </div>
                    <div className="h-2 rounded-full bg-slate-100 overflow-hidden">
                      <div className={`h-full ${r.c}`} style={{ width: `${r.pct}%` }} />
                    </div>
                  </div>
                ))}
              </div>
              <div className="mt-5 rounded-lg bg-emerald-50 border border-emerald-200 p-3 text-xs text-emerald-900">
                Mẹo: bật ưu đãi khung 14:00–16:00 để tăng tỉ lệ lấp đầy ngày trong tuần.
              </div>
            </Card>
          </div>

          <Card className="mt-5 border-slate-200">
            <div className="p-5 flex items-center justify-between">
              <div>
                <div className="text-slate-900">Booking gần đây</div>
                <div className="text-xs text-slate-500 mt-0.5">Xác nhận sớm để giữ chân khách quay lại.</div>
              </div>
              <Button variant="ghost" className="text-emerald-700">Xem tất cả <ChevronRight className="w-4 h-4 ml-1" /></Button>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-slate-50 text-slate-500 text-xs">
                  <tr>
                    <th className="text-left font-normal px-5 py-2.5">Khách hàng</th>
                    <th className="text-left font-normal px-5 py-2.5">Sân</th>
                    <th className="text-left font-normal px-5 py-2.5">Thời gian</th>
                    <th className="text-left font-normal px-5 py-2.5">Tổng tiền</th>
                    <th className="text-left font-normal px-5 py-2.5">Trạng thái</th>
                    <th className="text-right font-normal px-5 py-2.5">Hành động</th>
                  </tr>
                </thead>
                <tbody>
                  {ownerBookings.map((b) => (
                    <tr key={b.id} className="border-t border-slate-100">
                      <td className="px-5 py-3">
                        <div className="text-slate-900">{b.customer}</div>
                        <div className="text-xs text-slate-500">{b.phone}</div>
                      </td>
                      <td className="px-5 py-3 text-slate-700">{b.court}</td>
                      <td className="px-5 py-3 text-slate-700">{b.time}</td>
                      <td className="px-5 py-3 text-slate-700">{formatVND(b.total)}</td>
                      <td className="px-5 py-3"><StatusBadge status={b.status} /></td>
                      <td className="px-5 py-3 text-right">
                        {b.status === "pending" ? (
                          <div className="flex justify-end gap-2">
                            <Button size="sm" variant="outline" className="border-rose-200 text-rose-700 hover:bg-rose-50">Từ chối</Button>
                            <Button size="sm" className="bg-emerald-700 hover:bg-emerald-800">Xác nhận</Button>
                          </div>
                        ) : (
                          <Button size="sm" variant="ghost" className="text-slate-500">Chi tiết</Button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
        </section>
      </div>
    </main>
  );
}

function Stat({
  label, value, delta, icon: Icon, tone,
}: { label: string; value: string; delta: string; icon: any; tone: "emerald" | "amber" | "slate" }) {
  const toneCls = {
    emerald: "bg-emerald-50 text-emerald-700",
    amber: "bg-amber-50 text-amber-700",
    slate: "bg-slate-100 text-slate-600",
  }[tone];
  return (
    <Card className="p-4 border-slate-200">
      <div className="flex items-start justify-between">
        <div>
          <div className="text-xs text-slate-500">{label}</div>
          <div className="text-slate-900 text-xl mt-1">{value}</div>
        </div>
        <div className={`w-9 h-9 rounded-lg grid place-items-center ${toneCls}`}>
          <Icon className="w-4 h-4" />
        </div>
      </div>
      <div className="text-xs text-slate-500 mt-2">{delta}</div>
    </Card>
  );
}
