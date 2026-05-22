import { useMemo, useState } from "react";
import { Card } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { ArrowLeft, Clock, MapPin, Phone, Star, AlertCircle, CheckCircle2 } from "lucide-react";
import { buildSlotMatrix, formatVND, timeSlots, venues } from "./data";
import { CourtLines } from "./Shared";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "./ui/dialog";
import { toast } from "sonner";

const days = (() => {
  const arr: { key: string; label: string; sub: string }[] = [];
  const base = new Date(2026, 4, 20);
  const dayNames = ["CN", "T2", "T3", "T4", "T5", "T6", "T7"];
  for (let i = 0; i < 7; i++) {
    const d = new Date(base);
    d.setDate(base.getDate() + i);
    arr.push({
      key: d.toISOString().slice(0, 10),
      label: i === 0 ? "Hôm nay" : dayNames[d.getDay()],
      sub: `${String(d.getDate()).padStart(2, "0")}/${String(d.getMonth() + 1).padStart(2, "0")}`,
    });
  }
  return arr;
})();

export function VenueDetailPage({ venueId, onBack }: { venueId: string; onBack: () => void }) {
  const venue = venues.find((v) => v.id === venueId) || venues[0];
  const [dateKey, setDateKey] = useState(days[0].key);
  const [selected, setSelected] = useState<{ courtId: string; time: string } | null>(null);
  const [confirmOpen, setConfirmOpen] = useState(false);

  const matrix = useMemo(() => buildSlotMatrix(venue, dateKey), [venue, dateKey]);
  const selectedCourt = selected ? venue.courts.find((c) => c.id === selected.courtId) : null;
  const selectedDay = days.find((d) => d.key === dateKey)!;

  function confirmBooking() {
    setConfirmOpen(false);
    toast.success("Đã gửi yêu cầu đặt sân. Chủ sân sẽ xác nhận trong ít phút.");
    setSelected(null);
  }

  return (
    <main className="max-w-7xl mx-auto px-4 sm:px-6 py-6 pb-32 lg:pb-10">
      <button onClick={onBack} className="text-sm text-slate-600 hover:text-emerald-700 inline-flex items-center gap-1 mb-4">
        <ArrowLeft className="w-4 h-4" /> Quay lại danh sách
      </button>

      {/* Header */}
      <div className="relative rounded-xl overflow-hidden border border-slate-200 bg-emerald-800 text-emerald-300/30 h-48 mb-6">
        <CourtLines className="absolute inset-0 w-full h-full" />
        <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/20 to-transparent" />
        <div className="absolute bottom-0 left-0 right-0 p-5 text-white">
          <div className="flex flex-wrap items-end justify-between gap-3">
            <div>
              <div className="flex items-center gap-2 mb-1">
                {venue.hasSlotsToday && <Badge className="bg-amber-400 text-amber-900 hover:bg-amber-400">Còn slot hôm nay</Badge>}
                <Badge variant="outline" className="border-white/30 text-white bg-white/10">Đã xác minh</Badge>
              </div>
              <h1 className="text-white text-2xl">{venue.name}</h1>
              <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-sm text-white/85 mt-1">
                <span className="flex items-center gap-1"><MapPin className="w-3.5 h-3.5" /> {venue.address}</span>
                <span className="flex items-center gap-1"><Clock className="w-3.5 h-3.5" /> {venue.openHours}</span>
                <span className="flex items-center gap-1"><Star className="w-3.5 h-3.5 fill-amber-300 text-amber-300" /> {venue.rating} ({venue.reviews})</span>
              </div>
            </div>
            <Button variant="outline" className="border-white/40 bg-white/10 text-white hover:bg-white/20">
              <Phone className="w-4 h-4 mr-2" /> Gọi chủ sân
            </Button>
          </div>
        </div>
      </div>

      <div className="grid lg:grid-cols-[1fr_340px] gap-6">
        <div className="space-y-6 min-w-0">
          {/* Description */}
          <Card className="p-5 border-slate-200">
            <div className="text-slate-900 mb-2">Giới thiệu</div>
            <p className="text-sm text-slate-600 leading-relaxed">{venue.description}</p>
            <div className="grid sm:grid-cols-2 gap-3 mt-4">
              {venue.courts.map((c) => (
                <div key={c.id} className="flex items-center justify-between border border-slate-200 rounded-lg px-3 py-2">
                  <div>
                    <div className="text-sm text-slate-900">{c.name}</div>
                    {c.note && <div className="text-xs text-slate-500">{c.note}</div>}
                  </div>
                  <div className="text-sm text-slate-700">{formatVND(c.pricePerHour)}<span className="text-xs text-slate-500">/giờ</span></div>
                </div>
              ))}
            </div>
          </Card>

          {/* Date picker */}
          <Card className="p-5 border-slate-200">
            <div className="flex items-center justify-between mb-3">
              <div className="text-slate-900">Chọn ngày</div>
              <div className="text-xs text-slate-500">Bấm vào ô để chọn khung giờ</div>
            </div>
            <div className="flex gap-2 overflow-x-auto pb-1">
              {days.map((d) => (
                <button
                  key={d.key}
                  onClick={() => { setDateKey(d.key); setSelected(null); }}
                  className={`shrink-0 px-3 py-2 rounded-lg border text-sm min-w-[72px] ${
                    dateKey === d.key
                      ? "bg-emerald-700 border-emerald-700 text-white"
                      : "bg-white border-slate-200 text-slate-700 hover:border-emerald-400"
                  }`}
                >
                  <div className="text-xs opacity-80">{d.label}</div>
                  <div>{d.sub}</div>
                </button>
              ))}
            </div>

            {/* Slot grid */}
            <div className="mt-5 overflow-x-auto">
              <table className="w-full min-w-[520px] border-separate border-spacing-1">
                <thead>
                  <tr>
                    <th className="text-left text-xs text-slate-500 font-normal px-2 py-1 w-20">Giờ</th>
                    {venue.courts.map((c) => (
                      <th key={c.id} className="text-xs text-slate-600 font-normal px-2 py-1">{c.name}</th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {timeSlots.map((t) => (
                    <tr key={t}>
                      <td className="text-xs text-slate-500 px-2 py-1">{t}</td>
                      {venue.courts.map((c) => {
                        const status = matrix[c.id][t];
                        const isSel = selected?.courtId === c.id && selected?.time === t;
                        const base = "w-full py-2 rounded-md text-xs border transition";
                        let cls = "";
                        if (status === "booked") cls = "bg-slate-100 text-slate-400 border-slate-200 cursor-not-allowed";
                        else if (status === "pending") cls = "bg-amber-50 text-amber-800 border-amber-200 hover:border-amber-400";
                        else cls = "bg-emerald-50 text-emerald-800 border-emerald-200 hover:border-emerald-500 hover:bg-emerald-100";
                        if (isSel) cls = "bg-emerald-700 text-white border-emerald-700";
                        return (
                          <td key={c.id} className="p-0">
                            <button
                              disabled={status === "booked"}
                              onClick={() => setSelected({ courtId: c.id, time: t })}
                              className={`${base} ${cls}`}
                            >
                              {status === "booked" ? "Đã đặt" : status === "pending" ? "Chờ" : "Trống"}
                            </button>
                          </td>
                        );
                      })}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="flex flex-wrap gap-3 mt-4 text-xs text-slate-500">
              <span className="inline-flex items-center gap-1"><span className="w-3 h-3 rounded bg-emerald-100 border border-emerald-300" /> Còn trống</span>
              <span className="inline-flex items-center gap-1"><span className="w-3 h-3 rounded bg-amber-100 border border-amber-300" /> Chờ xác nhận</span>
              <span className="inline-flex items-center gap-1"><span className="w-3 h-3 rounded bg-slate-100 border border-slate-300" /> Đã đặt</span>
            </div>
          </Card>
        </div>

        {/* Confirmation panel (desktop) */}
        <aside className="hidden lg:block">
          <div className="sticky top-20">
            <Card className="p-5 border-slate-200">
              <div className="text-slate-900 mb-1">Xác nhận đặt sân</div>
              <p className="text-xs text-slate-500 mb-4">Chọn một khung giờ để xem chi phí.</p>
              {selected && selectedCourt ? (
                <>
                  <div className="space-y-2 text-sm">
                    <Row label="Cụm sân" value={venue.name} />
                    <Row label="Sân con" value={selectedCourt.name} />
                    <Row label="Ngày" value={`${selectedDay.label} · ${selectedDay.sub}`} />
                    <Row label="Khung giờ" value={`${selected.time} – ${incHour(selected.time)}`} />
                  </div>
                  <div className="border-t border-dashed border-slate-200 my-4" />
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-slate-600">Tổng cộng</span>
                    <span className="text-emerald-800 text-lg">{formatVND(selectedCourt.pricePerHour)}</span>
                  </div>
                  <Button className="w-full mt-4 bg-emerald-700 hover:bg-emerald-800" onClick={() => setConfirmOpen(true)}>
                    Xác nhận đặt sân
                  </Button>
                  <div className="mt-3 text-xs text-slate-500 flex items-start gap-1.5">
                    <AlertCircle className="w-3.5 h-3.5 mt-0.5 shrink-0" />
                    Khung giờ này sẽ được giữ chỗ 10 phút khi bạn xác nhận.
                  </div>
                </>
              ) : (
                <div className="border border-dashed border-slate-200 rounded-lg p-6 text-center text-sm text-slate-500">
                  Chưa có khung giờ nào được chọn
                </div>
              )}
            </Card>
          </div>
        </aside>
      </div>

      {/* Mobile sticky bar */}
      {selected && selectedCourt && (
        <div className="lg:hidden fixed bottom-0 left-0 right-0 bg-white border-t border-slate-200 px-4 py-3 shadow-lg z-20">
          <div className="flex items-center justify-between gap-3">
            <div className="min-w-0">
              <div className="text-xs text-slate-500 truncate">{selectedCourt.name} · {selected.time} – {incHour(selected.time)}</div>
              <div className="text-emerald-800">{formatVND(selectedCourt.pricePerHour)}</div>
            </div>
            <Button className="bg-emerald-700 hover:bg-emerald-800" onClick={() => setConfirmOpen(true)}>Đặt sân</Button>
          </div>
        </div>
      )}

      <Dialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Xác nhận đặt sân</DialogTitle>
            <DialogDescription>Kiểm tra lại thông tin trước khi gửi yêu cầu đến chủ sân.</DialogDescription>
          </DialogHeader>
          <div className="text-sm text-slate-600">
            Bạn đặt <strong className="text-slate-900">{selectedCourt?.name}</strong> tại <strong className="text-slate-900">{venue.name}</strong> vào lúc <strong className="text-slate-900">{selected?.time}</strong>, ngày <strong className="text-slate-900">{selectedDay.sub}</strong>.
          </div>
          <div className="rounded-lg bg-emerald-50 border border-emerald-200 p-3 flex items-start gap-2 text-sm text-emerald-900">
            <CheckCircle2 className="w-4 h-4 mt-0.5" /> Chủ sân thường xác nhận trong vòng 5–10 phút.
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmOpen(false)}>Hủy</Button>
            <Button className="bg-emerald-700 hover:bg-emerald-800" onClick={confirmBooking}>Xác nhận</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </main>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-3">
      <span className="text-slate-500">{label}</span>
      <span className="text-slate-900 text-right">{value}</span>
    </div>
  );
}

function incHour(t: string) {
  const h = parseInt(t.split(":")[0], 10) + 1;
  return `${String(h).padStart(2, "0")}:00`;
}
