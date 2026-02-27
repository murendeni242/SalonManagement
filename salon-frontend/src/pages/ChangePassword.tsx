// Shown when mustChangePassword === true after login.
// User cannot access any other page until they set a new password.
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/axios";
import { getUserEmail } from "../auth/authUtils";

export default function ChangePassword() {
  const navigate   = useNavigate();
  const email      = getUserEmail();

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword,     setNewPassword]     = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error,           setError]           = useState<string | null>(null);
  const [loading,         setLoading]         = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (newPassword.length < 8) {
      setError("New password must be at least 8 characters.");
      return;
    }
    if (newPassword !== confirmPassword) {
      setError("New passwords do not match.");
      return;
    }
    if (newPassword === currentPassword) {
      setError("New password must be different from your current password.");
      return;
    }

    setLoading(true);
    try {
      await api.post("/users/change-password", {
        currentPassword,
        newPassword,
      });

      // Update the stored auth object — clear the mustChangePassword flag
      const raw = localStorage.getItem("salon_auth");
      if (raw) {
        const auth = JSON.parse(raw);
        auth.mustChangePassword = false;
        localStorage.setItem("salon_auth", JSON.stringify(auth));
      }

      navigate("/dashboard");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e?.response?.data?.error ?? "Password change failed. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center">
      <div className="bg-white rounded-lg shadow p-8 w-[420px]">

        {/* Header */}
        <div className="mb-6">
          <h1 className="text-2xl font-bold">Set Your Password</h1>
          <p className="text-sm text-gray-500 mt-1">
            You're logged in as <strong>{email}</strong>.
            <br />
            You must set a new password before continuing.
          </p>
        </div>

        {/* Info banner */}
        <div className="bg-blue-50 border border-blue-200 rounded px-3 py-2 mb-5 text-sm text-blue-700">
          Your current password is the temporary one given to you by the salon owner.
          Choose something only you know.
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded mb-4">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">
              Current (Temporary) Password *
            </label>
            <input
              type="password"
              className="w-full border p-2 rounded text-sm"
              value={currentPassword}
              onChange={e => setCurrentPassword(e.target.value)}
              autoComplete="current-password"
              required
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">
              New Password * <span className="font-normal text-gray-400">(min 8 characters)</span>
            </label>
            <input
              type="password"
              className="w-full border p-2 rounded text-sm"
              value={newPassword}
              onChange={e => setNewPassword(e.target.value)}
              autoComplete="new-password"
              required
            />
            {/* Simple strength indicator */}
            {newPassword.length > 0 && (
              <div className="mt-1.5 flex gap-1">
                {[1, 2, 3, 4].map(level => (
                  <div
                    key={level}
                    className={`h-1 flex-1 rounded-full ${
                      passwordStrength(newPassword) >= level
                        ? level <= 1 ? "bg-red-400"
                          : level <= 2 ? "bg-orange-400"
                          : level <= 3 ? "bg-yellow-400"
                          : "bg-green-500"
                        : "bg-gray-200"
                    }`}
                  />
                ))}
                <span className="text-xs text-gray-400 ml-1">
                  {["", "Weak", "Fair", "Good", "Strong"][passwordStrength(newPassword)]}
                </span>
              </div>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">
              Confirm New Password *
            </label>
            <input
              type="password"
              className="w-full border p-2 rounded text-sm"
              value={confirmPassword}
              onChange={e => setConfirmPassword(e.target.value)}
              autoComplete="new-password"
              required
            />
            {confirmPassword && newPassword !== confirmPassword && (
              <p className="text-xs text-red-500 mt-1">Passwords do not match.</p>
            )}
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-black text-white py-2 rounded hover:bg-gray-800 disabled:opacity-60 text-sm mt-2"
          >
            {loading ? "Saving…" : "Set New Password & Continue"}
          </button>
        </form>

        <button
          onClick={() => {
            localStorage.removeItem("salon_auth");
            window.location.replace("/login");
          }}
          className="w-full mt-3 text-sm text-gray-400 hover:text-gray-600"
        >
          Log out instead
        </button>
      </div>
    </div>
  );
}

// Simple password strength score 1–4
function passwordStrength(password: string): number {
  let score = 0;
  if (password.length >= 8)  score++;
  if (/[A-Z]/.test(password)) score++;
  if (/[0-9]/.test(password)) score++;
  if (/[^A-Za-z0-9]/.test(password)) score++;
  return score;
}
