// - List all user accounts with role, status, last login
// - Create account → system generates password → shown ONCE in a modal
// - Reset password → new password shown ONCE
// - Deactivate / Reactivate account
// - Delete account (with confirmation)
// - Role badge colours

import { useEffect, useState, useCallback } from "react";
import api from "../api/axios";

// ── Types ──────────────────────────────────────────────────────────

interface SystemUser {
  id: number;
  email: string;
  role: string;             // Owner | Reception | Staff
  status: string;           // Active | Inactive
  mustChangePassword: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

// ── Helpers ────────────────────────────────────────────────────────

function formatDateTime(iso: string | null): string {
  if (!iso) return "Never";
  return new Date(iso).toLocaleString("en-ZA", {
    day: "numeric", month: "short", year: "numeric",
    hour: "2-digit", minute: "2-digit",
  });
}

const ROLE_CLASS: Record<string, string> = {
  Owner:     "bg-purple-100 text-purple-700",
  Reception: "bg-blue-100 text-blue-700",
  Staff:     "bg-gray-100 text-gray-600",
};

// ── Main component ─────────────────────────────────────────────────

export default function Users() {
  const [users,   setUsers]   = useState<SystemUser[]>([]);
  const [loading, setLoading] = useState(true);

  // Create modal
  const [createOpen,  setCreateOpen]  = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [createForm,  setCreateForm]  = useState({ email: "", role: "Reception" });

  // Credential reveal modal (shown after create or reset)
  const [credModalOpen,     setCredModalOpen]     = useState(false);
  const [credEmail,         setCredEmail]         = useState("");
  const [credPassword,      setCredPassword]      = useState("");
  const [credCopied,        setCredCopied]        = useState(false);

  // ── Fetch ────────────────────────────────────────────────────────

  const fetchUsers = useCallback(async () => {
    setLoading(true);
    try {
      const res = await api.get<SystemUser[]>("/users");
      setUsers(res.data);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchUsers(); }, [fetchUsers]);

  // ── Create account ───────────────────────────────────────────────

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setCreateError(null);

    try {
      const res = await api.post<{
        user: SystemUser;
        generatedPassword: string;
      }>("/users", createForm);

      setCreateOpen(false);
      fetchUsers();

      // Show credentials — this is the only time the password is visible
      setCredEmail(res.data.user.email);
      setCredPassword(res.data.generatedPassword);
      setCredCopied(false);
      setCredModalOpen(true);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setCreateError(e?.response?.data?.error ?? "Failed to create user");
    }
  };

  // ── Reset password ───────────────────────────────────────────────

  const handleResetPassword = async (user: SystemUser) => {
    if (!confirm(`Reset password for ${user.email}? A new password will be generated.`)) return;

    try {
      const res = await api.post<{ generatedPassword: string }>(
        `/users/${user.id}/reset-password`
      );

      // Show the new credentials
      setCredEmail(user.email);
      setCredPassword(res.data.generatedPassword);
      setCredCopied(false);
      setCredModalOpen(true);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      alert(e?.response?.data?.error ?? "Password reset failed");
    }
  };

  // ── Toggle status ────────────────────────────────────────────────

  const handleToggleStatus = async (user: SystemUser) => {
    const newStatus = user.status === "Active" ? "Inactive" : "Active";
    const verb      = newStatus === "Inactive" ? "deactivate" : "reactivate";

    if (!confirm(`Are you sure you want to ${verb} ${user.email}?`)) return;

    try {
      await api.patch(`/users/${user.id}/status`, { status: newStatus });
      fetchUsers();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      alert(e?.response?.data?.error ?? "Status update failed");
    }
  };

  // ── Delete ───────────────────────────────────────────────────────

  const handleDelete = async (user: SystemUser) => {
    if (!confirm(
      `Permanently delete ${user.email}?\n\nThis cannot be undone. Their booking history will be kept.`
    )) return;

    try {
      await api.delete(`/users/${user.id}`);
      fetchUsers();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      alert(e?.response?.data?.error ?? "Delete failed");
    }
  };

  // ── Copy credentials to clipboard ───────────────────────────────

  const copyCredentials = () => {
    const text = `Email: ${credEmail}\nPassword: ${credPassword}`;
    navigator.clipboard.writeText(text).then(() => {
      setCredCopied(true);
      setTimeout(() => setCredCopied(false), 2500);
    });
  };

  // ── Render ───────────────────────────────────────────────────────

  if (loading) return <div className="p-6 text-gray-500">Loading users…</div>;

  return (
    <div className="p-6 space-y-6">

      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">User Accounts</h1>
          <p className="text-sm text-gray-500 mt-0.5">
            Manage who can log in to this system.
          </p>
        </div>
        <button
          onClick={() => {
            setCreateForm({ email: "", role: "Reception" });
            setCreateError(null);
            setCreateOpen(true);
          }}
          className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 text-sm"
        >
          Create Account
        </button>
      </div>

      {/* Users table */}
      <div className="bg-white rounded shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-100 border-b text-left">
              <th className="py-3 px-4">Email</th>
              <th className="py-3 px-4">Role</th>
              <th className="py-3 px-4">Status</th>
              <th className="py-3 px-4">Last Login</th>
              <th className="py-3 px-4">Created</th>
              <th className="py-3 px-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map(user => (
              <tr
                key={user.id}
                className={`border-b ${user.status === "Inactive" ? "opacity-60" : "hover:bg-gray-50"}`}
              >
                <td className="py-3 px-4">
                  <div>
                    <span className="font-medium">{user.email}</span>
                    {user.mustChangePassword && (
                      <span className="ml-2 text-xs bg-amber-100 text-amber-700 px-1.5 py-0.5 rounded">
                        Must change password
                      </span>
                    )}
                  </div>
                </td>

                <td className="py-3 px-4">
                  <span className={`text-xs px-2 py-1 rounded-full font-medium ${
                    ROLE_CLASS[user.role] ?? "bg-gray-100 text-gray-600"
                  }`}>
                    {user.role}
                  </span>
                </td>

                <td className="py-3 px-4">
                  <span className={`text-xs px-2 py-1 rounded-full font-medium ${
                    user.status === "Active"
                      ? "bg-green-100 text-green-700"
                      : "bg-red-100 text-red-600"
                  }`}>
                    {user.status}
                  </span>
                </td>

                <td className="py-3 px-4 text-gray-500">
                  {formatDateTime(user.lastLoginAt)}
                </td>

                <td className="py-3 px-4 text-gray-500">
                  {formatDateTime(user.createdAt)}
                </td>

                <td className="py-3 px-4">
                  <div className="flex gap-1 flex-wrap">
                    <button
                      onClick={() => handleResetPassword(user)}
                      className="bg-blue-500 text-white px-2 py-1 rounded text-xs hover:bg-blue-600"
                      title="Generate a new password"
                    >
                      Reset Password
                    </button>

                    <button
                      onClick={() => handleToggleStatus(user)}
                      className={`px-2 py-1 rounded text-xs text-white ${
                        user.status === "Active"
                          ? "bg-orange-500 hover:bg-orange-600"
                          : "bg-green-600 hover:bg-green-700"
                      }`}
                    >
                      {user.status === "Active" ? "Deactivate" : "Reactivate"}
                    </button>

                    <button
                      onClick={() => handleDelete(user)}
                      className="bg-red-500 text-white px-2 py-1 rounded text-xs hover:bg-red-600"
                    >
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}

            {users.length === 0 && (
              <tr>
                <td colSpan={6} className="py-8 text-center text-gray-400">
                  No user accounts yet. Click "Create Account" to add one.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* ── Create Account Modal ─────────────────────────────────── */}
      {createOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-[420px] shadow-xl">
            <h2 className="text-xl font-bold mb-1">Create User Account</h2>
            <p className="text-sm text-gray-500 mb-4">
              A secure password will be generated automatically. You'll see it once to share with the new user.
            </p>

            {createError && (
              <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded">
                {createError}
              </div>
            )}

            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">Email Address *</label>
                <input
                  type="email"
                  className="w-full border p-2 rounded text-sm"
                  value={createForm.email}
                  onChange={e => setCreateForm({ ...createForm, email: e.target.value })}
                  placeholder="staff@yoursalon.co.za"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-2">System Role *</label>
                <div className="grid grid-cols-3 gap-2">
                  {["Owner", "Reception", "Staff"].map(role => (
                    <button
                      key={role}
                      type="button"
                      onClick={() => setCreateForm({ ...createForm, role })}
                      className={`py-2 rounded border text-sm font-medium transition-colors ${
                        createForm.role === role
                          ? "bg-black text-white border-black"
                          : "border-gray-300 hover:bg-gray-50"
                      }`}
                    >
                      {role}
                    </button>
                  ))}
                </div>
                <p className="text-xs text-gray-400 mt-2">
                  <strong>Owner</strong> — full access.{" "}
                  <strong>Reception</strong> — bookings + customers.{" "}
                  <strong>Staff</strong> — own schedule only.
                </p>
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setCreateOpen(false)}
                  className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 text-sm"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 rounded bg-green-600 text-white hover:bg-green-700 text-sm"
                >
                  Create Account
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ── Credentials Modal (shown ONCE after create or reset) ──── */}
      {credModalOpen && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-[440px] shadow-2xl">

            {/* Warning header */}
            <div className="bg-amber-50 border border-amber-300 rounded-lg px-4 py-3 mb-5">
              <p className="text-amber-800 font-semibold text-sm">
                ⚠ Save these credentials now
              </p>
              <p className="text-amber-700 text-xs mt-0.5">
                This password will not be shown again. Share it with the user directly.
              </p>
            </div>

            <h2 className="text-lg font-bold mb-4">New Login Credentials</h2>

            {/* Credential display */}
            <div className="bg-gray-50 rounded-lg border p-4 space-y-3 font-mono text-sm mb-5">
              <div>
                <span className="text-gray-400 text-xs block mb-0.5">Email</span>
                <span className="font-semibold text-gray-800">{credEmail}</span>
              </div>
              <div className="border-t pt-3">
                <span className="text-gray-400 text-xs block mb-0.5">Temporary Password</span>
                <span className="font-bold text-lg tracking-wider text-gray-900">
                  {credPassword}
                </span>
              </div>
            </div>

            <p className="text-xs text-gray-500 mb-4">
              The user will be prompted to change their password when they first log in.
            </p>

            <div className="flex gap-2">
              <button
                onClick={copyCredentials}
                className={`flex-1 py-2 rounded text-sm font-medium transition-colors ${
                  credCopied
                    ? "bg-green-600 text-white"
                    : "bg-gray-800 text-white hover:bg-gray-700"
                }`}
              >
                {credCopied ? "✓ Copied!" : "Copy to Clipboard"}
              </button>
              <button
                onClick={() => setCredModalOpen(false)}
                className="flex-1 py-2 rounded text-sm font-medium bg-gray-200 hover:bg-gray-300"
              >
                Done
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
