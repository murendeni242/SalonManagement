// Full CRUD + search + profile view
// - List all customers with search by name or phone
// - Add / Edit / Delete (soft delete)
// - Click a customer → slide-in profile panel showing
//   total visits, total spent, days since last visit, recent bookings
import { useEffect, useState, useCallback } from "react";
import api from "../api/axios";
import { getUserRole } from "../auth/authUtils";

// ── Types ──────────────────────────────────────────────────────────

interface Customer {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  phone: string;
  email: string | null;
  dateOfBirth: string | null;
  notes: string | null;
  lastVisitAt: string | null;
  isDeleted: boolean;
}

interface BookingHistory {
  bookingId: number;
  bookingDate: string;
  startTime: string;
  serviceId: number;
  staffId: number;
  totalPrice: number;
  status: string;
}

interface CustomerProfile {
  customer: Customer;
  totalVisits: number;
  totalSpent: number;
  lastVisitAt: string | null;
  daysSinceLastVisit: number | null;
  recentBookings: BookingHistory[];
}

// ── Helpers ────────────────────────────────────────────────────────

function formatDate(iso: string | null): string {
  if (!iso) return "—";
  return new Date(iso).toLocaleDateString("en-ZA", {
    day: "numeric", month: "short", year: "numeric",
  });
}

function formatCurrency(amount: number): string {
  return `R ${amount.toFixed(2)}`;
}

const STATUS_CLASS: Record<string, string> = {
  Pending:   "bg-yellow-100 text-yellow-700",
  Confirmed: "bg-blue-100 text-blue-700",
  Completed: "bg-green-100 text-green-700",
  Cancelled: "bg-red-100 text-red-700",
};

// ── Main component ─────────────────────────────────────────────────

