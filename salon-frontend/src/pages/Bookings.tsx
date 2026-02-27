import { useEffect, useState } from "react";
import api from "../api/axios";
import { getUserRole } from "../auth/authUtils";

interface Booking {
  id: number;
  customerId: number;
  staffId: number;
  serviceId: number;
  bookingDate: string;
  startTime: string;
  endTime: string;
  totalPrice: number;
  status: string;
  notes: string | null;
}

interface StaffMember { id: number; fullName: string; }
interface Service     { id: number; name: string; durationMinutes: number; }
interface Customer    { id: number; fullName: string; phone: string; }

export default function Bookings() {
  const role = getUserRole();

  const [bookings,     setBookings]     = useState<Booking[]>([]);
  const [staffList,    setStaffList]    = useState<StaffMember[]>([]);
  const [servicesList, setServicesList] = useState<Service[]>([]);
  const [customers,    setCustomers]    = useState<Customer[]>([]);
  const [loading,      setLoading]      = useState(true);
  const [modalOpen,    setModalOpen]    = useState(false);
  const [editingId,    setEditingId]    = useState<number | null>(null);
  const [error,        setError]        = useState<string | null>(null);

  const [form, setForm] = useState({
    customerId:  0,
    staffId:     0,
    serviceId:   0,
    bookingDate: "",
    startTime:   "09:00",
    notes:       "",
  });

  useEffect(() => { fetchAll(); }, []);

  const fetchAll = async () => {
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
      setServicesList(resSv.data);
      setCustomers(resC.data);
    } finally {
      setLoading(false);
    }
  };

  const openAdd = () => {
    setEditingId(null);
    setError(null);
    setForm({ customerId: 0, staffId: 0, serviceId: 0, bookingDate: "", startTime: "09:00", notes: "" });
    setModalOpen(true);
  };

  const openEdit = (b: Booking) => {
    setEditingId(b.id);
    setError(null);
    setForm({
      customerId:  b.customerId,
      staffId:     b.staffId,
      serviceId:   b.serviceId,
      bookingDate: b.bookingDate.substring(0, 10),
      startTime:   b.startTime.substring(0, 5),
      notes:       b.notes ?? "",
    });
    setModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!form.customerId || !form.staffId || !form.serviceId || !form.bookingDate) {
      setError("Please fill in all required fields.");
      return;
    }

    const payload = {
      customerId:  form.customerId,
      staffId:     form.staffId,
      serviceId:   form.serviceId,
      bookingDate: form.bookingDate,
      startTime:   `${form.startTime}:00`,
      notes:       form.notes || undefined,
    };

    try {
      if (editingId) {
        await api.put(`/bookings/${editingId}`, payload);
      } else {
        await api.post("/bookings", payload);
      }
      setModalOpen(false);
      fetchAll();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      setError(axiosErr?.response?.data?.error ?? "Operation failed");
    }
  };

  const handleConfirm = async (id: number) => {
    try {
      await api.post(`/bookings/${id}/confirm`);
      fetchAll();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      alert(axiosErr?.response?.data?.error ?? "Could not confirm booking");
    }
  };

  const handleComplete = async (id: number) => {
    if (!confirm("Mark this booking as completed?")) return;
    try {
      await api.post(`/bookings/${id}/complete`);
      fetchAll();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      alert(axiosErr?.response?.data?.error ?? "Could not complete booking");
    }
  };

  const handleCancel = async (id: number) => {
    if (!confirm("Cancel this booking?")) return;
    try {
      await api.post(`/bookings/${id}/cancel`);
      fetchAll();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      alert(axiosErr?.response?.data?.error ?? "Could not cancel booking");
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm("Delete this booking? This cannot be undone.")) return;
    try {
      await api.delete(`/bookings/${id}`);
      fetchAll();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      alert(axiosErr?.response?.data?.error ?? "Delete failed");
    }
  };

  const statusBadge = (status: string) => {
    const map: Record<string, string> = {
      Pending:   "bg-yellow-100 text-yellow-700",
      Confirmed: "bg-blue-100 text-blue-700",
      Completed: "bg-green-100 text-green-700",
      Cancelled: "bg-red-100 text-red-700",
    };
    return map[status] ?? "bg-gray-100 text-gray-600";
  };

  const customerName = (id: number) =>
    customers.find(c => c.id === id)?.fullName ?? `Customer #${id}`;

  const staffName = (id: number) =>
    staffList.find(s => s.id === id)?.fullName ?? `Staff #${id}`;

  const serviceName = (id: number) =>
    servicesList.find(s => s.id === id)?.name ?? `Service #${id}`;

  if (loading) return <div className="p-6 text-gray-500">Loading bookings…</div>;

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">
          {role === "Staff" ? "My Bookings" : "Bookings"}
        </h1>
        {(role === "Owner" || role === "Reception") && (
          <button
            onClick={openAdd}
            className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
          >
            New Booking
          </button>
        )}
      </div>

      <table className="w-full bg-white rounded shadow">
        <thead>
          <tr className="bg-gray-100 border-b text-left text-sm">
            <th className="py-3 px-4">Customer</th>
            <th className="py-3 px-4">Staff</th>
            <th className="py-3 px-4">Service</th>
            <th className="py-3 px-4">Date</th>
            <th className="py-3 px-4">Time</th>
            <th className="py-3 px-4">Price</th>
            <th className="py-3 px-4">Status</th>
            {(role === "Owner" || role === "Reception") && (
              <th className="py-3 px-4">Actions</th>
            )}
          </tr>
        </thead>
        <tbody>
          {bookings.map(b => (
            <tr key={b.id} className="border-b hover:bg-gray-50 text-sm">
              <td className="py-3 px-4">{customerName(b.customerId)}</td>
              <td className="py-3 px-4">{staffName(b.staffId)}</td>
              <td className="py-3 px-4">{serviceName(b.serviceId)}</td>
              <td className="py-3 px-4">
                {new Date(b.bookingDate).toLocaleDateString("en-ZA")}
              </td>
              <td className="py-3 px-4">
                {b.startTime.substring(0, 5)} – {b.endTime.substring(0, 5)}
              </td>
              <td className="py-3 px-4">R {b.totalPrice.toFixed(2)}</td>
              <td className="py-3 px-4">
                <span className={`text-xs px-2 py-1 rounded-full font-medium ${statusBadge(b.status)}`}>
                  {b.status}
                </span>
              </td>
              {(role === "Owner" || role === "Reception") && (
                <td className="py-3 px-4">
                  <div className="flex gap-1 flex-wrap">

                    {/* Pending → Confirm */}
                    {b.status === "Pending" && (
                      <button
                        onClick={() => handleConfirm(b.id)}
                        className="bg-blue-500 text-white px-2 py-1 rounded text-xs hover:bg-blue-600"
                      >
                        Confirm
                      </button>
                    )}

                    {/* Confirmed → Complete */}
                    {b.status === "Confirmed" && (
                      <button
                        onClick={() => handleComplete(b.id)}
                        className="bg-green-600 text-white px-2 py-1 rounded text-xs hover:bg-green-700"
                      >
                        Complete
                      </button>
                    )}

                    {/* Pending or Confirmed → Edit / Cancel */}
                    {(b.status === "Pending" || b.status === "Confirmed") && (
                      <>
                        <button
                          onClick={() => openEdit(b)}
                          className="bg-gray-500 text-white px-2 py-1 rounded text-xs hover:bg-gray-600"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleCancel(b.id)}
                          className="bg-orange-500 text-white px-2 py-1 rounded text-xs hover:bg-orange-600"
                        >
                          Cancel
                        </button>
                      </>
                    )}

                    {/* Owner only → Delete */}
                    {role === "Owner" && (
                      <button
                        onClick={() => handleDelete(b.id)}
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
          {bookings.length === 0 && (
            <tr>
              <td colSpan={8} className="py-8 text-center text-gray-400 text-sm">
                No bookings yet. Click "New Booking" to create one.
              </td>
            </tr>
          )}
        </tbody>
      </table>

      {/* Modal */}
      {modalOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-[460px] shadow-xl">
            <h2 className="text-xl font-bold mb-4">
              {editingId ? "Edit Booking" : "New Booking"}
            </h2>

            {error && (
              <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded">
                {error}
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-3">
              <div>
                <label className="block text-sm font-medium mb-1">Customer *</label>
                <select
                  className="w-full border p-2 rounded text-sm"
                  value={form.customerId || ""}
                  onChange={e => setForm({ ...form, customerId: Number(e.target.value) })}
                  required
                >
                  <option value="">Select customer…</option>
                  {customers.map(c => (
                    <option key={c.id} value={c.id}>
                      {c.fullName} — {c.phone}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Staff *</label>
                <select
                  className="w-full border p-2 rounded text-sm"
                  value={form.staffId || ""}
                  onChange={e => setForm({ ...form, staffId: Number(e.target.value) })}
                  required
                >
                  <option value="">Select staff member…</option>
                  {staffList.map(s => (
                    <option key={s.id} value={s.id}>{s.fullName}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Service *</label>
                <select
                  className="w-full border p-2 rounded text-sm"
                  value={form.serviceId || ""}
                  onChange={e => setForm({ ...form, serviceId: Number(e.target.value) })}
                  required
                >
                  <option value="">Select service…</option>
                  {servicesList.map(s => (
                    <option key={s.id} value={s.id}>
                      {s.name} ({s.durationMinutes} min)
                    </option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium mb-1">Date *</label>
                  <input
                    type="date"
                    className="w-full border p-2 rounded text-sm"
                    value={form.bookingDate}
                    onChange={e => setForm({ ...form, bookingDate: e.target.value })}
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Start Time *</label>
                  <input
                    type="time"
                    className="w-full border p-2 rounded text-sm"
                    value={form.startTime}
                    onChange={e => setForm({ ...form, startTime: e.target.value })}
                    required
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Notes</label>
                <textarea
                  className="w-full border p-2 rounded text-sm"
                  rows={2}
                  value={form.notes}
                  placeholder="Any notes about this appointment…"
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
                  {editingId ? "Update" : "Create Booking"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
