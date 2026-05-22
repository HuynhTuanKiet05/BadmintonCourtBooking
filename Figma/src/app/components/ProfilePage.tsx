import { Card } from "./ui/card";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { Label } from "./ui/label";
import { Avatar, AvatarFallback } from "./ui/avatar";
import { Switch } from "./ui/switch";
import { Separator } from "./ui/separator";
import { Bell, KeyRound, LogOut, Mail, MapPin, Phone, ShieldCheck, User } from "lucide-react";
import { toast } from "sonner";
import { ViewKey } from "./Header";

export function ProfilePage({ onNavigate }: { onNavigate: (v: ViewKey) => void }) {
  return (
    <main className="max-w-5xl mx-auto px-4 sm:px-6 py-8">
      <div className="mb-6">
        <h1 className="text-slate-900">Hồ sơ cá nhân</h1>
        <p className="text-sm text-slate-600 mt-1">Cập nhật thông tin và tùy chỉnh tài khoản của bạn.</p>
      </div>

      <div className="grid lg:grid-cols-[1fr_2fr] gap-5">
        <Card className="p-5 border-slate-200 self-start">
          <div className="flex flex-col items-center text-center">
            <Avatar className="w-20 h-20">
              <AvatarFallback className="bg-emerald-100 text-emerald-800 text-xl">MK</AvatarFallback>
            </Avatar>
            <div className="text-slate-900 mt-3">Nguyễn Minh Khoa</div>
            <div className="text-xs text-slate-500">Thành viên từ 03/2025</div>
            <div className="mt-3 flex items-center gap-1 text-xs text-emerald-700 bg-emerald-50 px-2 py-1 rounded">
              <ShieldCheck className="w-3 h-3" /> Đã xác minh số điện thoại
            </div>
            <Button variant="outline" size="sm" className="mt-4 w-full">Đổi ảnh đại diện</Button>
          </div>

          <Separator className="my-5" />

          <div className="space-y-1 text-sm">
            <div className="flex items-center justify-between py-2">
              <span className="text-slate-500">Số lượt đặt</span><span className="text-slate-900">24</span>
            </div>
            <div className="flex items-center justify-between py-2">
              <span className="text-slate-500">Cụm sân yêu thích</span><span className="text-slate-900">3</span>
            </div>
            <div className="flex items-center justify-between py-2">
              <span className="text-slate-500">Tỉ lệ đến sân</span><span className="text-emerald-700">96%</span>
            </div>
          </div>

          <Separator className="my-5" />

          <Button variant="ghost" className="w-full text-rose-700 hover:bg-rose-50 hover:text-rose-800" onClick={() => { toast("Đã đăng xuất"); onNavigate("landing"); }}>
            <LogOut className="w-4 h-4 mr-2" /> Đăng xuất
          </Button>
        </Card>

        <div className="space-y-5">
          <Card className="p-5 border-slate-200">
            <div className="text-slate-900 mb-4">Thông tin cá nhân</div>
            <div className="grid sm:grid-cols-2 gap-4">
              <Field label="Họ và tên" icon={User}><Input defaultValue="Nguyễn Minh Khoa" className="pl-9" /></Field>
              <Field label="Số điện thoại" icon={Phone}><Input defaultValue="0905 221 884" className="pl-9" /></Field>
              <Field label="Email" icon={Mail} full><Input defaultValue="khoa.nguyen@email.com" className="pl-9" /></Field>
              <Field label="Khu vực thường chơi" icon={MapPin} full><Input defaultValue="Quận 11, TP.HCM" className="pl-9" /></Field>
            </div>
            <div className="flex justify-end gap-2 mt-5">
              <Button variant="outline">Hủy</Button>
              <Button className="bg-emerald-700 hover:bg-emerald-800" onClick={() => toast.success("Đã cập nhật hồ sơ")}>Lưu thay đổi</Button>
            </div>
          </Card>

          <Card className="p-5 border-slate-200">
            <div className="text-slate-900 mb-4 flex items-center gap-2"><KeyRound className="w-4 h-4 text-slate-500" /> Bảo mật</div>
            <div className="grid sm:grid-cols-2 gap-4">
              <div>
                <Label className="text-sm text-slate-600 mb-1.5 block">Mật khẩu hiện tại</Label>
                <Input type="password" placeholder="••••••••" />
              </div>
              <div>
                <Label className="text-sm text-slate-600 mb-1.5 block">Mật khẩu mới</Label>
                <Input type="password" placeholder="Ít nhất 8 ký tự" />
              </div>
            </div>
            <div className="flex justify-end mt-4">
              <Button variant="outline" onClick={() => toast.success("Đã đổi mật khẩu")}>Đổi mật khẩu</Button>
            </div>
          </Card>

          <Card className="p-5 border-slate-200">
            <div className="text-slate-900 mb-1 flex items-center gap-2"><Bell className="w-4 h-4 text-slate-500" /> Thông báo</div>
            <p className="text-xs text-slate-500 mb-4">Chọn loại thông báo bạn muốn nhận.</p>
            <div className="divide-y divide-slate-100">
              {[
                { l: "Xác nhận đặt sân", d: "Khi chủ sân xác nhận hoặc từ chối", on: true },
                { l: "Nhắc lịch chơi", d: "1 giờ trước giờ đặt sân", on: true },
                { l: "Ưu đãi & cụm sân mới", d: "Tối đa 2 email mỗi tuần", on: false },
              ].map((r) => (
                <div key={r.l} className="flex items-center justify-between py-3">
                  <div>
                    <div className="text-sm text-slate-900">{r.l}</div>
                    <div className="text-xs text-slate-500">{r.d}</div>
                  </div>
                  <Switch defaultChecked={r.on} />
                </div>
              ))}
            </div>
          </Card>
        </div>
      </div>
    </main>
  );
}

function Field({ label, icon: Icon, children, full }: { label: string; icon: any; children: React.ReactNode; full?: boolean }) {
  return (
    <div className={full ? "sm:col-span-2" : ""}>
      <Label className="text-sm text-slate-600 mb-1.5 block">{label}</Label>
      <div className="relative">
        <Icon className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        {children}
      </div>
    </div>
  );
}
