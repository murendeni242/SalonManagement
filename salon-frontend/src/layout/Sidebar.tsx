import { Link, useLocation } from "react-router-dom";
import { getUserRole, getUserEmail } from "../auth/authUtils";

function Icon({ d, size = 18 }: { d: string; size?: number }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none"
      stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round"
      style={{ flexShrink: 0 }}>
      <path d={d} />
    </svg>
  );
}

const I = {
  scissors: "M6 3a3 3 0 1 0 0 6 3 3 0 0 0 0-6zm12 12a3 3 0 1 0 0 6 3 3 0 0 0 0-6zM20 4L8.12 15.88M14.47 14.48L20 20M8.12 8.12L12 12",
  home:     "M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z M9 22V12h6v10",
  calendar: "M8 2v4M16 2v4M3 10h18M5 4h14a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2z",
  users2:   "M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM23 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75",
  user:     "M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2M12 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z",
  wrench:   "M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z",
  dollar:   "M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6",
  chart:    "M18 20V10M12 20V4M6 20v-6",
  shield:   "M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z",
  clipboard:"M9 11l3 3L22 4M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11",
  logout:   "M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9",
};

function NavItem({ to, icon, label, active }: {
  to: string; icon: string; label: string; active: boolean;
}) {
  return (
    <Link to={to} style={{
      display: "flex", alignItems: "center", gap: "10px",
      padding: "8px 12px", borderRadius: "6px", marginBottom: "1px",
      fontSize: "0.875rem", fontWeight: active ? 600 : 400,
      color: active ? "#fff" : "rgba(255,255,255,0.6)",
      background: active ? "rgba(32,178,170,0.2)" : "transparent",
      borderLeft: `3px solid ${active ? "#20b2aa" : "transparent"}`,
      textDecoration: "none", transition: "all 0.15s",
    }}
    onMouseEnter={e => { if (!active) { const el = e.currentTarget as HTMLElement; el.style.background = "rgba(255,255,255,0.06)"; el.style.color = "rgba(255,255,255,0.9)"; } }}
    onMouseLeave={e => { if (!active) { const el = e.currentTarget as HTMLElement; el.style.background = "transparent"; el.style.color = "rgba(255,255,255,0.6)"; } }}
    >
      <Icon d={icon} size={17} />
      <span>{label}</span>
    </Link>
  );
}

function SectionLabel({ label }: { label: string }) {
  return (
    <div style={{
      fontSize: "0.65rem", fontWeight: 700, letterSpacing: "0.1em",
      textTransform: "uppercase", color: "rgba(255,255,255,0.3)",
      padding: "14px 12px 5px",
    }}>{label}</div>
  );
}

function Divider() {
  return <div style={{ height: "1px", background: "rgba(255,255,255,0.07)", margin: "6px 4px" }} />;
}

export default function Sidebar() {
  const role     = getUserRole();
  const email    = getUserEmail();
  const location = useLocation();
  const active   = (path: string) => location.pathname === path;

  const initials    = email ? email.substring(0, 2).toUpperCase() : "??";
  const displayName = email
    ? email.split("@")[0].replace(/[._]/g, " ").replace(/\b\w/g, (l: string) => l.toUpperCase())
    : "User";

  return (
    <aside style={{
      width: "240px", minHeight: "100vh",
      background: "#1e2a3a",
      display: "flex", flexDirection: "column", flexShrink: 0,
      borderRight: "1px solid rgba(255,255,255,0.06)",
      fontFamily: "-apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
    }}>

      {/* Brand */}
      <div style={{
        display: "flex", alignItems: "center", gap: "10px",
        padding: "18px 16px 16px",
        borderBottom: "1px solid rgba(255,255,255,0.07)",
      }}>
        <div style={{
          width: "34px", height: "34px", background: "#20b2aa",
          borderRadius: "8px", display: "flex", alignItems: "center",
          justifyContent: "center", color: "#fff", flexShrink: 0,
        }}>
          <Icon d={I.scissors} size={16} />
        </div>
        <div style={{ fontSize: "1.05rem", fontWeight: 700, color: "#fff" }}>
          Salon<span style={{ color: "#20b2aa" }}>System</span>
        </div>
      </div>

      {/* Nav */}
      <nav style={{ flex: 1, padding: "6px 8px", overflowY: "auto" }}>

        <SectionLabel label="Main" />
        <NavItem to="/dashboard" icon={I.home}     label="Dashboard" active={active("/dashboard")} />
        <NavItem to="/calendar"  icon={I.calendar} label="Calendar"  active={active("/calendar")}  />

        {(role === "Owner" || role === "Reception") && <>
          <NavItem to="/bookings"  icon={I.calendar} label="Bookings"  active={active("/bookings")}  />
          <NavItem to="/customers" icon={I.users2}   label="Customers" active={active("/customers")} />
          <NavItem to="/staff"     icon={I.user}     label="Staff"     active={active("/staff")}     />
          <NavItem to="/services"  icon={I.wrench}   label="Services"  active={active("/services")}  />
        </>}

        {role === "Staff" && (
          <NavItem to="/bookings" icon={I.calendar} label="My Bookings" active={active("/bookings")} />
        )}

        {role === "Owner" && <>
          <Divider />
          <SectionLabel label="Management" />
          <NavItem to="/sales"     icon={I.dollar} label="Sales & Payments"    active={active("/sales")}     />
          <NavItem to="/dashboard" icon={I.chart}  label="Reports & Analytics" active={false}               />
          <Divider />
          <SectionLabel label="Admin" />
          <NavItem to="/users"    icon={I.shield}    label="Staff & Roles" active={active("/users")}    />
          <NavItem to="/auditlog" icon={I.clipboard} label="Audit Log"     active={active("/auditlog")} />
        </>}

      </nav>

      {/* Footer — user profile + logout */}
      <div style={{ borderTop: "1px solid rgba(255,255,255,0.07)", padding: "10px 8px" }}>
        <div style={{ display: "flex", alignItems: "center", gap: "10px", padding: "8px 12px", marginBottom: "4px" }}>
          <div style={{
            width: "34px", height: "34px", borderRadius: "50%",
            background: "#20b2aa", color: "#fff",
            fontSize: "0.75rem", fontWeight: 700,
            display: "flex", alignItems: "center", justifyContent: "center", flexShrink: 0,
          }}>
            {initials}
          </div>
          <div style={{ overflow: "hidden" }}>
            <div style={{
              fontSize: "0.83rem", fontWeight: 600, color: "#fff",
              whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis",
            }}>{displayName}</div>
            <div style={{ fontSize: "0.72rem", color: "rgba(255,255,255,0.38)" }}>{role}</div>
          </div>
        </div>

        <button
          onClick={() => { localStorage.removeItem("salon_auth"); window.location.replace("/login"); }}
          style={{
            display: "flex", alignItems: "center", gap: "8px",
            width: "100%", padding: "8px 12px", borderRadius: "6px",
            background: "transparent", border: "none",
            color: "rgba(255,255,255,0.45)", fontSize: "0.83rem",
            cursor: "pointer", transition: "all 0.15s", textAlign: "left",
          }}
          onMouseEnter={e => { const el = e.currentTarget as HTMLElement; el.style.background = "rgba(239,68,68,0.15)"; el.style.color = "#f87171"; }}
          onMouseLeave={e => { const el = e.currentTarget as HTMLElement; el.style.background = "transparent"; el.style.color = "rgba(255,255,255,0.45)"; }}
        >
          <Icon d={I.logout} size={15} />
          Logout
        </button>
      </div>
    </aside>
  );
}
