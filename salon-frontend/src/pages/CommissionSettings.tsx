import { useEffect, useState } from "react";
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

type CommissionType = "Percentage" | "Fixed" | "Tiered";

interface TierRow {
  minServices: number;
  maxServices: number | null;
  percentage:  number;
}

// ── Helpers ────────────────────────────────────────────────────────

const TYPE_LABELS: Record<CommissionType, string> = {
  Percentage: "Percentage %",
  Fixed:      "Fixed Amount",
  Tiered:     "Tiered %",
};

const TYPE_COLORS: Record<string, string> = {
  Percentage: "bg-blue-100 text-blue-700",
  Fixed:      "bg-green-100 text-green-700",
  Tiered:     "bg-purple-100 text-purple-700",
  None:       "bg-gray-100 text-gray-500",
};

const TYPE_NUM: Record<CommissionType, number> = {
  Percentage: 1,
  Fixed:      2,
  Tiered:     3,
};

// ── Main component ─────────────────────────────────────────────────

export default function CommissionSettings() {
  const [staff,   setStaff]   = useState<StaffMember[]>([]);
  const [rules,   setRules]   = useState<CommissionRuleDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Modal state
  const [modalOpen,   setModalOpen]   = useState(false);
  const [editingStaff, setEditingStaff] = useState<StaffMember | null>(null);
  const [error,       setError]       = useState<string | null>(null);
  const [saving,      setSaving]      = useState(false);

  // Form state
  const [selectedType, setSelectedType] = useState<CommissionType>("Percentage");
  const [rateOrAmount, setRateOrAmount] = useState<number>(0);
  const [tiers,        setTiers]        = useState<TierRow[]>([
    { minServices: 0,  maxServices: 10,   percentage: 30 },
    { minServices: 11, maxServices: 30,   percentage: 40 },
    { minServices: 31, maxServices: null, percentage: 50 },
  ]);

  // ── Data fetching ────────────────────────────────────────────────

  useEffect(() => {
    fetchAll();
  }, []);

  const fetchAll = async () => {
    setLoading(true);
    try {
      const [resStaff, resRules] = await Promise.all([
        api.get<StaffMember[]>("/staff"),
        commissionService.getRules(),
      ]);
      setStaff(resStaff.data.filter(s => s.status === "Active"));
      setRules(resRules.data);
    } finally {
      setLoading(false);
    }
  };

  // ── Rule lookup ──────────────────────────────────────────────────

  const getRuleForStaff = (staffId: number): CommissionRuleDto | undefined =>
    rules.find(r => r.staffId === staffId);

  const ruleDescription = (rule: CommissionRuleDto | undefined): string => {
    if (!rule) return "No rule set";
    if (rule.type === "Percentage") return `${rule.rateOrAmount}% per payment`;
    if (rule.type === "Fixed")      return `R${rule.rateOrAmount.toFixed(2)} per service`;
    if (rule.type === "Tiered") {
      const tiers = rule.tiers
        .sort((a, b) => a.minServices - b.minServices)
        .map(t => `${t.percentage}%`)
        .join(" / ");
      return `Tiered: ${tiers}`;
    }
    return "Unknown";
  };

  // ── Modal open ───────────────────────────────────────────────────

  const openModal = (member: StaffMember) => {
    setEditingStaff(member);
    setError(null);

    const existing = getRuleForStaff(member.id);
    if (existing) {
      setSelectedType(existing.type as CommissionType);
      setRateOrAmount(existing.rateOrAmount);
      if (existing.type === "Tiered" && existing.tiers.length > 0) {
        setTiers(existing.tiers.map(t => ({
          minServices: t.minServices,
          maxServices: t.maxServices,
          percentage:  t.percentage,
        })));
      }
    } else {
      setSelectedType("Percentage");
      setRateOrAmount(40);
      setTiers([
        { minServices: 0,  maxServices: 10,   percentage: 30 },
        { minServices: 11, maxServices: 30,   percentage: 40 },
        { minServices: 31, maxServices: null, percentage: 50 },
      ]);
    }

    setModalOpen(true);
  };

  // ── Tier helpers ─────────────────────────────────────────────────

  const addTier = () => {
    const last = tiers[tiers.length - 1];
    const newMin = last ? (last.maxServices ?? 30) + 1 : 0;
    setTiers([...tiers, { minServices: newMin, maxServices: null, percentage: 40 }]);
  };

  const removeTier = (index: number) => {
    if (tiers.length <= 1) return;
    setTiers(tiers.filter((_, i) => i !== index));
  };

  const updateTier = (index: number, field: keyof TierRow, value: number | null) => {
    setTiers(tiers.map((t, i) => i === index ? { ...t, [field]: value } : t));
  };

  // ── Save rule ────────────────────────────────────────────────────

  const handleSave = async () => {
    if (!editingStaff) return;
    setError(null);
    setSaving(true);

    try {
      if (selectedType !== "Tiered" && (rateOrAmount <= 0)) {
        setError("Rate or amount must be greater than zero.");
        return;
      }
      if (selectedType === "Percentage" && rateOrAmount > 100) {
        setError("Percentage cannot exceed 100.");
        return;
      }
      if (selectedType === "Tiered" && tiers.some(t => t.percentage <= 0)) {
        setError("All tier percentages must be greater than zero.");
        return;
      }

      await commissionService.upsertRule(editingStaff.id, {
        type:         TYPE_NUM[selectedType],
        rateOrAmount: selectedType === "Tiered" ? 0 : rateOrAmount,
        tiers:        selectedType === "Tiered" ? tiers : [],
      });

      setModalOpen(false);
      fetchAll();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setError(e?.response?.data?.error ?? "Failed to save commission rule.");
    } finally {
      setSaving(false);
    }
  };

  // ── Render ───────────────────────────────────────────────────────

  if (loading) return <div className="p-6 text-gray-500">Loading commission settings…</div>;

  return (
    <div className="p-6 space-y-6">

      {/* ── Header ─────────────────────────────────────────────── */}
      <div>
        <h1 className="text-2xl font-bold">Commission Settings</h1>
        <p className="text-sm text-gray-500 mt-1">
          Configure how each staff member earns commission on payments.
          Changes apply to new payments only — historical commissions are not affected.
        </p>
      </div>

      {/* ── Strategy legend ─────────────────────────────────────── */}
      <div className="grid grid-cols-3 gap-4">
        <div className="bg-blue-50 border border-blue-100 rounded-lg p-4">
          <p className="text-sm font-semibold text-blue-700">Percentage</p>
          <p className="text-xs text-blue-600 mt-1">
            Staff earns a fixed % of every payment. Example: 40% of R280 = R112.
          </p>
        </div>
        <div className="bg-green-50 border border-green-100 rounded-lg p-4">
          <p className="text-sm font-semibold text-green-700">Fixed Amount</p>
          <p className="text-xs text-green-600 mt-1">
            Staff earns a flat amount per service regardless of price. Example: R80 per payment.
          </p>
        </div>
        <div className="bg-purple-50 border border-purple-100 rounded-lg p-4">
          <p className="text-sm font-semibold text-purple-700">Tiered</p>
          <p className="text-xs text-purple-600 mt-1">
            Percentage increases with monthly performance. Resets on the 1st of each month.
          </p>
        </div>
      </div>

      {/* ── Staff rules table ───────────────────────────────────── */}
      <div className="bg-white rounded shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-100 border-b text-left">
              <th className="py-3 px-4">Staff Member</th>
              <th className="py-3 px-4">Job Role</th>
              <th className="py-3 px-4">Strategy</th>
              <th className="py-3 px-4">Rate / Tiers</th>
              <th className="py-3 px-4">Action</th>
            </tr>
          </thead>
          <tbody>
            {staff.map(member => {
              const rule = getRuleForStaff(member.id);
              return (
                <tr key={member.id} className="border-b hover:bg-gray-50">
                  <td className="py-3 px-4 font-medium text-gray-800">
                    {member.fullName}
                  </td>
                  <td className="py-3 px-4 text-gray-500">{member.role}</td>
                  <td className="py-3 px-4">
                    <span className={`text-xs px-2 py-1 rounded-full font-medium ${
                      TYPE_COLORS[rule?.type ?? "None"]
                    }`}>
                      {rule?.type ?? "Not set"}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-gray-600 text-xs">
                    {ruleDescription(rule)}
                  </td>
                  <td className="py-3 px-4">
                    <button
                      onClick={() => openModal(member)}
                      className={`px-3 py-1 rounded text-sm ${
                        rule
                          ? "bg-blue-500 text-white hover:bg-blue-600"
                          : "bg-teal-500 text-white hover:bg-teal-600"
                      }`}
                    >
                      {rule ? "Edit Rule" : "Set Rule"}
                    </button>
                  </td>
                </tr>
              );
            })}
            {staff.length === 0 && (
              <tr>
                <td colSpan={5} className="py-8 text-center text-gray-400">
                  No active staff members found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* ── Modal ───────────────────────────────────────────────── */}
      {modalOpen && editingStaff && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-[500px] shadow-xl max-h-[90vh] overflow-y-auto">

            <h2 className="text-xl font-bold mb-1">Commission Rule</h2>
            <p className="text-sm text-gray-500 mb-5">
              {editingStaff.fullName} — {editingStaff.role}
            </p>

            {error && (
              <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded">
                {error}
              </div>
            )}

            {/* Strategy selector */}
            <div className="mb-5">
              <label className="block text-sm font-medium mb-2">Commission Strategy</label>
              <div className="grid grid-cols-3 gap-2">
                {(["Percentage", "Fixed", "Tiered"] as CommissionType[]).map(type => (
                  <button
                    key={type}
                    type="button"
                    onClick={() => setSelectedType(type)}
                    className={`py-2 rounded border text-sm font-medium transition-colors ${
                      selectedType === type
                        ? "bg-teal-600 text-white border-teal-600"
                        : "border-gray-300 hover:bg-gray-50"
                    }`}
                  >
                    {TYPE_LABELS[type]}
                  </button>
                ))}
              </div>
            </div>

            {/* Percentage input */}
            {selectedType === "Percentage" && (
              <div className="mb-5">
                <label className="block text-sm font-medium mb-1">
                  Commission Percentage
                </label>
                <div className="flex items-center gap-2">
                  <input
                    type="number"
                    min={1}
                    max={100}
                    step={0.5}
                    className="w-32 border p-2 rounded text-sm"
                    value={rateOrAmount}
                    onChange={e => setRateOrAmount(Number(e.target.value))}
                  />
                  <span className="text-gray-500 text-sm">%</span>
                </div>
                <p className="text-xs text-gray-400 mt-1">
                  Example: 40% of R280 payment = R112 commission
                </p>
              </div>
            )}

            {/* Fixed input */}
            {selectedType === "Fixed" && (
              <div className="mb-5">
                <label className="block text-sm font-medium mb-1">
                  Fixed Amount per Payment
                </label>
                <div className="flex items-center gap-2">
                  <span className="text-gray-500 text-sm">R</span>
                  <input
                    type="number"
                    min={1}
                    step={0.5}
                    className="w-32 border p-2 rounded text-sm"
                    value={rateOrAmount}
                    onChange={e => setRateOrAmount(Number(e.target.value))}
                  />
                </div>
                <p className="text-xs text-gray-400 mt-1">
                  Staff earns this amount for every payment recorded, regardless of size.
                </p>
              </div>
            )}

            {/* Tiered tiers */}
            {selectedType === "Tiered" && (
              <div className="mb-5">
                <div className="flex items-center justify-between mb-2">
                  <label className="text-sm font-medium">
                    Tiers (monthly completed services)
                  </label>
                  <button
                    type="button"
                    onClick={addTier}
                    className="text-xs text-teal-600 hover:text-teal-700 underline"
                  >
                    + Add tier
                  </button>
                </div>
                <div className="space-y-2">
                  {tiers.map((tier, index) => (
                    <div key={index} className="flex items-center gap-2 bg-gray-50 rounded p-2">
                      <div className="flex items-center gap-1">
                        <span className="text-xs text-gray-400">From</span>
                        <input
                          type="number"
                          min={0}
                          className="w-16 border p-1 rounded text-xs"
                          value={tier.minServices}
                          onChange={e => updateTier(index, "minServices", Number(e.target.value))}
                        />
                      </div>
                      <div className="flex items-center gap-1">
                        <span className="text-xs text-gray-400">to</span>
                        <input
                          type="number"
                          min={0}
                          className="w-16 border p-1 rounded text-xs"
                          value={tier.maxServices ?? ""}
                          placeholder="∞"
                          onChange={e => updateTier(index, "maxServices",
                            e.target.value === "" ? null : Number(e.target.value))}
                        />
                      </div>
                      <div className="flex items-center gap-1">
                        <span className="text-xs text-gray-400">→</span>
                        <input
                          type="number"
                          min={1}
                          max={100}
                          className="w-16 border p-1 rounded text-xs"
                          value={tier.percentage}
                          onChange={e => updateTier(index, "percentage", Number(e.target.value))}
                        />
                        <span className="text-xs text-gray-400">%</span>
                      </div>
                      {tiers.length > 1 && (
                        <button
                          type="button"
                          onClick={() => removeTier(index)}
                          className="text-red-400 hover:text-red-600 text-xs ml-1"
                        >
                          ✕
                        </button>
                      )}
                    </div>
                  ))}
                </div>
                <p className="text-xs text-gray-400 mt-2">
                  Leave the last tier's max blank (∞) for unlimited. Tiers reset on the 1st of each month.
                </p>
              </div>
            )}

            {/* Actions */}
            <div className="flex justify-end gap-2 pt-2 border-t">
              <button
                type="button"
                onClick={() => setModalOpen(false)}
                className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 text-sm"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleSave}
                disabled={saving}
                className="px-4 py-2 rounded bg-teal-600 text-white hover:bg-teal-700 text-sm disabled:opacity-50"
              >
                {saving ? "Saving…" : "Save Rule"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
