import { useEffect, useState } from "react";
import api from "../api/axios";

// ── Types ─────────────────────────────────────────────────────────────────────

interface StaffMember {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  phone: string;
  email: string | null;
  role: string;
  status: string;
}

interface WorkingHoursRow {
  id: number;
  staffId: number;
  dayOfWeek: number;
  dayName: string;
  startTime: string;
  endTime: string;
}

interface WeeklyWorkingHours {
  staffId: number;
  staffName: string;
  workingDays: WorkingHoursRow[];
}

type SalonRole = "Stylist" | "Colourist" | "Therapist" | "Manager" | "Receptionist";

// ── Constants ─────────────────────────────────────────────────────────────────

const SALON_ROLES: SalonRole[] = [
  "Stylist", "Colourist", "Therapist", "Manager", "Receptionist",
];

const DAYS_OF_WEEK = [
  { label: "Monday",    value: 1 },
  { label: "Tuesday",   value: 2 },
  { label: "Wednesday", value: 3 },
  { label: "Thursday",  value: 4 },
  { label: "Friday",    value: 5 },
  { label: "Saturday",  value: 6 },
  { label: "Sunday",    value: 0 },
];

// ── Main component ────────────────────────────────────────────────────────────

export default function Staff() {
  const [staff,     setStaff]     = useState<StaffMember[]>([]);
  const [loading,   setLoading]   = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [error,     setError]     = useState<string | null>(null);

  // Working hours panel state
  const [hoursStaff,   setHoursStaff]   = useState<StaffMember | null>(null);
  const [weeklyHours,  setWeeklyHours]  = useState<WeeklyWorkingHours | null>(null);
  const [hoursLoading, setHoursLoading] = useState(false);
  const [hoursError,   setHoursError]   = useState<string | null>(null);

  // Inline edit state for a single day row
  const [editingDay,    setEditingDay]    = useState<number | null>(null);
  const [dayStartTime,  setDayStartTime]  = useState("09:00");
  const [dayEndTime,    setDayEndTime]    = useState("17:00");
  const [savingDay,     setSavingDay]     = useState(false);

  const [form, setForm] = useState({
    firstName: "",
    lastName:  "",
    phone:     "",
    email:     "",
    role:      "Stylist" as SalonRole,
    status:    "Active",
  });

  useEffect(() => { fetchStaff(); }, []);

  // ── Staff CRUD ──────────────────────────────────────────────────

  const fetchStaff = async () => {
    setLoading(true);
    try {
      const res = await api.get<StaffMember[]>("/staff");
      setStaff(res.data);
    } finally {
      setLoading(false);
    }
  };

  const openAdd = () => {
    setEditingId(null);
    setError(null);
    setForm({ firstName: "", lastName: "", phone: "", email: "", role: "Stylist", status: "Active" });
    setModalOpen(true);
  };

  const openEdit = (member: StaffMember) => {
    setEditingId(member.id);
    setError(null);
    setForm({
      firstName: member.firstName,
      lastName:  member.lastName,
      phone:     member.phone,
      email:     member.email ?? "",
      role:      member.role as SalonRole,
      status:    member.status,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm("Delete this staff member? Their booking history will be kept.")) return;
    try {
      await api.delete(`/staff/${id}`);
      fetchStaff();
      if (hoursStaff?.id === id) setHoursStaff(null);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      alert(axiosErr?.response?.data?.error ?? "Delete failed");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    const payload = {
      firstName: form.firstName, lastName: form.lastName,
      phone: form.phone, email: form.email || undefined,
      role: form.role, status: form.status, specialisations: [] as number[],
    };
    try {
      if (editingId) {
        await api.put(`/staff/${editingId}`, payload);
      } else {
        await api.post("/staff", payload);
      }
      setModalOpen(false);
      fetchStaff();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      setError(axiosErr?.response?.data?.error ?? "Operation failed");
    }
  };

  // ── Working hours ───────────────────────────────────────────────

  const openWorkingHours = async (member: StaffMember) => {
    setHoursStaff(member);
    setEditingDay(null);
    setHoursError(null);
    setWeeklyHours(null);
    setHoursLoading(true);
    try {
      const res = await api.get<WeeklyWorkingHours>(`/staff/${member.id}/working-hours`);
      setWeeklyHours(res.data);
    } catch {
      setHoursError("Failed to load working hours.");
    } finally {
      setHoursLoading(false);
    }
  };

  const closeWorkingHours = () => {
    setHoursStaff(null);
    setWeeklyHours(null);
    setEditingDay(null);
    setHoursError(null);
  };

  const getRowForDay = (dayValue: number): WorkingHoursRow | undefined =>
    weeklyHours?.workingDays.find(d => d.dayOfWeek === dayValue);

  const startEditDay = (dayValue: number) => {
    const existing = getRowForDay(dayValue);
    setDayStartTime(existing?.startTime ?? "09:00");
    setDayEndTime(existing?.endTime   ?? "17:00");
    setEditingDay(dayValue);
    setHoursError(null);
  };

  const cancelEditDay = () => setEditingDay(null);

  const saveDay = async (dayValue: number) => {
    if (!hoursStaff) return;
    setSavingDay(true);
    setHoursError(null);
    try {
      await api.put(`/staff/${hoursStaff.id}/working-hours`, {
        dayOfWeek: dayValue,
        startTime: `${dayStartTime}:00`,
        endTime:   `${dayEndTime}:00`,
      });
      const res = await api.get<WeeklyWorkingHours>(`/staff/${hoursStaff.id}/working-hours`);
      setWeeklyHours(res.data);
      setEditingDay(null);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      setHoursError(axiosErr?.response?.data?.error ?? "Failed to save working hours.");
    } finally {
      setSavingDay(false);
    }
  };

  const deleteDay = async (dayValue: number) => {
    if (!hoursStaff) return;
    if (!confirm("Remove this working day?")) return;
    setHoursError(null);
    try {
      await api.delete(`/staff/${hoursStaff.id}/working-hours/${dayValue}`);
      const res = await api.get<WeeklyWorkingHours>(`/staff/${hoursStaff.id}/working-hours`);
      setWeeklyHours(res.data);
      setEditingDay(null);
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      setHoursError(axiosErr?.response?.data?.error ?? "Failed to remove day.");
    }
  };

  if (loading) return <div className="p-6 text-gray-500">Loading staff…</div>;

  // ── Render ──────────────────────────────────────────────────────

  return (
    <div className="p-6 flex gap-6">

      {/* ── Left: staff table ───────────────────────────────────── */}
      <div className={hoursStaff ? "flex-1 min-w-0" : "w-full"}>
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold">Staff Management</h1>
          <button
            onClick={openAdd}
            className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
          >
            Add Staff
          </button>
        </div>

        <table className="w-full bg-white rounded shadow">
          <thead>
            <tr className="bg-gray-100 border-b text-left">
              <th className="py-3 px-4">Name</th>
              <th className="py-3 px-4">Phone</th>
              <th className="py-3 px-4">Job Role</th>
              <th className="py-3 px-4">Status</th>
              <th className="py-3 px-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            {staff.map(member => (
              <tr
                key={member.id}
                className={`border-b hover:bg-gray-50 ${
                  hoursStaff?.id === member.id ? "bg-teal-50" : ""
                }`}
              >
                <td className="py-3 px-4 font-medium">{member.fullName}</td>
                <td className="py-3 px-4 text-sm text-gray-600">{member.phone}</td>
                <td className="py-3 px-4 text-sm">{member.role}</td>
                <td className="py-3 px-4">
                  <span className={`text-xs px-2 py-1 rounded-full font-medium ${
                    member.status === "Active"
                      ? "bg-green-100 text-green-700"
                      : "bg-gray-100 text-gray-500"
                  }`}>
                    {member.status}
                  </span>
                </td>
                <td className="py-3 px-4 space-x-2">
                  <button
                    onClick={() => openEdit(member)}
                    className="bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() =>
                      hoursStaff?.id === member.id
                        ? closeWorkingHours()
                        : openWorkingHours(member)
                    }
                    className={`px-3 py-1 rounded text-sm ${
                      hoursStaff?.id === member.id
                        ? "bg-teal-600 text-white hover:bg-teal-700"
                        : "bg-teal-500 text-white hover:bg-teal-600"
                    }`}
                  >
                    {hoursStaff?.id === member.id ? "Close Hours" : "Hours"}
                  </button>
                  <button
                    onClick={() => handleDelete(member.id)}
                    className="bg-red-500 text-white px-3 py-1 rounded text-sm hover:bg-red-600"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
            {staff.length === 0 && (
              <tr>
                <td colSpan={5} className="py-8 text-center text-gray-400 text-sm">
                  No staff members yet. Click "Add Staff" to get started.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* ── Right: working hours panel ──────────────────────────── */}
      {hoursStaff && (
        <div className="w-[400px] flex-shrink-0">
          <div className="bg-white rounded shadow">

            {/* Panel header */}
            <div className="flex items-center justify-between px-5 py-4 border-b">
              <div>
                <h2 className="font-bold text-gray-800">Working Hours</h2>
                <p className="text-sm text-teal-600 font-medium">{hoursStaff.fullName}</p>
              </div>
              <button
                onClick={closeWorkingHours}
                className="text-gray-400 hover:text-gray-600 text-xl leading-none"
              >
                ×
              </button>
            </div>

            {/* Error banner */}
            {hoursError && (
              <div className="mx-5 mt-4 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded">
                {hoursError}
              </div>
            )}

            {/* Loading */}
            {hoursLoading && (
              <div className="px-5 py-8 text-center text-gray-400 text-sm">
                Loading…
              </div>
            )}

            {/* Days grid */}
            {!hoursLoading && weeklyHours && (
              <div className="divide-y">
                {DAYS_OF_WEEK.map(day => {
                  const existing  = getRowForDay(day.value);
                  const isEditing = editingDay === day.value;
                  const hasHours  = !!existing;

                  return (
                    <div key={day.value} className="px-5 py-3">

                      {/* Day label + status */}
                      <div className="flex items-center justify-between mb-1">
                        <span className="text-sm font-semibold text-gray-700 w-24">
                          {day.label}
                        </span>

                        {!isEditing && (
                          <div className="flex items-center gap-2">
                            {hasHours ? (
                              <>
                                <span className="text-sm text-gray-600">
                                  {existing.startTime} – {existing.endTime}
                                </span>
                                <button
                                  onClick={() => startEditDay(day.value)}
                                  className="text-xs text-blue-500 hover:text-blue-700 underline"
                                >
                                  Edit
                                </button>
                                <button
                                  onClick={() => deleteDay(day.value)}
                                  className="text-xs text-red-400 hover:text-red-600 underline"
                                >
                                  Remove
                                </button>
                              </>
                            ) : (
                              <>
                                <span className="text-xs text-gray-400 italic">Not working</span>
                                <button
                                  onClick={() => startEditDay(day.value)}
                                  className="text-xs text-teal-500 hover:text-teal-700 underline"
                                >
                                  + Add
                                </button>
                              </>
                            )}
                          </div>
                        )}
                      </div>

                      {/* Inline time editor */}
                      {isEditing && (
                        <div className="mt-2 bg-gray-50 rounded p-3 space-y-2">
                          <div className="flex gap-3 items-center">
                            <div className="flex-1">
                              <label className="block text-xs text-gray-500 mb-1">Start</label>
                              <input
                                type="time"
                                className="w-full border rounded px-2 py-1 text-sm"
                                value={dayStartTime}
                                onChange={e => setDayStartTime(e.target.value)}
                              />
                            </div>
                            <div className="flex-1">
                              <label className="block text-xs text-gray-500 mb-1">End</label>
                              <input
                                type="time"
                                className="w-full border rounded px-2 py-1 text-sm"
                                value={dayEndTime}
                                onChange={e => setDayEndTime(e.target.value)}
                              />
                            </div>
                          </div>
                          <div className="flex gap-2 justify-end pt-1">
                            <button
                              onClick={cancelEditDay}
                              className="text-xs px-3 py-1 rounded bg-gray-200 hover:bg-gray-300"
                            >
                              Cancel
                            </button>
                            <button
                              onClick={() => saveDay(day.value)}
                              disabled={savingDay}
                              className="text-xs px-3 py-1 rounded bg-teal-600 text-white hover:bg-teal-700 disabled:opacity-50"
                            >
                              {savingDay ? "Saving…" : "Save"}
                            </button>
                          </div>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            )}

            {/* Helper text at bottom */}
            {!hoursLoading && weeklyHours && (
              <div className="px-5 py-3 border-t bg-gray-50 rounded-b">
                <p className="text-xs text-gray-400">
                  Days with no hours set are treated as unavailable.
                  Bookings outside these hours will be rejected.
                </p>
              </div>
            )}
          </div>
        </div>
      )}

      {/* ── Add / Edit staff modal ──────────────────────────────── */}
      {modalOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-[420px] shadow-xl">
            <h2 className="text-xl font-bold mb-4">
              {editingId ? "Edit Staff Member" : "Add Staff Member"}
            </h2>

            {error && (
              <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded">
                {error}
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium mb-1">First Name *</label>
                  <input
                    className="w-full border p-2 rounded"
                    value={form.firstName}
                    onChange={e => setForm({ ...form, firstName: e.target.value })}
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Last Name *</label>
                  <input
                    className="w-full border p-2 rounded"
                    value={form.lastName}
                    onChange={e => setForm({ ...form, lastName: e.target.value })}
                    required
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Phone *</label>
                <input
                  className="w-full border p-2 rounded"
                  value={form.phone}
                  onChange={e => setForm({ ...form, phone: e.target.value })}
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Email (optional)</label>
                <input
                  type="email"
                  className="w-full border p-2 rounded"
                  value={form.email}
                  onChange={e => setForm({ ...form, email: e.target.value })}
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Job Role *</label>
                <select
                  className="w-full border p-2 rounded"
                  value={form.role}
                  onChange={e => setForm({ ...form, role: e.target.value as SalonRole })}
                >
                  {SALON_ROLES.map(r => <option key={r} value={r}>{r}</option>)}
                </select>
                <p className="text-xs text-gray-400 mt-1">
                  This is the salon job title, not the system login role.
                </p>
              </div>

              {editingId && (
                <div>
                  <label className="block text-sm font-medium mb-1">Status</label>
                  <select
                    className="w-full border p-2 rounded"
                    value={form.status}
                    onChange={e => setForm({ ...form, status: e.target.value })}
                  >
                    <option value="Active">Active</option>
                    <option value="Inactive">Inactive</option>
                  </select>
                </div>
              )}

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setModalOpen(false)}
                  className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 text-sm"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 rounded bg-green-600 text-white hover:bg-green-700 text-sm"
                >
                  {editingId ? "Update" : "Add Staff"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
