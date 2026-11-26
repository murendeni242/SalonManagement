import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { useLocation } from "react-router-dom";
import Sidebar from "./Sidebar";
import { getUserEmail } from "../auth/authUtils";

// Add all page titles here — sidebar active state and header title
// both depend on this map being complete.
const PAGE_TITLES: Record<string, string> = {
  "/dashboard":              "Dashboard",
  "/calendar":               "Calendar",
  "/bookings":               "Bookings",
  "/customers":              "Customers",
  "/staff":                  "Staff",
  "/services":               "Services",
  "/sales":                  "Sales & Payments",
  "/users":                  "Staff & Roles",
  "/auditlog":               "Audit Log",
  "/reports":                "Reports & Analytics",
  // Commission pages
  "/commissions/settings":   "Commission Settings",
  "/commissions/earnings":   "Staff Earnings",
};

export default function DashboardLayout({ children }: { children: ReactNode }) {
  const location = useLocation();
  const [menuOpen, setMenuOpen] = useState(false);
  const title    = PAGE_TITLES[location.pathname] ?? "Salon System";
  const today    = new Date().toLocaleDateString("en-ZA", {
    weekday: "long", day: "numeric", month: "long", year: "numeric",
  });
  const initials = getUserEmail()?.substring(0, 2).toUpperCase() ?? "??";

  // Close the menu whenever the user navigates to another page
  useEffect(() => {
    setMenuOpen(false);
  }, [location.pathname]);

  return (
    <div className="flex min-h-screen bg-gray-100">

      {/* Dark backdrop behind the open menu (phones only) */}
      {menuOpen && (
        <div
          className="fixed inset-0 z-30 bg-black/40 md:hidden"
          onClick={() => setMenuOpen(false)}
          aria-hidden="true"
        />
      )}

      {/* Sidebar: slides in on phones, always visible from md (768px) up */}
      <div
        className={`fixed inset-y-0 left-0 z-40 transform transition-transform duration-200
          md:static md:z-auto md:translate-x-0
          ${menuOpen ? "translate-x-0" : "-translate-x-full"}`}
      >
        <Sidebar />
      </div>

      {/* Main */}
      <div className="flex flex-col flex-1 min-w-0">

        {/* Top header */}
        <header className="h-[60px] bg-white border-b border-gray-200 px-4 md:px-6 flex items-center justify-between flex-shrink-0 shadow-sm">
          <div className="flex items-center gap-3 min-w-0">
            <button
              type="button"
              onClick={() => setMenuOpen(true)}
              className="md:hidden flex items-center justify-center w-11 h-11 -ml-2 rounded-md text-gray-700 hover:bg-gray-100"
              aria-label="Open menu"
            >
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                strokeWidth="2" strokeLinecap="round">
                <path d="M3 6h18M3 12h18M3 18h18" />
              </svg>
            </button>
            <div className="min-w-0">
              <h1 className="text-[1.05rem] font-bold text-gray-900 leading-tight truncate">{title}</h1>
              <p className="hidden sm:block text-[0.72rem] text-gray-400">{today}</p>
            </div>
          </div>
          <div className="w-8 h-8 flex-shrink-0 rounded-full bg-teal-500 text-white text-xs font-bold flex items-center justify-center">
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