import { useEffect, useState } from "react";
import api from "../api/axios";
import { getUserRole } from "../auth/authUtils";

// ── Types ──────────────────────────────────────────────────────────
interface Booking {
  id: number;
  staffId: number;
  serviceId: number;
  customerId: number;
  bookingDate: string;  // "2025-03-15T00:00:00Z"
  startTime: string;    // "09:30:00"
  endTime: string;      // "10:15:00"
  totalPrice: number;
  status: string;
  notes: string | null;
}
interface StaffMember { id: number; fullName: string; }
interface Service     { id: number; name: string; }
interface Customer    { id: number; fullName: string; phone: string; }

// ── Constants ──────────────────────────────────────────────────────
const STATUS_COLORS: Record<string, { bg: string; text: string; dot: string }> = {
  Pending:   { bg: "bg-amber-100",  text: "text-amber-800",  dot: "bg-amber-400"  },
  Confirmed: { bg: "bg-blue-100",   text: "text-blue-800",   dot: "bg-blue-500"   },
  Completed: { bg: "bg-green-100",  text: "text-green-800",  dot: "bg-green-500"  },
  Cancelled: { bg: "bg-red-100",    text: "text-red-700",    dot: "bg-red-400"    },
};

const STATUS_BLOCK: Record<string, string> = {
  Pending:   "bg-amber-400  text-white",
  Confirmed: "bg-blue-500   text-white",
  Completed: "bg-green-500  text-white",
  Cancelled: "bg-red-400    text-white",
};

const SLOT_HEIGHT  = 48;  // px per 30-min slot
const SLOT_MINUTES = 30;
const DAY_START    = 8;   // 08:00
const DAY_END      = 19;  // 19:00

// ── Helpers ────────────────────────────────────────────────────────
function toMinutes(time: string) {
  const [h, m] = time.split(":").map(Number);
  return h * 60 + m;
}

function formatTime(time: string) {
  return time.substring(0, 5);
}

function dateKey(d: Date) {
  return d.toISOString().split("T")[0];
}

function generateTimeSlots() {
  const slots: string[] = [];
  for (let h = DAY_START; h < DAY_END; h++) {
    slots.push(`${String(h).padStart(2, "0")}:00`);
    slots.push(`${String(h).padStart(2, "0")}:30`);
  }
  return slots;
}

const TIME_SLOTS = generateTimeSlots();

// Returns the Monday of the week containing `date`
function weekStart(date: Date) {
  const d = new Date(date);
  const day = d.getDay();
  const diff = day === 0 ? -6 : 1 - day;
  d.setDate(d.getDate() + diff);
  d.setHours(0, 0, 0, 0);
  return d;
}

// Returns array of 7 dates for a week starting Monday
function weekDays(monday: Date) {
  return Array.from({ length: 7 }, (_, i) => {
    const d = new Date(monday);
    d.setDate(d.getDate() + i);
    return d;
  });
}

// Returns all dates in a calendar month grid (always 6 rows × 7 cols)
function monthGrid(year: number, month: number) {
  const firstDay  = new Date(year, month, 1);
  const lastDay   = new Date(year, month + 1, 0);
  const startDate = new Date(firstDay);
  const dow = firstDay.getDay();
  startDate.setDate(1 - (dow === 0 ? 6 : dow - 1));

  const days: Date[] = [];
  const cur = new Date(startDate);
  while (days.length < 42) {
    days.push(new Date(cur));
    cur.setDate(cur.getDate() + 1);
  }
  return days;
}

const MONTH_NAMES = ["January","February","March","April","May","June",
                     "July","August","September","October","November","December"];
const DAY_NAMES_SHORT = ["Mon","Tue","Wed","Thu","Fri","Sat","Sun"];