export default function Customers() {
  const role = getUserRole();

  // List state
  const [customers,       setCustomers]       = useState<Customer[]>([]);
  const [loading,         setLoading]         = useState(true);
  const [searchQuery,     setSearchQuery]     = useState("");
  const [searchResults,   setSearchResults]   = useState<Customer[] | null>(null);
  const [searching,       setSearching]       = useState(false);

  // Profile panel
  const [selectedId,      setSelectedId]      = useState<number | null>(null);
  const [profile,         setProfile]         = useState<CustomerProfile | null>(null);
  const [profileLoading,  setProfileLoading]  = useState(false);

  // Add/Edit modal
  const [modalOpen,       setModalOpen]       = useState(false);
  const [editingId,       setEditingId]       = useState<number | null>(null);
  const [modalError,      setModalError]      = useState<string | null>(null);

  // Notes editing (inside profile panel)
  const [editingNotes,    setEditingNotes]    = useState(false);
  const [notesValue,      setNotesValue]      = useState("");
  const [notesSaving,     setNotesSaving]     = useState(false);

  const [form, setForm] = useState({
    firstName:   "",
    lastName:    "",
    phone:       "",
    email:       "",
    dateOfBirth: "",
    notes:       "",
  });

  // ── Data fetching ────────────────────────────────────────────────

  const fetchCustomers = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.get<Customer[]>("/customers");
      setCustomers(res.data);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchCustomers(); }, [fetchCustomers]);

  // Search — fires when query >= 2 chars, clears when empty
  useEffect(() => {
    if (searchQuery.length === 0) {
      setSearchResults(null);
      return;
    }
    if (searchQuery.length < 2) return;

    const timer = setTimeout(async () => {
      setSearching(true);
      try {
        const res = await api.get<Customer[]>("/customers/search", {
          params: { q: searchQuery },
        });
        setSearchResults(res.data);
      } finally {
        setSearching(false);
      }
    }, 300); // debounce 300ms

    return () => clearTimeout(timer);
  }, [searchQuery]);

  // Load profile when a customer row is clicked
  useEffect(() => {
    if (!selectedId) { setProfile(null); return; }

    setProfileLoading(true);
    api.get<CustomerProfile>(`/customers/${selectedId}/profile`)
      .then(res => setProfile(res.data))
      .finally(() => setProfileLoading(false));
  }, [selectedId]);

  // ── CRUD handlers ────────────────────────────────────────────────

  const openAdd = () => {
    setEditingId(null);
    setModalError(null);
    setForm({ firstName: "", lastName: "", phone: "", email: "", dateOfBirth: "", notes: "" });
    setModalOpen(true);
  };

  const openEdit = (c: Customer) => {
    setEditingId(c.id);
    setModalError(null);
    setForm({
      firstName:   c.firstName,
      lastName:    c.lastName,
      phone:       c.phone,
      email:       c.email ?? "",
      dateOfBirth: c.dateOfBirth ? c.dateOfBirth.substring(0, 10) : "",
      notes:       c.notes ?? "",
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm("Delete this customer? Their booking history will be kept.")) return;
    try {
      await api.delete(`/customers/${id}`);
      if (selectedId === id) setSelectedId(null);
      fetchCustomers();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      alert(e?.response?.data?.error ?? "Delete failed");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setModalError(null);

    const payload = {
      firstName:   form.firstName,
      lastName:    form.lastName,
      phone:       form.phone,
      email:       form.email || undefined,
      dateOfBirth: form.dateOfBirth || undefined,
      notes:       form.notes || undefined,
    };

    try {
      if (editingId) {
        await api.put(`/customers/${editingId}`, payload);
        // Refresh profile if this customer is open
        if (selectedId === editingId) setSelectedId(editingId);
      } else {
        await api.post("/customers", payload);
      }
      setModalOpen(false);
      fetchCustomers();
      setSearchQuery("");
      setSearchResults(null);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setModalError(e?.response?.data?.error ?? "Operation failed");
    }
  };

  const handleSaveNotes = async () => {
    if (!selectedId) return;
    setNotesSaving(true);
    try {
      await api.patch(`/customers/${selectedId}/notes`, { notes: notesValue || null });
      // Refresh profile
      const res = await api.get<CustomerProfile>(`/customers/${selectedId}/profile`);
      setProfile(res.data);
      setEditingNotes(false);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      alert(e?.response?.data?.error ?? "Could not save notes");
    } finally {
      setNotesSaving(false);
    }
  };

  // ── Display list (search results or full list) ───────────────────

  const displayList = searchResults ?? customers;

  // ── Render ───────────────────────────────────────────────────────

  if (loading) return <div className="p-6 text-gray-500">Loading customers…</div>;

  return (
    <div className="flex h-[calc(100vh-0px)] overflow-hidden">

      {/* ── Left panel — list ───────────────────────────────────── */}
      <div className={`flex flex-col ${selectedId ? "w-1/2" : "w-full"} transition-all duration-200`}>

        {/* Header */}
        <div className="flex items-center justify-between p-6 pb-4">
          <h1 className="text-2xl font-bold">Customers</h1>
          {(role === "Owner" || role === "Reception") && (
            <button
              onClick={openAdd}
              className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 text-sm"
            >
              Add Customer
            </button>
          )}
        </div>

        {/* Search bar */}
        <div className="px-6 pb-4">
          <div className="relative">
            <input
              type="text"
              className="w-full border p-2 pl-9 rounded text-sm"
              placeholder="Search by name or phone number…"
              value={searchQuery}
              onChange={e => setSearchQuery(e.target.value)}
            />
            <span className="absolute left-3 top-2.5 text-gray-400 text-sm">🔍</span>
            {searching && (
              <span className="absolute right-3 top-2.5 text-gray-400 text-xs">Searching…</span>
            )}
          </div>
          {searchResults !== null && (
            <p className="text-xs text-gray-400 mt-1">
              {searchResults.length} result{searchResults.length !== 1 ? "s" : ""} for "{searchQuery}"
              <button
                className="ml-2 text-blue-500 hover:underline"
                onClick={() => { setSearchQuery(""); setSearchResults(null); }}
              >
                Clear
              </button>
            </p>
          )}
        </div>

        {/* Table */}
        <div className="flex-1 overflow-y-auto px-6 pb-6">
          <table className="w-full bg-white rounded shadow text-sm">
            <thead>
              <tr className="bg-gray-100 border-b text-left">
                <th className="py-3 px-4">Name</th>
                <th className="py-3 px-4">Phone</th>
                <th className="py-3 px-4">Email</th>
                <th className="py-3 px-4">Last Visit</th>
                {(role === "Owner" || role === "Reception") && (
                  <th className="py-3 px-4">Actions</th>
                )}
              </tr>
            </thead>
            <tbody>
              {displayList.map(c => (
                <tr
                  key={c.id}
                  className={`border-b cursor-pointer transition-colors ${
                    selectedId === c.id
                      ? "bg-blue-50"
                      : "hover:bg-gray-50"
                  }`}
                  onClick={() => setSelectedId(c.id === selectedId ? null : c.id)}
                >
                  <td className="py-3 px-4 font-medium">{c.fullName}</td>
                  <td className="py-3 px-4 text-gray-600">{c.phone}</td>
                  <td className="py-3 px-4 text-gray-500">{c.email ?? "—"}</td>
                  <td className="py-3 px-4 text-gray-500">{formatDate(c.lastVisitAt)}</td>
                  {(role === "Owner" || role === "Reception") && (
                    <td className="py-3 px-4" onClick={e => e.stopPropagation()}>
                      <div className="flex gap-1">
                        <button
                          onClick={() => openEdit(c)}
                          className="bg-blue-500 text-white px-2 py-1 rounded text-xs hover:bg-blue-600"
                        >
                          Edit
                        </button>
                        {role === "Owner" && (
                          <button
                            onClick={() => handleDelete(c.id)}
                            className="bg-red-500 text-white px-2 py-1 rounded text-xs hover:bg-red-600"
                          >
                            Delete
                          </button>
                        )}
                      </div>
                    </td>
                  )}
                </tr>
              ))}
              {displayList.length === 0 && (
                <tr>
                  <td colSpan={5} className="py-8 text-center text-gray-400">
                    {searchResults !== null
                      ? `No customers found matching "${searchQuery}"`
                      : "No customers yet. Click \"Add Customer\" to get started."}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* ── Right panel — profile ───────────────────────────────── */}
      {selectedId && (
        <div className="w-1/2 border-l bg-white overflow-y-auto flex flex-col">

          {/* Close button */}
          <div className="flex items-center justify-between px-6 py-4 border-b">
            <h2 className="text-lg font-bold">Customer Profile</h2>
            <button
              onClick={() => setSelectedId(null)}
              className="text-gray-400 hover:text-gray-600 text-xl leading-none"
            >
              ✕
            </button>
          </div>

          {profileLoading ? (
            <div className="p-6 text-gray-400">Loading profile…</div>
          ) : profile ? (
            <div className="p-6 space-y-6">

              {/* Personal details */}
              <div>
                <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">
                  Personal Details
                </h3>
                <div className="space-y-2 text-sm">
                  <Row label="Full Name"    value={profile.customer.fullName} />
                  <Row label="Phone"        value={profile.customer.phone} />
                  <Row label="Email"        value={profile.customer.email ?? "Not provided"} />
                  <Row label="Date of Birth"
                       value={formatDate(profile.customer.dateOfBirth)} />
                </div>
              </div>

              {/* Stats */}
              <div>
                <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">
                  Visit Summary
                </h3>
                <div className="grid grid-cols-3 gap-3">
                  <StatBox
                    label="Total Visits"
                    value={profile.totalVisits}
                    color="bg-blue-50 text-blue-700"
                  />
                  <StatBox
                    label="Total Spent"
                    value={formatCurrency(profile.totalSpent)}
                    color="bg-green-50 text-green-700"
                  />
                  <StatBox
                    label="Days Since Visit"
                    value={profile.daysSinceLastVisit ?? "Never"}
                    color={
                      profile.daysSinceLastVisit === null
                        ? "bg-gray-50 text-gray-500"
                        : profile.daysSinceLastVisit > 90
                        ? "bg-red-50 text-red-600"
                        : "bg-amber-50 text-amber-700"
                    }
                  />
                </div>
                {profile.daysSinceLastVisit !== null &&
                 profile.daysSinceLastVisit > 90 && (
                  <p className="mt-2 text-xs text-red-500 font-medium">
                    ⚠ Lapsed customer — last visit was over 90 days ago
                  </p>
                )}
              </div>

              {/* Notes */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider">
                    Notes / Allergies / Formulas
                  </h3>
                  {!editingNotes && (role === "Owner" || role === "Reception") && (
                    <button
                      onClick={() => {
                        setNotesValue(profile.customer.notes ?? "");
                        setEditingNotes(true);
                      }}
                      className="text-xs text-blue-500 hover:underline"
                    >
                      Edit
                    </button>
                  )}
                </div>

                {editingNotes ? (
                  <div className="space-y-2">
                    <textarea
                      className="w-full border p-2 rounded text-sm"
                      rows={4}
                      value={notesValue}
                      onChange={e => setNotesValue(e.target.value)}
                      placeholder="Allergies, colour formulas, preferences…"
                    />
                    <div className="flex gap-2">
                      <button
                        onClick={handleSaveNotes}
                        disabled={notesSaving}
                        className="bg-green-600 text-white px-3 py-1.5 rounded text-xs hover:bg-green-700 disabled:opacity-60"
                      >
                        {notesSaving ? "Saving…" : "Save Notes"}
                      </button>
                      <button
                        onClick={() => setEditingNotes(false)}
                        className="bg-gray-200 px-3 py-1.5 rounded text-xs hover:bg-gray-300"
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                ) : (
                  <p className={`text-sm p-3 rounded ${
                    profile.customer.notes
                      ? "bg-amber-50 text-gray-700 border border-amber-200"
                      : "text-gray-400 italic"
                  }`}>
                    {profile.customer.notes ?? "No notes recorded."}
                  </p>
                )}
              </div>

              {/* Recent bookings */}
              <div>
                <h3 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">
                  Recent Bookings
                </h3>
                {profile.recentBookings.length === 0 ? (
                  <p className="text-sm text-gray-400 italic">No bookings yet.</p>
                ) : (
                  <div className="space-y-2">
                    {profile.recentBookings.map(b => (
                      <div
                        key={b.bookingId}
                        className="flex items-center justify-between bg-gray-50 rounded px-3 py-2 text-sm border border-gray-100"
                      >
                        <div>
                          <span className="font-medium">
                            {formatDate(b.bookingDate)}
                          </span>
                          <span className="text-gray-400 ml-2 text-xs">
                            {b.startTime.substring(0, 5)}
                          </span>
                        </div>
                        <div className="flex items-center gap-3">
                          <span className="text-gray-600">
                            {formatCurrency(b.totalPrice)}
                          </span>
                          <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                            STATUS_CLASS[b.status] ?? "bg-gray-100 text-gray-500"
                          }`}>
                            {b.status}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>

            </div>
          ) : null}
        </div>
      )}

      {/* ── Add/Edit Modal ──────────────────────────────────────── */}
      {modalOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-[460px] shadow-xl max-h-[90vh] overflow-y-auto">
            <h2 className="text-xl font-bold mb-4">
              {editingId ? "Edit Customer" : "Add Customer"}
            </h2>

            {modalError && (
              <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded">
                {modalError}
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium mb-1">First Name *</label>
                  <input
                    className="w-full border p-2 rounded text-sm"
                    value={form.firstName}
                    onChange={e => setForm({ ...form, firstName: e.target.value })}
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Last Name *</label>
                  <input
                    className="w-full border p-2 rounded text-sm"
                    value={form.lastName}
                    onChange={e => setForm({ ...form, lastName: e.target.value })}
                    required
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Phone * <span className="font-normal text-gray-400">(primary lookup)</span>
                </label>
                <input
                  className="w-full border p-2 rounded text-sm"
                  value={form.phone}
                  onChange={e => setForm({ ...form, phone: e.target.value })}
                  placeholder="e.g. 0821234567"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Email <span className="font-normal text-gray-400">(optional)</span>
                </label>
                <input
                  type="email"
                  className="w-full border p-2 rounded text-sm"
                  value={form.email}
                  onChange={e => setForm({ ...form, email: e.target.value })}
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Date of Birth <span className="font-normal text-gray-400">(optional)</span>
                </label>
                <input
                  type="date"
                  className="w-full border p-2 rounded text-sm"
                  value={form.dateOfBirth}
                  onChange={e => setForm({ ...form, dateOfBirth: e.target.value })}
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Notes / Allergies <span className="font-normal text-gray-400">(optional)</span>
                </label>
                <textarea
                  className="w-full border p-2 rounded text-sm"
                  rows={3}
                  value={form.notes}
                  placeholder="Allergies, colour formulas, preferences…"
                  onChange={e => setForm({ ...form, notes: e.target.value })}
                />
              </div>

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
                  {editingId ? "Update" : "Add Customer"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

// ── Small reusable components ──────────────────────────────────────

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between">
      <span className="text-gray-500">{label}</span>
      <span className="text-gray-800 font-medium">{value}</span>
    </div>
  );
}

function StatBox({
  label, value, color,
}: {
  label: string;
  value: string | number;
  color: string;
}) {
  return (
    <div className={`${color} rounded-lg p-3 text-center`}>
      <p className="text-xs opacity-70 mb-1">{label}</p>
      <p className="text-lg font-bold">{value}</p>
    </div>
  );
}
