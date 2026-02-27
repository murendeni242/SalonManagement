// UPDATED — field names match backend ServiceDto exactly.
// Owner only: Create + Edit + Delete.
// All roles: View list.
import { useEffect, useState } from "react";
import api from "../api/axios";
import { getUserRole } from "../auth/authUtils";

interface Service {
  id: number;
  name: string;
  description: string;
  durationMinutes: number;
  basePrice: number;
  status: string;   // Active | Inactive
}

export default function Services() {
  const role = getUserRole();

  const [services,  setServices]  = useState<Service[]>([]);
  const [loading,   setLoading]   = useState(true);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [error,     setError]     = useState<string | null>(null);

  const [form, setForm] = useState({
    name:            "",
    description:     "",
    durationMinutes: 30,
    basePrice:       0,
    status:          "Active",
  });

  useEffect(() => { fetchServices(); }, []);

  const fetchServices = async () => {
    setLoading(true);
    try {
      const res = await api.get<Service[]>("/services");
      setServices(res.data);
    } finally {
      setLoading(false);
    }
  };

  const openAdd = () => {
    setEditingId(null);
    setError(null);
    setForm({ name: "", description: "", durationMinutes: 30, basePrice: 0, status: "Active" });
    setModalOpen(true);
  };

  const openEdit = (service: Service) => {
    setEditingId(service.id);
    setError(null);
    setForm({
      name:            service.name,
      description:     service.description,
      durationMinutes: service.durationMinutes,
      basePrice:       service.basePrice,
      status:          service.status,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!confirm("Delete this service? It will be hidden but historical bookings are kept.")) return;
    try {
      await api.delete(`/services/${id}`);
      fetchServices();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      alert(axiosErr?.response?.data?.error ?? "Delete failed");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      if (editingId) {
        await api.put(`/services/${editingId}`, form);
      } else {
        await api.post("/services", {
          name:            form.name,
          description:     form.description,
          durationMinutes: form.durationMinutes,
          basePrice:       form.basePrice,
        });
      }
      setModalOpen(false);
      fetchServices();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } };
      setError(axiosErr?.response?.data?.error ?? "Operation failed");
    }
  };

  if (loading) return <div className="p-6 text-gray-500">Loading services…</div>;

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Services</h1>
        {role === "Owner" && (
          <button
            onClick={openAdd}
            className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700"
          >
            Add Service
          </button>
        )}
      </div>

      <table className="w-full bg-white rounded shadow">
        <thead>
          <tr className="bg-gray-100 border-b text-left">
            <th className="py-3 px-4">Name</th>
            <th className="py-3 px-4">Duration</th>
            <th className="py-3 px-4">Price</th>
            <th className="py-3 px-4">Status</th>
            {role === "Owner" && <th className="py-3 px-4">Actions</th>}
          </tr>
        </thead>
        <tbody>
          {services.map(s => (
            <tr key={s.id} className="border-b hover:bg-gray-50">
              <td className="py-3 px-4 font-medium">{s.name}</td>
              <td className="py-3 px-4 text-sm">{s.durationMinutes} min</td>
              <td className="py-3 px-4 text-sm">R {s.basePrice.toFixed(2)}</td>
              <td className="py-3 px-4">
                <span className={`text-xs px-2 py-1 rounded-full font-medium ${
                  s.status === "Active"
                    ? "bg-green-100 text-green-700"
                    : "bg-gray-100 text-gray-500"
                }`}>
                  {s.status}
                </span>
              </td>
              {role === "Owner" && (
                <td className="py-3 px-4 space-x-2">
                  <button
                    onClick={() => openEdit(s)}
                    className="bg-blue-500 text-white px-3 py-1 rounded text-sm hover:bg-blue-600"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleDelete(s.id)}
                    className="bg-red-500 text-white px-3 py-1 rounded text-sm hover:bg-red-600"
                  >
                    Delete
                  </button>
                </td>
              )}
            </tr>
          ))}
          {services.length === 0 && (
            <tr>
              <td colSpan={5} className="py-8 text-center text-gray-400 text-sm">
                No services yet. Click "Add Service" to create one.
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
              {editingId ? "Edit Service" : "Add Service"}
            </h2>

            {error && (
              <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded">
                {error}
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-3">
              <div>
                <label className="block text-sm font-medium mb-1">Name *</label>
                <input
                  className="w-full border p-2 rounded"
                  value={form.name}
                  onChange={e => setForm({ ...form, name: e.target.value })}
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">Description</label>
                <textarea
                  className="w-full border p-2 rounded text-sm"
                  rows={2}
                  value={form.description}
                  onChange={e => setForm({ ...form, description: e.target.value })}
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium mb-1">Duration (mins) *</label>
                  <input
                    type="number"
                    min={5}
                    className="w-full border p-2 rounded"
                    value={form.durationMinutes}
                    onChange={e => setForm({ ...form, durationMinutes: Number(e.target.value) })}
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Price (R) *</label>
                  <input
                    type="number"
                    min={0}
                    step={0.01}
                    className="w-full border p-2 rounded"
                    value={form.basePrice}
                    onChange={e => setForm({ ...form, basePrice: Number(e.target.value) })}
                    required
                  />
                </div>
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
                  {editingId ? "Update" : "Add Service"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
