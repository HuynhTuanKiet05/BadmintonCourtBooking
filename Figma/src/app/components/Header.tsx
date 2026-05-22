import { Button } from "./ui/button";
import { Avatar, AvatarFallback } from "./ui/avatar";
import { Menu, Shield } from "lucide-react";
import { Sheet, SheetContent, SheetTrigger } from "./ui/sheet";

export type ViewKey =
  | "landing"
  | "find"
  | "venue"
  | "bookings"
  | "owner"
  | "ownerVenues"
  | "admin"
  | "login"
  | "register"
  | "profile";

const customerLinks: { key: ViewKey; label: string }[] = [
  { key: "landing", label: "Trang chủ" },
  { key: "find", label: "Tìm sân" },
  { key: "bookings", label: "Lịch của tôi" },
];

export function Header({
  view,
  onNavigate,
}: {
  view: ViewKey;
  onNavigate: (v: ViewKey) => void;
}) {
  const isOwnerScope = view === "owner" || view === "ownerVenues";
  const isAdmin = view === "admin";

  return (
    <header className="sticky top-0 z-30 bg-white/90 backdrop-blur border-b border-slate-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 h-16 flex items-center justify-between gap-4">
        <button onClick={() => onNavigate("landing")} className="flex items-center gap-2">
          <div className="w-9 h-9 rounded-lg bg-emerald-700 text-white grid place-items-center">
            <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M5 19l9-9" /><circle cx="16" cy="7" r="3" /><path d="M3 21l4-1 1-3" />
            </svg>
          </div>
          <div className="leading-tight text-left">
            <div className="text-slate-900">CourtBook</div>
            <div className="text-[11px] text-slate-500 -mt-0.5">Đặt sân cầu lông</div>
          </div>
        </button>

        <nav className="hidden md:flex items-center gap-1">
          {customerLinks.map((l) => (
            <button
              key={l.key}
              onClick={() => onNavigate(l.key)}
              className={`px-3 py-2 rounded-md text-sm transition-colors ${
                view === l.key || (l.key === "find" && view === "venue")
                  ? "text-emerald-800 bg-emerald-50"
                  : "text-slate-600 hover:text-slate-900 hover:bg-slate-50"
              }`}
            >
              {l.label}
            </button>
          ))}
          <button
            onClick={() => onNavigate("owner")}
            className={`px-3 py-2 rounded-md text-sm transition-colors ${
              isOwnerScope ? "text-emerald-800 bg-emerald-50" : "text-slate-600 hover:text-slate-900 hover:bg-slate-50"
            }`}
          >
            Dành cho chủ sân
          </button>
          <button
            onClick={() => onNavigate("admin")}
            className={`px-3 py-2 rounded-md text-sm transition-colors flex items-center gap-1 ${
              isAdmin ? "text-emerald-800 bg-emerald-50" : "text-slate-500 hover:text-slate-900 hover:bg-slate-50"
            }`}
          >
            <Shield className="w-3.5 h-3.5" />
            Admin
          </button>
        </nav>

        <div className="hidden md:flex items-center gap-2">
          <Button variant="ghost" className="text-slate-700" onClick={() => onNavigate("login")}>Đăng nhập</Button>
          <Button className="bg-emerald-700 hover:bg-emerald-800" onClick={() => onNavigate("register")}>Đăng ký</Button>
          {isOwnerScope && (
            <button onClick={() => onNavigate("profile")} className="ml-2 pl-3 border-l border-slate-200 flex items-center gap-2 hover:opacity-80">
              <Avatar className="w-8 h-8"><AvatarFallback className="bg-emerald-100 text-emerald-800">HN</AvatarFallback></Avatar>
              <div className="text-xs leading-tight text-left">
                <div className="text-slate-900">Anh Hoàng Nam</div>
                <div className="text-slate-500">Chủ sân Phú Thọ</div>
              </div>
            </button>
          )}
        </div>

        <Sheet>
          <SheetTrigger className="md:hidden inline-flex items-center justify-center w-9 h-9 rounded-md hover:bg-slate-100 text-slate-700">
            <Menu className="w-5 h-5" />
          </SheetTrigger>
          <SheetContent side="right" className="w-72">
            <div className="flex flex-col gap-1 mt-6">
              {customerLinks.map((l) => (
                <button key={l.key} onClick={() => onNavigate(l.key)} className="text-left px-3 py-2 rounded hover:bg-slate-50">{l.label}</button>
              ))}
              <button onClick={() => onNavigate("owner")} className="text-left px-3 py-2 rounded hover:bg-slate-50">Dành cho chủ sân</button>
              <button onClick={() => onNavigate("admin")} className="text-left px-3 py-2 rounded hover:bg-slate-50">Admin</button>
              <div className="border-t my-3" />
              <Button variant="outline" onClick={() => onNavigate("login")}>Đăng nhập</Button>
              <Button className="bg-emerald-700 hover:bg-emerald-800" onClick={() => onNavigate("register")}>Đăng ký</Button>
              <button onClick={() => onNavigate("profile")} className="text-left px-3 py-2 rounded hover:bg-slate-50 text-sm">Hồ sơ của tôi</button>
            </div>
          </SheetContent>
        </Sheet>
      </div>
    </header>
  );
}
