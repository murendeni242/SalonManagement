import { useEffect, useState, useCallback } from "react";
import api from "../api/axios";
import { commissionService } from "../api/commissionService";
import type { CommissionSummaryDto, CommissionRuleDto } from "../api/commissionService";


// ── Types ──────────────────────────────────────────────────────────

interface StaffMember {
  id:       number;
  fullName: string;
  role:     string;
  status:   string;
}

// ── Helpers ────────────────────────────────────────────────────────

function formatCurrency(amount: number): string {
  return `R ${amount.toFixed(2)}`;
}

function currentMonthRange(): { from: string; to: string } {
  const now  = new Date();
  const from = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-01`;
  const to   = now.toISOString().split("T")[0];
  return { from, to };
}

// ── Main component ─────────────────────────────────────────────────

export default function CommissionEarnings() {
  const [staff,      setStaff]      = useState<StaffMember[]>([]);
  const [rules,      setRules]      = useState<CommissionRuleDto[]>([]);
  const [summaries,  setSummaries]  = useState<Record<number, CommissionSummaryDto>>({});
  const [loading,    setLoading]    = useState(true);
  const [paying,     setPaying]     = useState<number | null>(null);
  const [paySuccess, setPaySuccess] = useState<number | null>(null);

  const { from: defaultFrom, to: defaultTo } = currentMonthRange();
  const [fromDate, setFromDate] = useState(defaultFrom);
  const [toDate,   setToDate]   = useState(defaultTo);

  // ── Data fetching ────────────────────────────────────────────────

  const fetchAll = useCallback(async () => {
    setLoading(true);
    try {
      const [resStaff, resRules] = await Promise.all([
        api.get<StaffMember[]>("/staff"),
        commissionService.getRules(),
      ]);

      const activeStaff = resStaff.data.filter(s => s.status === "Active");
      setStaff(activeStaff);
      setRules(resRules.data);

      // Load summary for each staff member that has a commission rule
      const staffWithRules = activeStaff.filter(s =>
        resRules.data.some(r => r.staffId === s.id)
      );

      const summaryResults = await Promise.all(
        staffWithRules.map(s =>
          commissionService
            .getStaffSummary(s.id, fromDate, toDate)
            .then(res => ({ staffId: s.id, data: res.data }))
            .catch(() => null)
        )
      );

      const map: Record<number, CommissionSummaryDto> = {};
      summaryResults.forEach(r => {
        if (r) map[r.staffId] = r.data;
      });
      setSummaries(map);

    } finally {
      setLoading(false);
    }
  }, [fromDate, toDate]);

  useEffect(() => { fetchAll(); }, [fetchAll]);

  // ── Mark all paid ────────────────────────────────────────────────

  const handleMarkAllPaid = async (staffId: number) => {
    setPaying(staffId);
    setPaySuccess(null);
    try {
      await commissionService.markAllPaid(staffId);
      setPaySuccess(staffId);
      fetchAll();
      setTimeout(() => setPaySuccess(null), 3000);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      alert(e?.response?.data?.error ?? "Failed to mark commissions as paid.");
    } finally {
      setPaying(null);
    }
  };

  // ── Helpers ──────────────────────────────────────────────────────

  const getRuleDescription = (staffId: number): string => {
    const rule = rules.find(r => r.staffId === staffId);
    if (!rule) return "No rule";
    if (rule.type === "Percentage") return `${rule.rateOrAmount}%`;
    if (rule.type === "Fixed")      return `R${rule.rateOrAmount.toFixed(2)} flat`;
    if (rule.type === "Tiered")     return "Tiered %";
    return "—";
  };

  // Total pending across all staff
  const totalPendingAll = Object.values(summaries)
    .reduce((sum, s) => sum + s.totalPending, 0);

  const totalEarnedAll = Object.values(summaries)
    .reduce((sum, s) => sum + s.totalEarned, 0);

  // ── Render ───────────────────────────────────────────────────────

  if (loading) return <div className="p-6 text-gray-500">Loading earnings…</div>;

  return (
    <div className="p-6 space-y-6">

      {/* ── Header ─────────────────────────────────────────────── */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Staff Commissions</h1>
          <p className="text-sm text-gray-500 mt-1">
            View earnings and process payouts per staff member.
          </p>
        </div>
      </div>

      {/* ── Date filter ────────────────────────────────────────── */}
      <div className="flex items-center gap-3 bg-white border rounded px-4 py-3 w-fit">
        <span className="text-sm text-gray-500 font-medium">Period:</span>
        <input
          type="date"
          className="border p-1.5 rounded text-sm"
          value={fromDate}
          onChange={e => setFromDate(e.target.value)}
        />
        <span className="text-gray-400 text-sm">to</span>
        <input
          type="date"
          className="border p-1.5 rounded text-sm"
          value={toDate}
          onChange={e => setToDate(e.target.value)}
        />
      </div>

      {/* ── Period totals ───────────────────────────────────────── */}
      <div className="grid grid-cols-2 lg:grid-cols-3 gap-4">
        <div className="bg-teal-600 text-white p-5 rounded-lg shadow">
          <p className="text-xs font-medium opacity-75 uppercase tracking-wide">
            Total Earned (Period)
          </p>
          <p className="text-2xl font-bold mt-1">{formatCurrency(totalEarnedAll)}</p>
        </div>
        <div className="bg-orange-500 text-white p-5 rounded-lg shadow">
          <p className="text-xs font-medium opacity-75 uppercase tracking-wide">
            Total Pending Payout
          </p>
          <p className="text-2xl font-bold mt-1">{formatCurrency(totalPendingAll)}</p>
        </div>
        <div className="bg-gray-700 text-white p-5 rounded-lg shadow">
          <p className="text-xs font-medium opacity-75 uppercase tracking-wide">
            Staff on Commission
          </p>
          <p className="text-2xl font-bold mt-1">{Object.keys(summaries).length}</p>
        </div>
      </div>

      {/* ── Per-staff earnings table ────────────────────────────── */}
      <div className="bg-white rounded shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-100 border-b text-left">
              <th className="py-3 px-4">Staff Member</th>
              <th className="py-3 px-4">Strategy</th>
              <th className="py-3 px-4">Total Earned</th>
              <th className="py-3 px-4">Paid Out</th>
              <th className="py-3 px-4">Pending</th>
              <th className="py-3 px-4">Transactions</th>
              <th className="py-3 px-4">Action</th>
            </tr>
          </thead>
          <tbody>
            {staff.map(member => {
              const summary = summaries[member.id];
              const hasRule = rules.some(r => r.staffId === member.id);

              if (!hasRule) return null;

              return (
                <tr key={member.id} className="border-b hover:bg-gray-50">
                  <td className="py-3 px-4">
                    <div className="font-medium text-gray-800">{member.fullName}</div>
                    <div className="text-xs text-gray-400">{member.role}</div>
                  </td>
                  <td className="py-3 px-4 text-xs text-gray-600">
                    {getRuleDescription(member.id)}
                  </td>
                  <td className="py-3 px-4 font-semibold text-gray-800">
                    {summary ? formatCurrency(summary.totalEarned) : "—"}
                  </td>
                  <td className="py-3 px-4 text-green-600 font-medium">
                    {summary ? formatCurrency(summary.totalPaid) : "—"}
                  </td>
                  <td className="py-3 px-4">
                    {summary && summary.totalPending > 0 ? (
                      <span className="text-orange-600 font-semibold">
                        {formatCurrency(summary.totalPending)}
                      </span>
                    ) : (
                      <span className="text-gray-400">
                        {summary ? "R 0.00" : "—"}
                      </span>
                    )}
                  </td>
                  <td className="py-3 px-4 text-gray-500">
                    {summary?.totalRecords ?? "—"}
                  </td>
                  <td className="py-3 px-4">
                    {summary && summary.totalPending > 0 ? (
                      paySuccess === member.id ? (
                        <span className="text-green-600 text-xs font-medium">
                          ✓ Marked paid
                        </span>
                      ) : (
                        <button
                          onClick={() => handleMarkAllPaid(member.id)}
                          disabled={paying === member.id}
                          className="bg-teal-600 text-white px-3 py-1 rounded text-xs hover:bg-teal-700 disabled:opacity-50"
                        >
                          {paying === member.id ? "Processing…" : `Pay R${summary.totalPending.toFixed(2)}`}
                        </button>
                      )
                    ) : (
                      <span className="text-xs text-gray-300">No pending</span>
                    )}
                  </td>
                </tr>
              );
            })}

            {Object.keys(summaries).length === 0 && (
              <tr>
                <td colSpan={7} className="py-8 text-center text-gray-400">
                  No commission rules have been configured yet.
                  Go to Commission Settings to set up rules per staff member.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* ── Info note ──────────────────────────────────────────── */}
      <div className="bg-blue-50 border border-blue-100 rounded-lg px-4 py-3">
        <p className="text-xs text-blue-700">
          <span className="font-semibold">How payouts work: </span>
          Commissions are calculated automatically when a payment is recorded.
          Click "Pay R…" to mark all pending commissions as paid for a staff member.
          This records the payout timestamp and your name against each commission.
          It does not trigger a bank transfer — use this to track when you physically pay staff.
        </p>
      </div>
    </div>
  );
}
