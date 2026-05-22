import { Badge } from "./ui/badge";
import { BookingStatus, SlotStatus } from "./data";

export function StatusBadge({ status }: { status: BookingStatus }) {
  const map: Record<BookingStatus, { label: string; cls: string }> = {
    confirmed: { label: "Đã xác nhận", cls: "bg-emerald-100 text-emerald-800 border-emerald-200" },
    pending: { label: "Chờ xác nhận", cls: "bg-amber-100 text-amber-800 border-amber-200" },
    completed: { label: "Đã hoàn thành", cls: "bg-slate-100 text-slate-700 border-slate-200" },
    cancelled: { label: "Đã hủy", cls: "bg-rose-100 text-rose-700 border-rose-200" },
  };
  const m = map[status];
  return <Badge variant="outline" className={m.cls}>{m.label}</Badge>;
}

export function SlotPill({ status }: { status: SlotStatus }) {
  const cfg: Record<SlotStatus, string> = {
    available: "bg-emerald-50 text-emerald-700 border-emerald-200",
    booked: "bg-slate-100 text-slate-400 border-slate-200",
    pending: "bg-amber-50 text-amber-700 border-amber-200",
  };
  const label: Record<SlotStatus, string> = {
    available: "Trống",
    booked: "Đã đặt",
    pending: "Chờ",
  };
  return (
    <span className={`inline-flex items-center justify-center px-2 py-1 rounded-md border text-xs ${cfg[status]}`}>
      {label[status]}
    </span>
  );
}

export function CourtLines({ className = "" }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 400 240" preserveAspectRatio="none" aria-hidden>
      <rect x="20" y="20" width="360" height="200" fill="none" stroke="currentColor" strokeWidth="1.2" />
      <line x1="200" y1="20" x2="200" y2="220" stroke="currentColor" strokeWidth="1" />
      <line x1="20" y1="120" x2="380" y2="120" stroke="currentColor" strokeWidth="0.8" strokeDasharray="4 4" />
      <rect x="60" y="60" width="280" height="120" fill="none" stroke="currentColor" strokeWidth="0.8" />
      <rect x="60" y="100" width="280" height="40" fill="none" stroke="currentColor" strokeWidth="0.6" />
    </svg>
  );
}
