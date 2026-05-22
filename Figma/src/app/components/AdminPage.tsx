import { Card } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Building2, CalendarCheck, ShieldCheck, Users } from "lucide-react";
import { pendingVenues } from "./data";
import { toast } from "sonner";

export function AdminPage() {
  return (
    <main className="max-w-7xl mx-auto px-4 sm:px-6 py-8">
      <div className="flex items-center gap-2 text-xs text-slate-500 mb-2">
        <ShieldCheck className="w-3.5 h-3.5" /> Platform admin
      </div>
      <h1 className="text-slate-900">Quản trị CourtBook</h1>
      <p className="text-sm text-slate-600 mt-1">Theo dõi sức khỏe nền tảng và duyệt cụm sân mới.</p>

      <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-3 mt-6">
        <AdminStat label="Tổng chủ sân" value="186" icon={Users} />
        <AdminStat label="Tổng khách hàng" value="12.420" icon={Users} />
        <AdminStat label="Tổng cụm sân" value="231" icon={Building2} />
        <AdminStat label="Booking tháng này" value="8.943" icon={CalendarCheck} />
      </div>

      <div className="grid lg:grid-cols-[1.4fr_1fr] gap-5 mt-6">
        <Card className="border-slate-200">
          <div className="p-5 flex items-center justify-between">
            <div>
              <div className="text-slate-900">Cụm sân chờ duyệt</div>
              <div className="text-xs text-slate-500 mt-0.5">Duyệt nhanh để chủ sân có thể nhận booking.</div>
            </div>
            <Badge className="bg-amber-100 text-amber-800 hover:bg-amber-100">{pendingVenues.length} chờ</Badge>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-slate-50 text-xs text-slate-500">
                <tr>
                  <th className="text-left font-normal px-5 py-2.5">Cụm sân</th>
                  <th className="text-left font-normal px-5 py-2.5">Chủ sân</th>
                  <th className="text-left font-normal px-5 py-2.5">Khu vực</th>
                  <th className="text-left font-normal px-5 py-2.5">Sân</th>
                  <th className="text-left font-normal px-5 py-2.5">Ngày gửi</th>
                  <th className="text-right font-normal px-5 py-2.5">Hành động</th>
                </tr>
              </thead>
              <tbody>
                {pendingVenues.map((v) => (
                  <tr key={v.id} className="border-t border-slate-100">
                    <td className="px-5 py-3 text-slate-900">{v.name}</td>
                    <td className="px-5 py-3 text-slate-700">{v.owner}</td>
                    <td className="px-5 py-3 text-slate-700">{v.district}</td>
                    <td className="px-5 py-3 text-slate-700">{v.courts}</td>
                    <td className="px-5 py-3 text-slate-500 text-xs">{v.submitted}</td>
                    <td className="px-5 py-3">
                      <div className="flex justify-end gap-2">
                        <Button size="sm" variant="outline" className="border-rose-200 text-rose-700 hover:bg-rose-50" onClick={() => toast("Đã từ chối cụm sân")}>Từ chối</Button>
                        <Button size="sm" className="bg-emerald-700 hover:bg-emerald-800" onClick={() => toast.success("Đã duyệt cụm sân")}>Duyệt</Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>

        <Card className="border-slate-200">
          <div className="p-5">
            <div className="text-slate-900">Người dùng gần đây</div>
            <div className="text-xs text-slate-500 mt-0.5">Tài khoản mới đăng ký trong 7 ngày.</div>
          </div>
          <div className="divide-y divide-slate-100">
            {[
              { n: "Nguyễn Minh Khoa", r: "Khách hàng", j: "20/05" },
              { n: "Trần Văn Hùng", r: "Chủ sân", j: "19/05" },
              { n: "Lê Quốc Bảo", r: "Khách hàng", j: "19/05" },
              { n: "Phạm Hồng Nhung", r: "Khách hàng", j: "18/05" },
              { n: "Lê Thị Mai", r: "Chủ sân", j: "17/05" },
            ].map((u) => (
              <div key={u.n} className="flex items-center justify-between px-5 py-3 text-sm">
                <div>
                  <div className="text-slate-900">{u.n}</div>
                  <div className="text-xs text-slate-500">Tham gia {u.j}/2026</div>
                </div>
                <Badge variant="outline" className={u.r === "Chủ sân" ? "bg-amber-50 text-amber-800 border-amber-200" : "bg-slate-50 text-slate-700"}>{u.r}</Badge>
              </div>
            ))}
          </div>
        </Card>
      </div>
    </main>
  );
}

function AdminStat({ label, value, icon: Icon }: { label: string; value: string; icon: any }) {
  return (
    <Card className="p-4 border-slate-200">
      <div className="flex items-start justify-between">
        <div>
          <div className="text-xs text-slate-500">{label}</div>
          <div className="text-slate-900 text-xl mt-1">{value}</div>
        </div>
        <div className="w-9 h-9 rounded-lg bg-emerald-50 text-emerald-700 grid place-items-center">
          <Icon className="w-4 h-4" />
        </div>
      </div>
    </Card>
  );
}
