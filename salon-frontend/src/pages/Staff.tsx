// UPDATED — uses correct StaffDto field names from backend:
// - firstName + lastName (not name)
// - fullName for display
// - phone (required, not email)
// - role = Stylist|Colourist|Therapist|Manager|Receptionist (salon job role)
//   NOTE: this is NOT the system role (Owner/Reception/Staff)
import { useEffect, useState } from "react";
import api from "../api/axios";

interface StaffMember {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  phone: string;
  email: string | null;
  role: string;       // Salon job role: Stylist, Colourist, etc.
  status: string;     // Active | Inactive
}

type SalonRole = "Stylist" | "Colourist" | "Therapist" | "Manager" | "Receptionist";

const SALON_ROLES: SalonRole[] = [
  "Stylist", "Colourist", "Therapist", "Manager", "Receptionist"
];

export default function Staff() {
  const [staff,     setStaff]     = useState<StaffMember[]>([]);
  const [loading,   setLoading]   = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [error,     setError]     = useState<string | null>(null);

  const [form, setForm] = useState({
    firstName: "",
    lastName:  "",
    phone:     "",
    email:     "",
    role:      "Stylist" as SalonRole,
    status:    "Active",
  });

  useEffect(() => { fetchStaff(); }, []);

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
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      alert(axiosErr?.response?.data?.error ?? "Delete failed");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    const payload = {
      firstName:       form.firstName,
      lastName:        form.lastName,
      phone:           form.phone,
      email:           form.email || undefined,
      role:            form.role,
      status:          form.status,
      specialisations: [] as number[],
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

  if (loading) return <div className="p-6 text-gray-500">Loading staff…</div>;

  return (
    <div className="p-6">
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
            <tr key={member.id} className="border-b hover:bg-gray-50">
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

      {/* Modal */}
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
                  {SALON_ROLES.map(r => (
                    <option key={r} value={r}>{r}</option>
                  ))}
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
