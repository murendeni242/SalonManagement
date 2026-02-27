// Analytics dashboard with 4 charts + summary cards
// Uses Recharts — make sure it's installed: npm install recharts
import { useEffect, useState } from "react";
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from "recharts";
import api from "../api/axios";
import { getUserRole } from "../auth/authUtils";

// ── Types ──────────────────────────────────────────────────────────

interface DailyRevenue {
  date:     string;
  revenue:  number;
  bookings: number;
}

interface StatusCount {
  status: string;
  count:  number;
}

interface ServiceRevenue {
  serviceName: string;
  revenue:     number;
  bookings:    number;
}

interface DayOfWeek {
  day:      string;
  bookings: number;
}

interface Summary {
  totalRevenue:      number;
  totalBookings:     number;
  newCustomers:      number;
  avgBookingValue:   number;
  completedBookings: number;
}

interface Analytics {
  revenueOverTime:  DailyRevenue[];
  bookingsByStatus: StatusCount[];
  topServices:      ServiceRevenue[];
  busiestDays:      DayOfWeek[];
  summary:          Summary;
}

// ── Colour palette ─────────────────────────────────────────────────

const STATUS_COLORS: Record<string, string> = {
  Pending:   "#f59e0b",
  Confirmed: "#3b82f6",
  Completed: "#22c55e",
  Cancelled: "#ef4444",
};

const BAR_COLOR   = "#1a1a1a";
const LINE_COLOR  = "#22c55e";

// ── Formatters ─────────────────────────────────────────────────────

const fmtCurrency = (v: number) => `R ${v.toFixed(0)}`;
const fmtDate     = (d: string) => {
  const date = new Date(d);
  return `${date.getDate()} ${date.toLocaleString("en-ZA", { month: "short" })}`;
};

// ── Custom tooltip for line chart ──────────────────────────────────

function RevenueTooltip({ active, payload, label }: {
  active?: boolean;
  payload?: { value: number }[];
  label?: string;
}) {
  if (!active || !payload?.length) return null;
  return (
    <div className="bg-white border border-gray-200 rounded-lg shadow px-3 py-2 text-sm">
      <p className="font-semibold text-gray-700 mb-1">{label}</p>
      <p className="text-green-600 font-bold">R {payload[0]?.value?.toFixed(2)}</p>
    </div>
  );
}

// ── Main component ─────────────────────────────────────────────────

