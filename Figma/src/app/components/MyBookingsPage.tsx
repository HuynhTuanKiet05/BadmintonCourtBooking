import { Card } from "./ui/card";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "./ui/tabs";
import { Button } from "./ui/button";
import { Calendar, Clock, MapPin, Search } from "lucide-react";
import { formatVND, myBookings } from "./data";
import { StatusBadge } from "./Shared";
import { ViewKey } from "./Header";

export function MyBookingsPage({ onNavigate }: { onNavigate: (v: ViewKey) => void }) {
  const groups = {
    upcoming: myBookings.filter((b) => b.when === "upcoming"),
    completed: myBookings.filter((b) => b.when === "completed"),
    cancelled: myBookings.filter((b) => b.when === "cancelled"),
  };

  return (
    <main className="max-w-5xl mx-auto px-4 sm:px-6 py-8">
      <div className="mb-6">
        <h1 className="text-slate-900">Lịch đặt sân của tôi</h1>
        <p className="text-sm text-slate-600 mt-1">Quản lý các lịch đặt sắp tới và lịch sử đã chơi.</p>
      </div>

      <Tabs defaultValue="upcoming">
        <TabsList className="bg-slate-100">
          <TabsTrigger value="upcoming">Sắp tới ({groups.upcoming.length})</TabsTrigger>
          <TabsTrigger value="completed">Đã hoàn thành ({groups.completed.length})</TabsTrigger>
          <TabsTrigger value="cancelled">Đã hủy ({groups.cancelled.length})</TabsTrigger>
        </TabsList>

        {(["upcoming", "completed", "cancelled"] as const).map((key) => (
          <TabsContent key={key} value={key} className="mt-5">
            {groups[key].length === 0 ? (
              <Card className="p-12 text-center border-dashed border-slate-300">
                <div className="w-14 h-14 mx-auto rounded-full bg-emerald-50 grid place-items-center text-emerald-700">
                  <Calendar className="w-7 h-7" />
                </div>
                <div className="mt-3 text-slate-900">Chưa có lịch đặt nào</div>
                <p className="text-sm text-slate-500 mt-1">Tìm cụm sân gần bạn và đặt khung giờ phù hợp.</p>
                <Button className="mt-4 bg-emerald-700 hover:bg-emerald-800" onClick={() => onNavigate("find")}>
                  <Search className="w-4 h-4 mr-2" /> Tìm sân ngay
                </Button>
              </Card>
            ) : (
              <div className="grid gap-3">
                {groups[key].map((b) => (
                  <Card key={b.id} className="p-4 border-slate-200">
                    <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-2 flex-wrap">
                          <div className="text-slate-900">{b.venue}</div>
                          <StatusBadge status={b.status} />
                        </div>
                        <div className="text-sm text-slate-600 mt-1">{b.court}</div>
                        <div className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-slate-500 mt-2">
                          <span className="inline-flex items-center gap-1"><Calendar className="w-3 h-3" /> {b.date}</span>
                          <span className="inline-flex items-center gap-1"><Clock className="w-3 h-3" /> {b.time}</span>
                          <span className="inline-flex items-center gap-1"><MapPin className="w-3 h-3" /> TP.HCM</span>
                        </div>
                      </div>
                      <div className="flex md:flex-col md:items-end items-center justify-between gap-3 md:gap-2 md:text-right">
                        <div>
                          <div className="text-xs text-slate-500">Tổng tiền</div>
                          <div className="text-slate-900">{formatVND(b.total)}</div>
                        </div>
                        <div className="flex gap-2">
                          {key === "upcoming" && b.status !== "cancelled" && (
                            <Button variant="outline" size="sm" className="border-rose-200 text-rose-700 hover:bg-rose-50">Hủy lịch</Button>
                          )}
                          <Button variant="outline" size="sm">Chi tiết</Button>
                        </div>
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            )}
          </TabsContent>
        ))}
      </Tabs>
    </main>
  );
}
