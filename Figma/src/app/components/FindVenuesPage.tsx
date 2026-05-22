import { useState, useMemo } from "react";
import { Card } from "./ui/card";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Checkbox } from "./ui/checkbox";
import { Slider } from "./ui/slider";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { MapPin, Search, Clock, ChevronRight, SlidersHorizontal, Inbox } from "lucide-react";
import { formatVND, venues } from "./data";
import { CourtLines } from "./Shared";
import { ViewKey } from "./Header";

const districts = ["Tất cả", "Quận 10", "Quận 11", "Tân Bình", "Bình Thạnh", "Gò Vấp", "Thủ Đức"];

export function FindVenuesPage({ onNavigate }: { onNavigate: (v: ViewKey, id?: string) => void }) {
  const [q, setQ] = useState("");
  const [district, setDistrict] = useState("Tất cả");
  const [maxPrice, setMaxPrice] = useState([200000]);
  const [onlyAvailable, setOnlyAvailable] = useState(false);
  const [sort, setSort] = useState("relevance");

  const filtered = useMemo(() => {
    let list = venues.filter((v) => {
      if (q && !(`${v.name} ${v.district}`.toLowerCase().includes(q.toLowerCase()))) return false;
      if (district !== "Tất cả" && !v.district.includes(district)) return false;
      if (v.priceFrom > maxPrice[0]) return false;
      if (onlyAvailable && !v.hasSlotsToday) return false;
      return true;
    });
    if (sort === "price") list = [...list].sort((a, b) => a.priceFrom - b.priceFrom);
    if (sort === "rating") list = [...list].sort((a, b) => b.rating - a.rating);
    return list;
  }, [q, district, maxPrice, onlyAvailable, sort]);

  return (
    <main className="max-w-7xl mx-auto px-4 sm:px-6 py-8">
      <div className="mb-6">
        <h1 className="text-slate-900">Tìm sân cầu lông</h1>
        <p className="text-slate-600 text-sm mt-1">{filtered.length} cụm sân phù hợp tại khu vực TP.HCM</p>
      </div>

      <div className="grid lg:grid-cols-[280px_1fr] gap-6">
        {/* Filter sidebar */}
        <aside className="space-y-4 lg:sticky lg:top-20 self-start">
          <Card className="p-5 border-slate-200">
            <div className="flex items-center gap-2 mb-4">
              <SlidersHorizontal className="w-4 h-4 text-slate-500" />
              <div className="text-slate-900">Bộ lọc</div>
            </div>

            <div className="space-y-5">
              <div>
                <label className="text-sm text-slate-600 mb-2 block">Khu vực</label>
                <div className="flex flex-wrap gap-1.5">
                  {districts.map((d) => (
                    <button
                      key={d}
                      onClick={() => setDistrict(d)}
                      className={`px-2.5 py-1 text-xs rounded-full border ${
                        district === d
                          ? "bg-emerald-700 border-emerald-700 text-white"
                          : "bg-white border-slate-200 text-slate-600 hover:border-slate-300"
                      }`}
                    >
                      {d}
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-sm text-slate-600">Giá tối đa / giờ</label>
                  <span className="text-xs text-slate-500">{formatVND(maxPrice[0])}</span>
                </div>
                <Slider value={maxPrice} onValueChange={setMaxPrice} min={50000} max={250000} step={10000} />
              </div>

              <div>
                <label className="text-sm text-slate-600 mb-2 block">Giờ mở cửa</label>
                <Select defaultValue="any">
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="any">Bất kỳ</SelectItem>
                    <SelectItem value="morning">Mở sớm (trước 06:00)</SelectItem>
                    <SelectItem value="late">Mở khuya (sau 22:00)</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <label className="flex items-start gap-2 cursor-pointer">
                <Checkbox checked={onlyAvailable} onCheckedChange={(c) => setOnlyAvailable(!!c)} className="mt-0.5" />
                <span className="text-sm text-slate-700 leading-tight">Chỉ hiện sân còn slot hôm nay</span>
              </label>
            </div>
          </Card>

          <Card className="p-4 border-dashed border-emerald-200 bg-emerald-50/40">
            <div className="text-sm text-emerald-900">Mẹo nhỏ</div>
            <p className="text-xs text-emerald-800/80 mt-1">Khung 17:00–20:00 thường full. Đặt trước 2–3 ngày để có slot đẹp.</p>
          </Card>
        </aside>

        {/* Results */}
        <section>
          <div className="flex flex-col sm:flex-row gap-3 mb-4">
            <div className="relative flex-1">
              <Search className="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
              <Input value={q} onChange={(e) => setQ(e.target.value)} placeholder="Tìm theo tên sân hoặc khu vực..." className="pl-9 bg-white" />
            </div>
            <Select value={sort} onValueChange={setSort}>
              <SelectTrigger className="sm:w-48 bg-white"><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="relevance">Liên quan nhất</SelectItem>
                <SelectItem value="price">Giá thấp đến cao</SelectItem>
                <SelectItem value="rating">Đánh giá cao nhất</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {filtered.length === 0 ? (
            <Card className="p-12 text-center border-dashed border-slate-300 bg-white">
              <div className="w-14 h-14 mx-auto rounded-full bg-slate-100 grid place-items-center text-slate-400">
                <Inbox className="w-7 h-7" />
              </div>
              <div className="mt-3 text-slate-900">Không tìm thấy sân phù hợp</div>
              <p className="text-sm text-slate-500 mt-1">Thử nới khu vực hoặc tăng giá tối đa.</p>
              <Button variant="outline" className="mt-4" onClick={() => { setQ(""); setDistrict("Tất cả"); setMaxPrice([200000]); setOnlyAvailable(false); }}>
                Xóa bộ lọc
              </Button>
            </Card>
          ) : (
            <div className="grid sm:grid-cols-2 gap-4">
              {filtered.map((v, idx) => (
                <Card
                  key={v.id}
                  className="overflow-hidden border-slate-200 hover:shadow-md hover:border-emerald-300 transition cursor-pointer flex flex-col"
                  onClick={() => onNavigate("venue", v.id)}
                >
                  <div className={`relative ${idx % 3 === 0 ? "h-40" : "h-32"} bg-emerald-800 text-emerald-300/40`}>
                    <CourtLines className="absolute inset-0 w-full h-full" />
                    <div className="absolute top-3 left-3 flex gap-2 flex-wrap">
                      {v.hasSlotsToday && <Badge className="bg-amber-400 text-amber-900 hover:bg-amber-400">Còn slot hôm nay</Badge>}
                      {v.responseFast && <Badge className="bg-white/95 text-slate-700 hover:bg-white">Phản hồi nhanh</Badge>}
                    </div>
                    <div className="absolute bottom-3 right-3 bg-black/40 text-white text-xs px-2 py-1 rounded backdrop-blur">
                      {v.courts.length} sân con
                    </div>
                  </div>
                  <div className="p-4 flex-1 flex flex-col">
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <div className="text-slate-900">{v.name}</div>
                        <div className="text-xs text-slate-500 flex items-center gap-1 mt-1">
                          <MapPin className="w-3 h-3" /> {v.address}
                        </div>
                      </div>
                      <div className="text-right text-xs text-slate-600 shrink-0">
                        <div>★ {v.rating}</div>
                        <div className="text-slate-400">{v.reviews} đánh giá</div>
                      </div>
                    </div>
                    {v.highlight && (
                      <div className="mt-2 text-xs text-emerald-800 bg-emerald-50 inline-flex items-center gap-1 px-2 py-1 rounded w-fit">
                        {v.highlight}
                      </div>
                    )}
                    <div className="text-xs text-slate-500 flex items-center gap-1 mt-2">
                      <Clock className="w-3 h-3" /> Mở cửa {v.openHours}
                    </div>
                    <div className="mt-auto pt-4 flex items-center justify-between border-t border-slate-100">
                      <div>
                        <div className="text-xs text-slate-500">Từ</div>
                        <div className="text-slate-900">{formatVND(v.priceFrom)}<span className="text-xs text-slate-500"> /giờ</span></div>
                      </div>
                      <Button variant="ghost" size="sm" className="text-emerald-700">
                        Xem chi tiết <ChevronRight className="w-4 h-4 ml-1" />
                      </Button>
                    </div>
                  </div>
                </Card>
              ))}
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