// ── Booking Popup ──────────────────────────────────────────────────
function BookingPopup({
  booking, customers, staffList, services,
  onClose, onConfirm, onComplete, onCancel,
}: {
  booking:   Booking;
  customers: Customer[];
  staffList: StaffMember[];
  services:  Service[];
  onClose:   () => void;
  onConfirm: (id: number) => void;
  onComplete:(id: number) => void;
  onCancel:  (id: number) => void;
}) {
  const role        = getUserRole();
  const customer    = customers.find(c => c.id === booking.customerId);
  const staff       = staffList.find(s => s.id === booking.staffId);
  const service     = services.find(s => s.id === booking.serviceId);
  const colors      = STATUS_COLORS[booking.status] ?? STATUS_COLORS.Pending;
  const canAct      = role === "Owner" || role === "Reception";

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50" onClick={onClose}>
      <div
        className="bg-white rounded-xl shadow-2xl w-[360px] overflow-hidden"
        onClick={e => e.stopPropagation()}
      >
        {/* Header strip */}
        <div className={`${colors.bg} px-5 py-4 flex items-center justify-between`}>
          <div className="flex items-center gap-2">
            <div className={`w-2.5 h-2.5 rounded-full ${colors.dot}`} />
            <span className={`text-sm font-bold ${colors.text}`}>{booking.status}</span>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-xl leading-none">×</button>
        </div>

        {/* Body */}
        <div className="px-5 py-4 space-y-3">
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-0.5">Customer</p>
            <p className="text-sm font-semibold text-gray-900">{customer?.fullName ?? `#${booking.customerId}`}</p>
            {customer?.phone && <p className="text-xs text-gray-500">{customer.phone}</p>}
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-0.5">Service</p>
              <p className="text-sm text-gray-800">{service?.name ?? `#${booking.serviceId}`}</p>
            </div>
            <div>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-0.5">Staff</p>
              <p className="text-sm text-gray-800">{staff?.fullName ?? `#${booking.staffId}`}</p>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-0.5">Time</p>
              <p className="text-sm text-gray-800">
                {formatTime(booking.startTime)} – {formatTime(booking.endTime)}
              </p>
            </div>
            <div>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-0.5">Price</p>
              <p className="text-sm font-semibold text-gray-900">R {booking.totalPrice.toFixed(2)}</p>
            </div>
          </div>

          {booking.notes && (
            <div>
              <p className="text-xs font-semibold text-gray-400 uppercase tracking-wide mb-0.5">Notes</p>
              <p className="text-sm text-gray-600 bg-gray-50 rounded px-2 py-1">{booking.notes}</p>
            </div>
          )}
        </div>

        {/* Action buttons */}
        {canAct && (
          <div className="px-5 pb-4 flex gap-2">
            {booking.status === "Pending" && (
              <button
                onClick={() => { onConfirm(booking.id); onClose(); }}
                className="flex-1 py-2 bg-blue-500 hover:bg-blue-600 text-white text-sm font-semibold rounded-lg transition-colors"
              >
                Confirm
              </button>
            )}
            {booking.status === "Confirmed" && (
              <button
                onClick={() => { onComplete(booking.id); onClose(); }}
                className="flex-1 py-2 bg-green-600 hover:bg-green-700 text-white text-sm font-semibold rounded-lg transition-colors"
              >
                Complete
              </button>
            )}
            {(booking.status === "Pending" || booking.status === "Confirmed") && (
              <button
                onClick={() => { onCancel(booking.id); onClose(); }}
                className="flex-1 py-2 bg-orange-500 hover:bg-orange-600 text-white text-sm font-semibold rounded-lg transition-colors"
              >
                Cancel
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

// ── Month View ─────────────────────────────────────────────────────
function MonthView({
  year, month, bookings, today,
  onSelectBooking, onDayClick,
}: {
  year: number; month: number;
  bookings: Booking[];
  today: string;
  onSelectBooking: (b: Booking) => void;
  onDayClick: (dateStr: string) => void;
}) {
  const days = monthGrid(year, month);

  const bookingsByDay: Record<string, Booking[]> = {};
  bookings.forEach(b => {
    const key = b.bookingDate.substring(0, 10);
    if (!bookingsByDay[key]) bookingsByDay[key] = [];
    bookingsByDay[key].push(b);
  });

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      {/* Day headers */}
      <div className="grid grid-cols-7 border-b border-gray-200">
        {DAY_NAMES_SHORT.map(d => (
          <div key={d} className="py-2.5 text-center text-xs font-semibold text-gray-500 uppercase tracking-wide">
            {d}
          </div>
        ))}
      </div>

      {/* Calendar grid */}
      <div className="grid grid-cols-7">
        {days.map((day, i) => {
          const key       = dateKey(day);
          const isToday   = key === today;
          const isMonth   = day.getMonth() === month;
          const dayBooks  = bookingsByDay[key] ?? [];

          return (
            <div
              key={i}
              className={`min-h-[110px] border-b border-r border-gray-100 p-1.5 cursor-pointer hover:bg-gray-50 transition-colors
                ${!isMonth ? "bg-gray-50/60" : ""}`}
              onClick={() => onDayClick(key)}
            >
              {/* Day number */}
              <div className={`w-7 h-7 flex items-center justify-center rounded-full text-sm font-semibold mb-1
                ${isToday
                  ? "bg-teal-500 text-white"
                  : isMonth ? "text-gray-800" : "text-gray-300"
                }`}>
                {day.getDate()}
              </div>

              {/* Booking pills — show up to 3, then "+N more" */}
              <div className="space-y-0.5">
                {dayBooks.slice(0, 3).map(b => {
                  const c = STATUS_BLOCK[b.status] ?? "bg-gray-400 text-white";
                  return (
                    <div
                      key={b.id}
                      className={`${c} text-[10px] font-medium px-1.5 py-0.5 rounded truncate cursor-pointer hover:opacity-80`}
                      onClick={e => { e.stopPropagation(); onSelectBooking(b); }}
                      title={`${formatTime(b.startTime)} — ${b.status}`}
                    >
                      {formatTime(b.startTime)}
                    </div>
                  );
                })}
                {dayBooks.length > 3 && (
                  <div className="text-[10px] text-gray-400 font-medium pl-1">
                    +{dayBooks.length - 3} more
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ── Week View ──────────────────────────────────────────────────────
function WeekView({
  monday, bookings, services, customers, today,
  onSelectBooking,
}: {
  monday: Date;
  bookings: Booking[];
  staffList: StaffMember[];
  services: Service[];
  customers: Customer[];
  today: string;
  onSelectBooking: (b: Booking) => void;
}) {
  const days = weekDays(monday);

  const bookingsByDay: Record<string, Booking[]> = {};
  bookings.forEach(b => {
    const key = b.bookingDate.substring(0, 10);
    if (!bookingsByDay[key]) bookingsByDay[key] = [];
    bookingsByDay[key].push(b);
  });

  const dayStart = DAY_START * 60;

  return (
    <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
      <div className="flex overflow-x-auto">

        {/* Time column */}
        <div className="flex-shrink-0 w-16 border-r border-gray-100">
          <div className="h-14 border-b border-gray-100" />
          {TIME_SLOTS.map(slot => (
            <div
              key={slot}
              className="border-b border-gray-50 flex items-start justify-end pr-2 pt-1"
              style={{ height: `${SLOT_HEIGHT}px` }}
            >
              {slot.endsWith(":00") && (
                <span className="text-[10px] text-gray-400 font-mono">{slot}</span>
              )}
            </div>
          ))}
        </div>

        {/* Day columns */}
        {days.map(day => {
          const key        = dateKey(day);
          const isToday    = key === today;
          const dayBooks   = bookingsByDay[key] ?? [];

          return (
            <div key={key} className="flex-1 min-w-[120px] border-r border-gray-100 last:border-r-0">
              {/* Day header */}
              <div className={`h-14 border-b border-gray-100 flex flex-col items-center justify-center
                ${isToday ? "bg-teal-50" : ""}`}>
                <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide">
                  {DAY_NAMES_SHORT[days.indexOf(day)]}
                </span>
                <span className={`text-lg font-bold mt-0.5
                  ${isToday ? "text-teal-600" : "text-gray-800"}`}>
                  {day.getDate()}
                </span>
              </div>

              {/* Time grid */}
              <div className="relative">
                {TIME_SLOTS.map(slot => (
                  <div
                    key={slot}
                    className={`border-b ${slot.endsWith(":00") ? "border-gray-100" : "border-gray-50"}`}
                    style={{ height: `${SLOT_HEIGHT}px` }}
                  />
                ))}

                {/* Booking blocks */}
                {dayBooks.map(b => {
                  const startMins = toMinutes(b.startTime);
                  const endMins   = toMinutes(b.endTime);
                  const topPx     = ((startMins - dayStart) / SLOT_MINUTES) * SLOT_HEIGHT;
                  const heightPx  = Math.max(((endMins - startMins) / SLOT_MINUTES) * SLOT_HEIGHT - 2, 20);
                  const color     = STATUS_BLOCK[b.status] ?? "bg-gray-400 text-white";
                  const customer  = customers.find(c => c.id === b.customerId);
                  const service   = services.find(s => s.id === b.serviceId);

                  return (
                    <div
                      key={b.id}
                      className={`absolute left-1 right-1 ${color} rounded-md px-1.5 py-1 overflow-hidden cursor-pointer hover:opacity-85 transition-opacity shadow-sm`}
                      style={{ top: `${topPx}px`, height: `${heightPx}px` }}
                      onClick={() => onSelectBooking(b)}
                    >
                      <p className="text-[11px] font-bold truncate leading-tight">
                        {service?.name ?? "Service"}
                      </p>
                      {heightPx > 30 && (
                        <p className="text-[10px] opacity-85 truncate">
                          {customer?.fullName ?? "Customer"}
                        </p>
                      )}
                      {heightPx > 46 && (
                        <p className="text-[10px] opacity-70">
                          {formatTime(b.startTime)}–{formatTime(b.endTime)}
                        </p>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ── Main Calendar ──────────────────────────────────────────────────
export default function Calendar() {
  const role  = getUserRole();
  const now   = new Date();
  const today = dateKey(now);

  const [view,          setView]          = useState<"month" | "week">("month");
  const [currentDate,   setCurrentDate]   = useState(new Date());
  const [bookings,      setBookings]      = useState<Booking[]>([]);
  const [staffList,     setStaffList]     = useState<StaffMember[]>([]);
  const [services,      setServices]      = useState<Service[]>([]);
  const [customers,     setCustomers]     = useState<Customer[]>([]);
  const [loading,       setLoading]       = useState(true);
  const [selectedStaff, setSelectedStaff] = useState<number | null>(null);
  const [popup,         setPopup]         = useState<Booking | null>(null);

  useEffect(() => { fetchData(); }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [resB, resS, resSv, resC] = await Promise.all([
        api.get<Booking[]>("/bookings"),
        api.get<StaffMember[]>("/staff"),
        api.get<Service[]>("/services"),
        api.get<Customer[]>("/customers"),
      ]);
      setBookings(resB.data);
      setStaffList(resS.data);
      setServices(resSv.data);
      setCustomers(resC.data);
    } finally {
      setLoading(false);
    }
  };

  // ── Status actions ───────────────────────────────────────────────
  const handleConfirm = async (id: number) => {
    await api.post(`/bookings/${id}/confirm`);
    fetchData();
  };
  const handleComplete = async (id: number) => {
    await api.post(`/bookings/${id}/complete`);
    fetchData();
  };
  const handleCancel = async (id: number) => {
    if (!confirm("Cancel this booking?")) return;
    await api.post(`/bookings/${id}/cancel`);
    fetchData();
  };

  // ── Navigation ───────────────────────────────────────────────────
  const navigate = (dir: -1 | 1) => {
    setCurrentDate(prev => {
      const d = new Date(prev);
      if (view === "month") d.setMonth(d.getMonth() + dir);
      else d.setDate(d.getDate() + 7 * dir);
      return d;
    });
  };

  const goToday = () => setCurrentDate(new Date());

  // When a month day cell is clicked, switch to week view for that week
  const handleDayClick = (dateStr: string) => {
    setCurrentDate(new Date(dateStr));
    setView("week");
  };

  // ── Filter bookings ──────────────────────────────────────────────
  const filteredBookings = selectedStaff
    ? bookings.filter(b => b.staffId === selectedStaff)
    : bookings;

  // ── Header label ────────────────────────────────────────────────
  const headerLabel = view === "month"
    ? `${MONTH_NAMES[currentDate.getMonth()]} ${currentDate.getFullYear()}`
    : (() => {
        const mon = weekStart(currentDate);
        const sun = new Date(mon); sun.setDate(sun.getDate() + 6);
        return `${mon.getDate()} ${MONTH_NAMES[mon.getMonth()]} – ${sun.getDate()} ${MONTH_NAMES[sun.getMonth()]} ${sun.getFullYear()}`;
      })();

  // ── Count for the current period ────────────────────────────────
  const periodCount = (() => {
    if (view === "month") {
      return filteredBookings.filter(b =>
        new Date(b.bookingDate).getMonth()    === currentDate.getMonth() &&
        new Date(b.bookingDate).getFullYear() === currentDate.getFullYear()
      ).length;
    }
    const mon = weekStart(currentDate);
    const sun = new Date(mon); sun.setDate(sun.getDate() + 7);
    return filteredBookings.filter(b => {
      const d = new Date(b.bookingDate);
      return d >= mon && d < sun;
    }).length;
  })();

  if (loading) return (
    <div className="p-6 flex items-center gap-3 text-gray-500">
      <div className="w-5 h-5 border-2 border-gray-300 border-t-gray-700 rounded-full animate-spin" />
      Loading calendar…
    </div>
  );

  return (
    <div className="p-6">

      {/* ── Toolbar ─────────────────────────────────────────────── */}
      <div className="flex items-center justify-between mb-5 flex-wrap gap-3">

        {/* Left — nav + title */}
        <div className="flex items-center gap-3">
          <button
            onClick={goToday}
            className="px-3 py-1.5 text-sm font-semibold border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
          >
            Today
          </button>
          <div className="flex items-center gap-1">
            <button
              onClick={() => navigate(-1)}
              className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-100 transition-colors text-gray-600"
            >
              ‹
            </button>
            <button
              onClick={() => navigate(1)}
              className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-100 transition-colors text-gray-600"
            >
              ›
            </button>
          </div>
          <h2 className="text-lg font-bold text-gray-900">{headerLabel}</h2>
          <span className="text-sm text-gray-400">{periodCount} booking{periodCount !== 1 ? "s" : ""}</span>
        </div>

        {/* Right — staff filter + view toggle */}
        <div className="flex items-center gap-3">
          {(role === "Owner" || role === "Reception") && (
            <select
              className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm text-gray-700 bg-white focus:outline-none focus:border-teal-400"
              value={selectedStaff ?? ""}
              onChange={e => setSelectedStaff(e.target.value ? Number(e.target.value) : null)}
            >
              <option value="">All Staff</option>
              {staffList.map(s => (
                <option key={s.id} value={s.id}>{s.fullName}</option>
              ))}
            </select>
          )}

          {/* View toggle */}
          <div className="flex bg-gray-100 rounded-lg p-1 gap-1">
            {(["month", "week"] as const).map(v => (
              <button
                key={v}
                onClick={() => setView(v)}
                className={`px-3 py-1 rounded-md text-sm font-semibold transition-colors capitalize
                  ${view === v
                    ? "bg-white text-gray-900 shadow-sm"
                    : "text-gray-500 hover:text-gray-700"
                  }`}
              >
                {v}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* ── Calendar ────────────────────────────────────────────── */}
      {view === "month" ? (
        <MonthView
          year={currentDate.getFullYear()}
          month={currentDate.getMonth()}
          bookings={filteredBookings}
          today={today}
          onSelectBooking={setPopup}
          onDayClick={handleDayClick}
        />
      ) : (
        <WeekView
          monday={weekStart(currentDate)}
          bookings={filteredBookings}
          staffList={staffList}
          services={services}
          customers={customers}
          today={today}
          onSelectBooking={setPopup}
        />
      )}

      {/* ── Legend ──────────────────────────────────────────────── */}
      <div className="flex gap-4 mt-4 flex-wrap">
        {Object.entries(STATUS_COLORS).map(([status, colors]) => (
          <div key={status} className="flex items-center gap-1.5 text-xs text-gray-500">
            <div className={`w-2.5 h-2.5 rounded-full ${colors.dot}`} />
            {status}
          </div>
        ))}
        {view === "month" && (
          <span className="text-xs text-gray-400 ml-auto">Click a day to open week view</span>
        )}
      </div>

      {/* ── Popup ───────────────────────────────────────────────── */}
      {popup && (
        <BookingPopup
          booking={popup}
          customers={customers}
          staffList={staffList}
          services={services}
          onClose={() => setPopup(null)}
          onConfirm={handleConfirm}
          onComplete={handleComplete}
          onCancel={handleCancel}
        />
      )}
    </div>
  );
}
