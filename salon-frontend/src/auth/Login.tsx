import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/axios";

interface LoginResponse {
  token: string;
  role: string;
  email: string;
  expiresAt: string;
  mustChangePassword: boolean;
}

export default function Login() {
  const [email,    setEmail]    = useState("");
  const [password, setPassword] = useState("");
  const [error,    setError]    = useState<string | null>(null);
  const [loading,  setLoading]  = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await api.post<LoginResponse>("/auth/login", { email, password });
      const { token, role, email: userEmail, expiresAt, mustChangePassword } = res.data;
      localStorage.setItem("salon_auth", JSON.stringify({ token, role, email: userEmail, expiresAt, mustChangePassword }));
      navigate(mustChangePassword ? "/change-password" : "/dashboard");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e?.response?.data?.error ?? "Invalid email or password.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-[#1e2a3a] flex items-center justify-center">
      <div className="bg-[#253447] rounded-xl p-10 w-[380px] shadow-2xl border border-white/[0.07]">

        {/* Logo */}
        <div className="text-center mb-8">
          <div className="w-13 h-13 bg-teal-500 rounded-xl inline-flex items-center justify-center mb-3">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round">
              <path d="M6 3a3 3 0 1 0 0 6 3 3 0 0 0 0-6zm12 12a3 3 0 1 0 0 6 3 3 0 0 0 0-6zM20 4L8.12 15.88M14.47 14.48L20 20M8.12 8.12L12 12" />
            </svg>
          </div>
          <h1 className="text-2xl font-bold text-white">
            Salon<span className="text-teal-400">System</span>
          </h1>
          <p className="text-sm text-white/40 mt-1">Sign in to your account</p>
        </div>

        {error && (
          <div className="bg-red-500/15 border border-red-500/30 text-red-400 text-sm px-4 py-2.5 rounded-lg mb-5">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-xs font-semibold text-white/60 mb-1.5 uppercase tracking-wide">
              Email Address
            </label>
            <input
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
              placeholder="owner@salon.co.za"
              className="w-full px-4 py-2.5 bg-white/[0.06] border border-white/10 rounded-lg text-white text-sm placeholder-white/20 outline-none focus:border-teal-400/60 transition-colors"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold text-white/60 mb-1.5 uppercase tracking-wide">
              Password
            </label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
              placeholder="••••••••"
              className="w-full px-4 py-2.5 bg-white/[0.06] border border-white/10 rounded-lg text-white text-sm placeholder-white/20 outline-none focus:border-teal-400/60 transition-colors"
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-2.5 bg-teal-500 hover:bg-teal-600 disabled:bg-teal-700 disabled:opacity-60 text-white font-bold rounded-lg text-sm transition-colors mt-2"
          >
            {loading ? "Signing in…" : "Sign In"}
          </button>
        </form>

      </div>
    </div>
  );
}
