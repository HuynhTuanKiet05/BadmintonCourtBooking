import { useState } from "react";
import { Header, ViewKey } from "./components/Header";
import { LandingPage } from "./components/LandingPage";
import { FindVenuesPage } from "./components/FindVenuesPage";
import { VenueDetailPage } from "./components/VenueDetailPage";
import { MyBookingsPage } from "./components/MyBookingsPage";
import { OwnerDashboardPage } from "./components/OwnerDashboardPage";
import { OwnerVenuesPage } from "./components/OwnerVenuesPage";
import { AdminPage } from "./components/AdminPage";
import { AuthPage } from "./components/AuthPage";
import { ProfilePage } from "./components/ProfilePage";
import { Toaster } from "./components/ui/sonner";

export default function App() {
  const [view, setView] = useState<ViewKey>("landing");
  const [venueId, setVenueId] = useState<string>("v1");

  function navigate(v: ViewKey, id?: string) {
    if (id) setVenueId(id);
    setView(v);
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  return (
    <div className="min-h-screen bg-[#fafaf7] text-slate-900">
      <Header view={view} onNavigate={navigate} />
      {view === "landing" && <LandingPage onNavigate={navigate} />}
      {view === "find" && <FindVenuesPage onNavigate={navigate} />}
      {view === "venue" && <VenueDetailPage venueId={venueId} onBack={() => navigate("find")} />}
      {view === "bookings" && <MyBookingsPage onNavigate={navigate} />}
      {view === "owner" && <OwnerDashboardPage onNavigate={navigate} />}
      {view === "ownerVenues" && <OwnerVenuesPage />}
      {view === "admin" && <AdminPage />}
      {view === "login" && <AuthPage mode="login" onNavigate={navigate} />}
      {view === "register" && <AuthPage mode="register" onNavigate={navigate} />}
      {view === "profile" && <ProfilePage onNavigate={navigate} />}
      <Toaster position="top-right" richColors />
    </div>
  );
}
