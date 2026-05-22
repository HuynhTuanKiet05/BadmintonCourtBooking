import { useState } from "react";
import { Card } from "./ui/card";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Textarea } from "./ui/textarea";
import { Badge } from "./ui/badge";
import { Switch } from "./ui/switch";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "./ui/dialog";
import { Plus, Pencil, Trash2, MapPin } from "lucide-react";
import { formatVND, venues } from "./data";
import { toast } from "sonner";

export function OwnerVenuesPage() {
  const owned = venues.slice(0, 2);
  const [selected, setSelected] = useState(owned[0]);

  return (
    <main className="max-w-7xl mx-auto px-4 sm:px-6 py-6">
      <div className="flex flex-wrap items-end justify-between gap-3 mb-5">
        <div>
          <h1 className="text-slate-900">Cụm sân của tôi</h1>
          <p className="text-sm text-slate-600 mt-1">Quản lý thông tin cụm sân, sân con và giá thuê.</p>
        </div>
        <AddVenueButton />
      </div>

      <div className="grid lg:grid-cols-[300px_1fr] gap-5">
        {/* Venue list */}
        <div className="space-y-3">
          {owned.map((v) => (
            <Card
              key={v.id}
              onClick={() => setSelected(v)}
              className={`p-4 cursor-pointer border ${selected.id === v.id ? "border-emerald-500 ring-2 ring-emerald-100" : "border-slate-200 hover:border-slate-300"}`}
            >
              <div className="flex items-start justify-between">
                <div>
                  <div className="text-slate-900">{v.name}</div>
                  <div className="text-xs text-slate-500 flex items-center gap-1 mt-1"><MapPin className="w-3 h-3" /> {v.district}</div>
                </div>
                <Badge variant="outline" className="bg-emerald-50 text-emerald-800 border-emerald-200">Đang hoạt động</Badge>
              </div>
              <div className="text-xs text-slate-500 mt-3">{v.courts.length} sân con · từ {formatVND(v.priceFrom)}/giờ</div>
            </Card>
          ))}
          <Card className="p-4 border-dashed border-slate-300 text-center text-sm text-slate-500">
            Bạn còn 1 slot cụm sân trong gói cơ bản
          </Card>
        </div>

        {/* Edit form */}
        <div className="space-y-5">
          <Card className="p-5 border-slate-200">
            <div className="flex items-center justify-between mb-4">
              <div className="text-slate-900">Thông tin cụm sân</div>
              <div className="flex items-center gap-2">
                <span className="text-xs text-slate-500">Hoạt động</span>
                <Switch defaultChecked />
              </div>
            </div>
            <div className="grid sm:grid-cols-2 gap-4">
              <Field label="Tên cụm sân"><Input defaultValue={selected.name} /></Field>
              <Field label="Khu vực"><Input defaultValue={selected.district} /></Field>
              <Field label="Địa chỉ" full><Input defaultValue={selected.address} /></Field>
              <Field label="Giờ mở cửa"><Input defaultValue="06:00" /></Field>
              <Field label="Giờ đóng cửa"><Input defaultValue="22:00" /></Field>
              <Field label="Mô tả" full><Textarea defaultValue={selected.description} rows={3} /></Field>
            </div>
            <div className="flex justify-end gap-2 mt-5">
              <Button variant="outline">Hủy</Button>
              <Button className="bg-emerald-700 hover:bg-emerald-800" onClick={() => toast.success("Đã lưu thông tin cụm sân")}>Lưu thay đổi</Button>
            </div>
          </Card>

          <Card className="p-5 border-slate-200">
            <div className="flex items-center justify-between mb-4">
              <div>
                <div className="text-slate-900">Sân con trong cụm</div>
                <div className="text-xs text-slate-500 mt-0.5">Đặt giá riêng cho từng sân con (VIP, sân chuẩn...).</div>
              </div>
              <AddCourtButton />
            </div>
            <div className="space-y-2">
              {selected.courts.map((c) => (
                <div key={c.id} className="flex items-center justify-between gap-3 border border-slate-200 rounded-lg p-3">
                  <div className="flex-1">
                    <div className="text-sm text-slate-900">{c.name}</div>
                    <div className="text-xs text-slate-500">{c.note || "Sân tiêu chuẩn"}</div>
                  </div>
                  <div className="text-sm text-slate-700 hidden sm:block">{formatVND(c.pricePerHour)}/giờ</div>
                  <Badge variant="outline" className="bg-emerald-50 text-emerald-800 border-emerald-200">Mở</Badge>
                  <div className="flex gap-1">
                    <Button size="icon" variant="ghost"><Pencil className="w-4 h-4" /></Button>
                    <Button size="icon" variant="ghost" className="text-rose-600 hover:text-rose-700 hover:bg-rose-50"><Trash2 className="w-4 h-4" /></Button>
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      </div>
    </main>
  );
}

function Field({ label, children, full }: { label: string; children: React.ReactNode; full?: boolean }) {
  return (
    <div className={full ? "sm:col-span-2" : ""}>
      <label className="text-sm text-slate-600 mb-1.5 block">{label}</label>
      {children}
    </div>
  );
}

function AddVenueButton() {
  const [open, setOpen] = useState(false);
  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button className="bg-emerald-700 hover:bg-emerald-800"><Plus className="w-4 h-4 mr-2" /> Thêm cụm sân</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Tạo cụm sân mới</DialogTitle>
          <DialogDescription>Cụm sân sẽ chờ admin duyệt trước khi hiển thị công khai.</DialogDescription>
        </DialogHeader>
        <div className="grid gap-3">
          <Field label="Tên cụm sân"><Input placeholder="Ví dụ: Sân Cầu Lông An Phú" /></Field>
          <Field label="Địa chỉ"><Input placeholder="Số nhà, phường, quận" /></Field>
          <Field label="Mô tả"><Textarea rows={3} placeholder="Mô tả ngắn về cụm sân của bạn" /></Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Giờ mở cửa"><Input defaultValue="06:00" /></Field>
            <Field label="Giờ đóng cửa"><Input defaultValue="22:00" /></Field>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>Hủy</Button>
          <Button className="bg-emerald-700 hover:bg-emerald-800" onClick={() => { setOpen(false); toast.success("Cụm sân đã gửi chờ admin duyệt"); }}>Tạo cụm sân</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function AddCourtButton() {
  const [open, setOpen] = useState(false);
  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm"><Plus className="w-4 h-4 mr-1" /> Thêm sân con</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Thêm sân con</DialogTitle>
          <DialogDescription>Thêm một sân mới vào cụm sân hiện tại.</DialogDescription>
        </DialogHeader>
        <div className="grid gap-3">
          <Field label="Tên sân"><Input placeholder="Ví dụ: Sân 5" /></Field>
          <Field label="Giá mỗi giờ (₫)"><Input type="number" defaultValue={100000} /></Field>
          <Field label="Mô tả ngắn"><Input placeholder="Sàn gỗ, đèn LED..." /></Field>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>Hủy</Button>
          <Button className="bg-emerald-700 hover:bg-emerald-800" onClick={() => { setOpen(false); toast.success("Đã thêm sân con"); }}>Thêm sân</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
