import type { ReactNode } from "react";
import { useLocation } from "react-router-dom";
import Sidebar from "./Sidebar";
import { getUserEmail } from "../auth/authUtils";

const PAGE_TITLES: Record<string, string> = {
  "/dashboard": "Dashboard",
  "/calendar":  "Calendar",
  "/bookings":  "Bookings",
  "/customers": "Customers",
  "/staff":     "Staff",
  "/services":  "Services",
  "/sales":     "Sales & Payments",
  "/users":     "Staff & Roles",
  "/auditlog":  "Audit Log",
};

export default function DashboardLayout({ children }: { children: ReactNode }) {
  const location = useLocation();
  const title    = PAGE_TITLES[location.pathname] ?? "Salon System";
  const today    = new Date().toLocaleDateString("en-ZA", {
    weekday: "long", day: "numeric", month: "long", year: "numeric",
  });
  const initials = getUserEmail()?.substring(0, 2).toUpperCase() ?? "??";

  return (
    <div className="flex min-h-screen bg-gray-100">

      {/* Sidebar */}
      <Sidebar />

      {/* Main */}
      <div className="flex flex-col flex-1 min-w-0">

        {/* Top header */}
        <header className="h-[60px] bg-white border-b border-gray-200 px-6 flex items-center justify-between flex-shrink-0 shadow-sm">
          <div>
            <h1 className="text-[1.05rem] font-bold text-gray-900 leading-tight">{title}</h1>
            <p className="text-[0.72rem] text-gray-400">{today}</p>
          </div>
          <div className="w-8 h-8 rounded-full bg-teal-500 text-white text-xs font-bold flex items-center justify-center">
            {initials}
          </div>
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-y-auto">
          {children}
        </main>

      </div>
    </div>
  );
}
