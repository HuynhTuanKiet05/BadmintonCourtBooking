import { useState } from "react";
import { Card } from "./ui/card";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { Label } from "./ui/label";
import { Checkbox } from "./ui/checkbox";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "./ui/tabs";
import { Eye, EyeOff, Mail, Phone, Lock, User, Store, Users } from "lucide-react";
import { CourtLines } from "./Shared";
import { ViewKey } from "./Header";
import { toast } from "sonner";

type Mode = "login" | "register";

export function AuthPage({
  mode: initialMode,
  onNavigate,
}: {
  mode: Mode;
  onNavigate: (v: ViewKey) => void;
}) {
  const [mode, setMode] = useState<Mode>(initialMode);
  const [role, setRole] = useState<"player" | "owner">("player");
  const [showPass, setShowPass] = useState(false);

  function submit(e: React.FormEvent) {
    e.preventDefault();
    if (mode === "login") {
      toast.success("Đăng nhập thành công. Chào mừng trở lại!");
      onNavigate(role === "owner" ? "owner" : "find");
    } else {
      toast.success(role === "owner" ? "Đã tạo tài khoản chủ sân. Hãy đăng ký cụm sân đầu tiên!" : "Tạo tài khoản thành công!");
      onNavigate(role === "owner" ? "ownerVenues" : "find");
    }
  }

  return (
    <main className="min-h-[calc(100vh-4rem)] grid lg:grid-cols-2">
      {/* Left visual */}
      <div className="relative hidden lg:flex bg-emerald-800 text-white overflow-hidden">
        <div className="absolute inset-0 text-emerald-400/30">
          <CourtLines className="absolute inset-0 w-full h-full" />
        </div>
        <div className="absolute inset-0 bg-gradient-to-br from-emerald-900/40 via-transparent to-emerald-700/30" />
        <div className="relative z-10 p-12 flex flex-col justify-between">
          <div>
            <div className="inline-flex items-center gap-2 text-emerald-100">
              <div className="w-9 h-9 rounded-lg bg-white/10 grid place-items-center">
                <svg viewBox="0 0 24 24" className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2">
                  <path d="M5 19l9-9" /><circle cx="16" cy="7" r="3" /><path d="M3 21l4-1 1-3" />
                </svg>
              </div>
              CourtBook
            </div>
          </div>
          <div>
            <div className="text-3xl leading-tight max-w-md">
              Đặt sân cầu lông trong vài chạm.<br />
              Quản lý cụm sân gọn gàng mỗi ngày.
            </div>
            <p className="text-emerald-100/80 mt-4 max-w-md text-sm">
              Hơn 12.000 người chơi và 180 chủ sân đang dùng CourtBook để kết nối nhanh hơn,
              minh bạch hơn — không còn cảnh gọi điện hỏi sân trống.
            </p>
            <div className="mt-8 grid grid-cols-3 gap-4 max-w-md">
              {[
                { v: "12k+", l: "Người chơi" },
                { v: "180+", l: "Cụm sân" },
                { v: "4.7★", l: "Đánh giá" },
              ].map((s) => (
                <div key={s.l} className="bg-white/5 border border-white/10 rounded-lg p-3 backdrop-blur-sm">
                  <div className="text-white text-lg">{s.v}</div>
                  <div className="text-xs text-emerald-100/70">{s.l}</div>
                </div>
              ))}
            </div>
          </div>
          <div className="text-xs text-emerald-100/60">© 2026 CourtBook Platform</div>
        </div>
      </div>

      {/* Right form */}
      <div className="flex items-center justify-center p-6 sm:p-10 bg-[#fafaf7]">
        <Card className="w-full max-w-md p-7 border-slate-200 shadow-sm">
          <div className="mb-5">
            <h1 className="text-slate-900">
              {mode === "login" ? "Đăng nhập CourtBook" : "Tạo tài khoản mới"}
            </h1>
            <p className="text-sm text-slate-600 mt-1">
              {mode === "login"
                ? "Tiếp tục đặt sân hoặc quản lý cụm sân của bạn."
                : "Chọn vai trò phù hợp để bắt đầu trong vài bước."}
            </p>
          </div>

          {/* Role selector */}
          <div className="grid grid-cols-2 gap-2 mb-5">
            <button
              type="button"
              onClick={() => setRole("player")}
              className={`flex items-center gap-2 px-3 py-3 rounded-lg border text-left transition ${
                role === "player"
                  ? "border-emerald-600 bg-emerald-50 ring-2 ring-emerald-100"
                  : "border-slate-200 hover:border-slate-300 bg-white"
              }`}
            >
              <div className={`w-9 h-9 rounded-md grid place-items-center ${role === "player" ? "bg-emerald-600 text-white" : "bg-slate-100 text-slate-500"}`}>
                <Users className="w-4 h-4" />
              </div>
              <div>
                <div className="text-sm text-slate-900">Người chơi</div>
                <div className="text-xs text-slate-500">Tìm sân & đặt</div>
              </div>
            </button>
            <button
              type="button"
              onClick={() => setRole("owner")}
              className={`flex items-center gap-2 px-3 py-3 rounded-lg border text-left transition ${
                role === "owner"
                  ? "border-emerald-600 bg-emerald-50 ring-2 ring-emerald-100"
                  : "border-slate-200 hover:border-slate-300 bg-white"
              }`}
            >
              <div className={`w-9 h-9 rounded-md grid place-items-center ${role === "owner" ? "bg-emerald-600 text-white" : "bg-slate-100 text-slate-500"}`}>
                <Store className="w-4 h-4" />
              </div>
              <div>
                <div className="text-sm text-slate-900">Chủ sân</div>
                <div className="text-xs text-slate-500">Quản lý cụm sân</div>
              </div>
            </button>
          </div>

          <Tabs value={mode} onValueChange={(v) => setMode(v as Mode)}>
            <TabsList className="grid grid-cols-2 w-full bg-slate-100">
              <TabsTrigger value="login">Đăng nhập</TabsTrigger>
              <TabsTrigger value="register">Đăng ký</TabsTrigger>
            </TabsList>

            <TabsContent value="login" className="mt-5">
              <form onSubmit={submit} className="space-y-4">
                <Field label="Email hoặc số điện thoại" icon={Mail}>
                  <Input type="text" placeholder="ban@example.com" className="pl-9" defaultValue="player@courtbook.vn" />
                </Field>
                <Field label="Mật khẩu" icon={Lock} trailing={
                  <button type="button" onClick={() => setShowPass(!showPass)} className="text-slate-400 hover:text-slate-600">
                    {showPass ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  </button>
                }>
                  <Input type={showPass ? "text" : "password"} placeholder="••••••••" className="pl-9 pr-9" defaultValue="demo1234" />
                </Field>
                <div className="flex items-center justify-between text-sm">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <Checkbox defaultChecked /> <span className="text-slate-600">Ghi nhớ đăng nhập</span>
                  </label>
                  <button type="button" className="text-emerald-700 hover:underline">Quên mật khẩu?</button>
                </div>
                <Button type="submit" className="w-full bg-emerald-700 hover:bg-emerald-800">Đăng nhập</Button>

                <Divider />
                <SocialButtons />

                <p className="text-center text-sm text-slate-600 pt-2">
                  Chưa có tài khoản?{" "}
                  <button type="button" onClick={() => setMode("register")} className="text-emerald-700 hover:underline">Tạo mới</button>
                </p>
              </form>
            </TabsContent>

            <TabsContent value="register" className="mt-5">
              <form onSubmit={submit} className="space-y-4">
                <Field label="Họ và tên" icon={User}>
                  <Input placeholder="Nguyễn Văn A" className="pl-9" />
                </Field>
                <div className="grid grid-cols-2 gap-3">
                  <Field label="Email" icon={Mail}>
                    <Input type="email" placeholder="ban@email.com" className="pl-9" />
                  </Field>
                  <Field label="Số điện thoại" icon={Phone}>
                    <Input placeholder="09xx xxx xxx" className="pl-9" />
                  </Field>
                </div>
                <Field label="Mật khẩu" icon={Lock} trailing={
                  <button type="button" onClick={() => setShowPass(!showPass)} className="text-slate-400 hover:text-slate-600">
                    {showPass ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                  </button>
                }>
                  <Input type={showPass ? "text" : "password"} placeholder="Ít nhất 8 ký tự" className="pl-9 pr-9" />
                </Field>

                {role === "owner" && (
                  <div className="rounded-lg bg-amber-50 border border-amber-200 p-3 text-xs text-amber-900">
                    Tài khoản chủ sân cần được admin duyệt trước khi đăng cụm sân. Quá trình thường mất 1-2 ngày làm việc.
                  </div>
                )}

                <label className="flex items-start gap-2 text-sm text-slate-600">
                  <Checkbox className="mt-0.5" /> <span>Tôi đồng ý với <a className="text-emerald-700 hover:underline">Điều khoản dịch vụ</a> và <a className="text-emerald-700 hover:underline">Chính sách bảo mật</a>.</span>
                </label>

                <Button type="submit" className="w-full bg-emerald-700 hover:bg-emerald-800">
                  {role === "owner" ? "Đăng ký chủ sân" : "Tạo tài khoản"}
                </Button>

                <Divider />
                <SocialButtons />

                <p className="text-center text-sm text-slate-600 pt-2">
                  Đã có tài khoản?{" "}
                  <button type="button" onClick={() => setMode("login")} className="text-emerald-700 hover:underline">Đăng nhập</button>
                </p>
              </form>
            </TabsContent>
          </Tabs>
        </Card>
      </div>
    </main>
  );
}

function Field({
  label, icon: Icon, children, trailing,
}: { label: string; icon: any; children: React.ReactNode; trailing?: React.ReactNode }) {
  return (
    <div>
      <Label className="text-sm text-slate-600 mb-1.5 block">{label}</Label>
      <div className="relative">
        <Icon className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        {children}
        {trailing && <div className="absolute right-3 top-1/2 -translate-y-1/2">{trailing}</div>}
      </div>
    </div>
  );
}

function Divider() {
  return (
    <div className="flex items-center gap-3 text-xs text-slate-400">
      <div className="flex-1 h-px bg-slate-200" /> hoặc tiếp tục với <div className="flex-1 h-px bg-slate-200" />
    </div>
  );
}

function SocialButtons() {
  return (
    <div className="grid grid-cols-2 gap-2">
      <Button type="button" variant="outline" className="border-slate-200">
        <svg viewBox="0 0 24 24" className="w-4 h-4 mr-2"><path fill="#4285F4" d="M22.5 12.3c0-.7-.1-1.4-.2-2H12v3.8h5.9c-.3 1.4-1.1 2.6-2.3 3.4v2.8h3.7c2.2-2 3.4-5 3.4-8z"/><path fill="#34A853" d="M12 23c3.1 0 5.7-1 7.6-2.8l-3.7-2.8c-1 .7-2.3 1.1-3.9 1.1-3 0-5.5-2-6.4-4.7H1.7v2.9C3.6 20.5 7.5 23 12 23z"/><path fill="#FBBC05" d="M5.6 13.8c-.2-.7-.4-1.4-.4-2.3s.1-1.6.4-2.3V6.3H1.7C.9 7.9.5 9.9.5 12c0 2.1.5 4.1 1.3 5.7l3.8-2.9z"/><path fill="#EA4335" d="M12 5c1.7 0 3.2.6 4.4 1.7l3.3-3.3C17.7 1.5 15.1.5 12 .5 7.5.5 3.6 3 1.7 6.3l3.8 2.9C6.5 7 9 5 12 5z"/></svg>
        Google
      </Button>
      <Button type="button" variant="outline" className="border-slate-200">
        <svg viewBox="0 0 24 24" className="w-4 h-4 mr-2" fill="#1877F2"><path d="M24 12a12 12 0 10-13.9 11.9v-8.4H7v-3.5h3.1V9.4c0-3 1.8-4.7 4.5-4.7 1.3 0 2.7.2 2.7.2v3h-1.5c-1.5 0-2 .9-2 1.9v2.3h3.4l-.5 3.5h-2.9v8.4A12 12 0 0024 12z"/></svg>
        Facebook
      </Button>
    </div>
  );
}