export default function Dashboard() {
  const role = getUserRole();

  // Date range — default last 30 days
  const today   = new Date();
  const ago30   = new Date(today);
  ago30.setDate(ago30.getDate() - 30);

  const [fromDate,   setFromDate]   = useState(ago30.toISOString().split("T")[0]);
  const [toDate,     setToDate]     = useState(today.toISOString().split("T")[0]);
  const [analytics,  setAnalytics]  = useState<Analytics | null>(null);
  const [loading,    setLoading]    = useState(true);
  const [error,      setError]      = useState<string | null>(null);

  // Non-owner summary (simple booking counts)
  const [simpleStats, setSimpleStats] = useState({
    bookingsToday: 0,
    upcoming:      0,
  });

  useEffect(() => {
    if (role === "Owner") {
      fetchAnalytics();
    } else {
      fetchSimpleStats();
    }
  }, [fromDate, toDate, role]);

  const fetchAnalytics = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<Analytics>("/analytics/dashboard", {
        params: { from: fromDate, to: toDate },
      });
      setAnalytics(res.data);
    } catch {
      setError("Could not load analytics. Make sure the API is running.");
    } finally {
      setLoading(false);
    }
  };

  const fetchSimpleStats = async () => {
    setLoading(true);
    try {
      const res = await api.get<{ bookingDate: string; status: string }[]>("/bookings");
      const todayStr = new Date().toISOString().split("T")[0];
      setSimpleStats({
        bookingsToday: res.data.filter(b => b.bookingDate.startsWith(todayStr)).length,
        upcoming:      res.data.filter(b =>
          b.bookingDate >= todayStr && b.status !== "Cancelled"
        ).length,
      });
    } finally {
      setLoading(false);
    }
  };

  // ── Non-owner view ─────────────────────────────────────────────

  if (role !== "Owner") {
    return (
      <div className="p-6">
        <h1 className="text-2xl font-bold mb-6">Dashboard</h1>
        <div className="grid grid-cols-2 gap-4 max-w-sm">
          <StatCard label="Bookings Today" value={simpleStats.bookingsToday} color="bg-blue-600" />
          <StatCard label="Upcoming"       value={simpleStats.upcoming}      color="bg-purple-600" />
        </div>
      </div>
    );
  }

  // ── Owner loading / error ──────────────────────────────────────

  if (loading) return (
    <div className="p-6 flex items-center gap-3 text-gray-500">
      <div className="w-5 h-5 border-2 border-gray-300 border-t-gray-700 rounded-full animate-spin" />
      Loading analytics…
    </div>
  );

  if (error || !analytics) return (
    <div className="p-6">
      <div className="bg-red-50 border border-red-200 text-red-700 rounded-lg px-4 py-3 text-sm">
        {error ?? "No data available."}
      </div>
    </div>
  );

  const { summary, revenueOverTime, bookingsByStatus, topServices, busiestDays } = analytics;

  // ── Owner full dashboard ───────────────────────────────────────

  return (
    <div className="p-6 space-y-6">

      {/* Header + date picker */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-2xl font-bold">Dashboard</h1>
          <p className="text-sm text-gray-500">Analytics for your salon</p>
        </div>
        <div className="flex items-center gap-2 bg-white border rounded-lg px-3 py-2">
          <span className="text-sm text-gray-500">From</span>
          <input
            type="date"
            className="text-sm border-0 outline-none"
            value={fromDate}
            onChange={e => setFromDate(e.target.value)}
          />
          <span className="text-sm text-gray-400">→</span>
          <input
            type="date"
            className="text-sm border-0 outline-none"
            value={toDate}
            onChange={e => setToDate(e.target.value)}
          />
        </div>
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-2 lg:grid-cols-5 gap-4">
        <StatCard label="Total Revenue"    value={`R ${summary.totalRevenue.toFixed(0)}`}     color="bg-green-600" />
        <StatCard label="Net Avg / Booking" value={`R ${summary.avgBookingValue.toFixed(0)}`} color="bg-blue-600" />
        <StatCard label="Total Bookings"   value={summary.totalBookings}                       color="bg-purple-600" />
        <StatCard label="Completed"        value={summary.completedBookings}                   color="bg-gray-800" />
        <StatCard label="New Customers"    value={summary.newCustomers}                        color="bg-amber-500" />
      </div>

      {/* Row 1 — Revenue over time (full width) */}
      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
        <h2 className="text-sm font-semibold text-gray-700 mb-4">Revenue Over Time</h2>
        {revenueOverTime.every(d => d.revenue === 0) ? (
          <EmptyChart message="No revenue recorded in this period." />
        ) : (
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={revenueOverTime} margin={{ top: 5, right: 20, bottom: 5, left: 10 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis
                dataKey="date"
                tickFormatter={fmtDate}
                tick={{ fontSize: 11, fill: "#9ca3af" }}
                interval="preserveStartEnd"
              />
              <YAxis
                tickFormatter={v => `R${v}`}
                tick={{ fontSize: 11, fill: "#9ca3af" }}
                width={55}
              />
              <Tooltip content={<RevenueTooltip />} />
              <Line
                type="monotone"
                dataKey="revenue"
                stroke={LINE_COLOR}
                strokeWidth={2.5}
                dot={false}
                activeDot={{ r: 5, fill: LINE_COLOR }}
              />
            </LineChart>
          </ResponsiveContainer>
        )}
      </div>

      {/* Row 2 — Bookings by status + Busiest days */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">

        {/* Donut — bookings by status */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">Bookings by Status</h2>
          {bookingsByStatus.every(s => s.count === 0) ? (
            <EmptyChart message="No bookings in this period." />
          ) : (
            <div className="flex items-center gap-4">
              <ResponsiveContainer width="55%" height={180}>
                <PieChart>
                  <Pie
                    data={bookingsByStatus.filter(s => s.count > 0)}
                    dataKey="count"
                    nameKey="status"
                    cx="50%"
                    cy="50%"
                    innerRadius={50}
                    outerRadius={75}
                    paddingAngle={3}
                  >
                    {bookingsByStatus.map((entry) => (
                      <Cell
                        key={entry.status}
                        fill={STATUS_COLORS[entry.status] ?? "#9ca3af"}
                      />
                    ))}
                  </Pie>
                  <Tooltip formatter={(v, n) => [`${v} bookings`, n]} />
                </PieChart>
              </ResponsiveContainer>

              {/* Legend */}
              <div className="space-y-2 flex-1">
                {bookingsByStatus.map(s => (
                  <div key={s.status} className="flex items-center justify-between text-sm">
                    <div className="flex items-center gap-2">
                      <div
                        className="w-2.5 h-2.5 rounded-full flex-shrink-0"
                        style={{ background: STATUS_COLORS[s.status] ?? "#9ca3af" }}
                      />
                      <span className="text-gray-600">{s.status}</span>
                    </div>
                    <span className="font-semibold text-gray-800">{s.count}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Bar — busiest days */}
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">Busiest Days of the Week</h2>
          {busiestDays.every(d => d.bookings === 0) ? (
            <EmptyChart message="No bookings in this period." />
          ) : (
            <ResponsiveContainer width="100%" height={180}>
              <BarChart data={busiestDays} margin={{ top: 5, right: 10, bottom: 5, left: -10 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" vertical={false} />
                <XAxis dataKey="day" tick={{ fontSize: 12, fill: "#9ca3af" }} />
                <YAxis tick={{ fontSize: 11, fill: "#9ca3af" }} allowDecimals={false} />
                <Tooltip formatter={v => [`${v} bookings`]} />
                <Bar dataKey="bookings" fill={BAR_COLOR} radius={[4, 4, 0, 0]} maxBarSize={36} />
              </BarChart>
            </ResponsiveContainer>
          )}
        </div>
      </div>

      {/* Row 3 — Top services */}
      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-5">
        <h2 className="text-sm font-semibold text-gray-700 mb-4">Top Services by Revenue</h2>
        {topServices.length === 0 ? (
          <EmptyChart message="No completed bookings with payments in this period." />
        ) : (
          <ResponsiveContainer width="100%" height={topServices.length * 52 + 20}>
            <BarChart
              layout="vertical"
              data={topServices}
              margin={{ top: 5, right: 60, bottom: 5, left: 10 }}
            >
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" horizontal={false} />
              <XAxis
                type="number"
                tickFormatter={v => `R${v}`}
                tick={{ fontSize: 11, fill: "#9ca3af" }}
              />
              <YAxis
                type="category"
                dataKey="serviceName"
                tick={{ fontSize: 12, fill: "#374151" }}
                width={120}
              />
              <Tooltip
                formatter={(v, n) => [
                  n === "revenue" ? fmtCurrency(Number(v)) : `${v} bookings`,
                  n === "revenue" ? "Revenue" : "Bookings",
                ]}
              />
              <Bar dataKey="revenue" fill={BAR_COLOR} radius={[0, 4, 4, 0]} maxBarSize={28}>
                {topServices.map((_, i) => (
                  <Cell
                    key={i}
                    fill={i === 0 ? "#1a1a1a" : i === 1 ? "#374151" : "#6b7280"}
                  />
                ))}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        )}
      </div>

    </div>
  );
}

// ── Small reusable components ──────────────────────────────────────

function StatCard({ label, value, color }: {
  label: string;
  value: string | number;
  color: string;
}) {
  return (
    <div className={`${color} text-white rounded-xl p-4 shadow-sm`}>
      <p className="text-xs font-medium opacity-75 uppercase tracking-wide">{label}</p>
      <p className="text-2xl font-bold mt-1">{value}</p>
    </div>
  );
}

function EmptyChart({ message }: { message: string }) {
  return (
    <div className="h-[160px] flex items-center justify-center text-gray-400 text-sm">
      {message}
    </div>
  );
}
