// Features:
// - Revenue summary cards (total, refunded, net, by payment method)
// - Sales table with date filter (from/to)
// - Record payment modal — links to a confirmed booking
// - Refund modal — partial or full refund
// - Void sale (Owner only)
// - Status badges: Paid | Refunded | Voided

import { useEffect, useState, useCallback } from "react";
import api from "../api/axios";

// ── Types ──────────────────────────────────────────────────────────

interface Sale {
  id: number;
  bookingId: number;
  amountPaid: number;
  paymentMethod: string;   // Cash | Card | EFT | Voucher
  status: string;          // Paid | Refunded | Voided
  paidAt: string;          // ISO datetime
  notes: string | null;
  originalSaleId: number | null;
}

interface SaleSummary {
  totalRevenue: number;
  totalRefunded: number;
  netRevenue: number;
  totalTransactions: number;
  revenueByMethod: Record<string, number>;
}

interface SalesPage {
  sales: Sale[];
  summary: SaleSummary;
}

interface Booking {
  id: number;
  customerId: number;
  staffId: number;
  serviceId: number;
  bookingDate: string;
  startTime: string;
  totalPrice: number;
  status: string;
}

interface Customer {
  id: number;
  fullName: string;
}

// ── Helpers ────────────────────────────────────────────────────────

function formatCurrency(amount: number): string {
  return `R ${amount.toFixed(2)}`;
}

function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString("en-ZA", {
    day: "numeric", month: "short", year: "numeric",
    hour: "2-digit", minute: "2-digit",
  });
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString("en-ZA", {
    day: "numeric", month: "short", year: "numeric",
  });
}

const STATUS_CLASS: Record<string, string> = {
  Paid:     "bg-green-100 text-green-700",
  Refunded: "bg-orange-100 text-orange-700",
  Voided:   "bg-gray-100 text-gray-500",
};

const METHOD_COLORS: Record<string, string> = {
  Cash:    "bg-emerald-500",
  Card:    "bg-blue-500",
  EFT:     "bg-purple-500",
  Voucher: "bg-amber-500",
};

const PAYMENT_METHODS = ["Cash", "Card", "EFT", "Voucher"];

// ── Main component ─────────────────────────────────────────────────

export default function Sales() {
  // Data
  const [sales,     setSales]     = useState<Sale[]>([]);
  const [summary,   setSummary]   = useState<SaleSummary | null>(null);
  const [bookings,  setBookings]  = useState<Booking[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading,   setLoading]   = useState(true);

  // Date filter — default to current month
  const now      = new Date();
  const firstDay = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-01`;
  const today    = now.toISOString().split("T")[0];

  const [fromDate, setFromDate] = useState(firstDay);
  const [toDate,   setToDate]   = useState(today);

  // Record payment modal
  const [payModalOpen,  setPayModalOpen]  = useState(false);
  const [payError,      setPayError]      = useState<string | null>(null);
  const [payForm, setPayForm] = useState({
    bookingId:     0,
    amountPaid:    0,
    paymentMethod: "Card",
    notes:         "",
  });

  // Refund modal
  const [refundModalOpen, setRefundModalOpen] = useState(false);
  const [refundSaleId,    setRefundSaleId]    = useState<number | null>(null);
  const [refundError,     setRefundError]     = useState<string | null>(null);
  const [refundForm, setRefundForm] = useState({
    refundAmount: 0,
    notes:        "",
  });

  // ── Data fetching ────────────────────────────────────────────────

  const fetchSales = useCallback(async () => {
    setLoading(true);
    try {
      const [resSales, resBookings, resCustomers] = await Promise.all([
        api.get<SalesPage>("/sales", {
          params: { from: fromDate, to: toDate },
        }),
        api.get<Booking[]>("/bookings"),
        api.get<Customer[]>("/customers"),
      ]);
      setSales(resSales.data.sales);
      setSummary(resSales.data.summary);
      setBookings(resBookings.data);
      setCustomers(resCustomers.data);
    } finally {
      setLoading(false);
    }
  }, [fromDate, toDate]);

  useEffect(() => { fetchSales(); }, [fetchSales]);

  // ── Lookup helpers ───────────────────────────────────────────────

  const bookingLabel = (id: number) => {
    const b = bookings.find(b => b.id === id);
    if (!b) return `Booking #${id}`;
    const c = customers.find(c => c.id === b.customerId);
    const name = c?.fullName ?? `Customer #${b.customerId}`;
    return `${name} — ${formatDate(b.bookingDate)} ${b.startTime.substring(0, 5)}`;
  };

  // Confirmed bookings that don't already have a Paid sale
  const paidBookingIds = new Set(
    sales.filter(s => s.status === "Paid").map(s => s.bookingId)
  );
  const unpaidBookings = bookings.filter(
    b => b.status === "Confirmed" && !paidBookingIds.has(b.id)
  );

  // ── Record payment ───────────────────────────────────────────────

  const openPayModal = () => {
    setPayError(null);
    setPayForm({ bookingId: 0, amountPaid: 0, paymentMethod: "Card", notes: "" });
    setPayModalOpen(true);
  };

  // Auto-fill amount when booking is selected
  const handleBookingSelect = (bookingId: number) => {
    const b = bookings.find(b => b.id === bookingId);
    setPayForm(f => ({
      ...f,
      bookingId,
      amountPaid: b?.totalPrice ?? 0,
    }));
  };

  const handleRecordPayment = async (e: React.FormEvent) => {
    e.preventDefault();
    setPayError(null);
    if (!payForm.bookingId) { setPayError("Please select a booking."); return; }
    if (payForm.amountPaid <= 0) { setPayError("Amount must be greater than zero."); return; }

    try {
      await api.post("/sales", {
        bookingId:     payForm.bookingId,
        amountPaid:    payForm.amountPaid,
        paymentMethod: payForm.paymentMethod,
        notes:         payForm.notes || undefined,
      });
      setPayModalOpen(false);
      fetchSales();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setPayError(e?.response?.data?.error ?? "Payment recording failed");
    }
  };

  // ── Refund ───────────────────────────────────────────────────────

  const openRefundModal = (sale: Sale) => {
    setRefundSaleId(sale.id);
    setRefundError(null);
    setRefundForm({ refundAmount: sale.amountPaid, notes: "" });
    setRefundModalOpen(true);
  };

  const handleRefund = async (e: React.FormEvent) => {
    e.preventDefault();
    setRefundError(null);
    if (!refundSaleId) return;

    try {
      await api.post(`/sales/${refundSaleId}/refund`, {
        refundAmount: refundForm.refundAmount,
        notes:        refundForm.notes || undefined,
      });
      setRefundModalOpen(false);
      fetchSales();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      setRefundError(e?.response?.data?.error ?? "Refund failed");
    }
  };

  // ── Void ─────────────────────────────────────────────────────────

  const handleVoid = async (sale: Sale) => {
    const reason = prompt("Reason for voiding this sale:");
    if (reason === null) return; // cancelled
    try {
      await api.post(`/sales/${sale.id}/void`, { reason: reason || "No reason given" });
      fetchSales();
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string } } };
      alert(e?.response?.data?.error ?? "Void failed");
    }
  };

  // ── Render ───────────────────────────────────────────────────────

  if (loading) return <div className="p-6 text-gray-500">Loading sales…</div>;

  return (
    <div className="p-6 space-y-6">

      {/* ── Header ─────────────────────────────────────────────── */}
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Sales & Revenue</h1>
        <button
          onClick={openPayModal}
          className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 text-sm"
        >
          Record Payment
        </button>
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

      {/* ── Summary cards ──────────────────────────────────────── */}
      {summary && (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <SummaryCard
            label="Total Revenue"
            value={formatCurrency(summary.totalRevenue)}
            color="bg-green-600"
          />
          <SummaryCard
            label="Net Revenue"
            value={formatCurrency(summary.netRevenue)}
            color="bg-blue-600"
          />
          <SummaryCard
            label="Refunded"
            value={formatCurrency(summary.totalRefunded)}
            color="bg-orange-500"
          />
          <SummaryCard
            label="Transactions"
            value={summary.totalTransactions}
            color="bg-purple-600"
          />
        </div>
      )}

      {/* ── Revenue by payment method ───────────────────────────── */}
      {summary && Object.keys(summary.revenueByMethod).length > 0 && (
        <div className="bg-white rounded shadow p-4">
          <h2 className="text-sm font-semibold text-gray-500 mb-3 uppercase tracking-wide">
            Revenue by Payment Method
          </h2>
          <div className="flex gap-4 flex-wrap">
            {Object.entries(summary.revenueByMethod).map(([method, amount]) => (
              amount > 0 ? (
                <div key={method} className="flex items-center gap-2">
                  <div className={`w-3 h-3 rounded-full ${METHOD_COLORS[method] ?? "bg-gray-400"}`} />
                  <span className="text-sm font-medium">{method}</span>
                  <span className="text-sm text-gray-600">{formatCurrency(amount)}</span>
                </div>
              ) : null
            ))}
          </div>
        </div>
      )}

      {/* ── Sales table ────────────────────────────────────────── */}
      <div className="bg-white rounded shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-gray-100 border-b text-left">
              <th className="py-3 px-4">Date & Time</th>
              <th className="py-3 px-4">Booking</th>
              <th className="py-3 px-4">Amount</th>
              <th className="py-3 px-4">Method</th>
              <th className="py-3 px-4">Status</th>
              <th className="py-3 px-4">Notes</th>
              <th className="py-3 px-4">Actions</th>
            </tr>
          </thead>
          <tbody>
            {sales.map(sale => (
              <tr
                key={sale.id}
                className={`border-b hover:bg-gray-50 ${
                  sale.originalSaleId ? "opacity-70" : ""
                }`}
              >
                <td className="py-3 px-4 text-gray-600">
                  {formatDateTime(sale.paidAt)}
                </td>
                <td className="py-3 px-4">
                  {sale.originalSaleId ? (
                    <span className="text-orange-600 text-xs font-medium">
                      ↩ Refund of Sale #{sale.originalSaleId}
                    </span>
                  ) : (
                    <span className="text-gray-700">
                      {bookingLabel(sale.bookingId)}
                    </span>
                  )}
                </td>
                <td className={`py-3 px-4 font-semibold ${
                  sale.amountPaid < 0 ? "text-red-600" : "text-gray-800"
                }`}>
                  {formatCurrency(sale.amountPaid)}
                </td>
                <td className="py-3 px-4">
                  <div className="flex items-center gap-1.5">
                    <div className={`w-2 h-2 rounded-full ${METHOD_COLORS[sale.paymentMethod] ?? "bg-gray-400"}`} />
                    {sale.paymentMethod}
                  </div>
                </td>
                <td className="py-3 px-4">
                  <span className={`text-xs px-2 py-1 rounded-full font-medium ${
                    STATUS_CLASS[sale.status] ?? "bg-gray-100 text-gray-500"
                  }`}>
                    {sale.status}
                  </span>
                </td>
                <td className="py-3 px-4 text-gray-500 text-xs max-w-[160px] truncate">
                  {sale.notes ?? "—"}
                </td>
                <td className="py-3 px-4">
                  <div className="flex gap-1">
                    {sale.status === "Paid" && !sale.originalSaleId && (
                      <>
                        <button
                          onClick={() => openRefundModal(sale)}
                          className="bg-orange-500 text-white px-2 py-1 rounded text-xs hover:bg-orange-600"
                        >
                          Refund
                        </button>
                        <button
                          onClick={() => handleVoid(sale)}
                          className="bg-gray-400 text-white px-2 py-1 rounded text-xs hover:bg-gray-500"
                        >
                          Void
                        </button>
                      </>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {sales.length === 0 && (
              <tr>
                <td colSpan={7} className="py-8 text-center text-gray-400">
                  No sales in this period. Change the date range or record a payment.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* ── Record Payment Modal ────────────────────────────────── */}
      {payModalOpen && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-[460px] shadow-xl">
            <h2 className="text-xl font-bold mb-4">Record Payment</h2>

            {payError && (
              <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded">
                {payError}
              </div>
            )}

            {unpaidBookings.length === 0 ? (
              <div className="text-center py-6 text-gray-400">
                <p className="text-sm">No confirmed bookings waiting for payment.</p>
                <p className="text-xs mt-1">
                  Confirm a booking on the Bookings page first.
                </p>
                <button
                  onClick={() => setPayModalOpen(false)}
                  className="mt-4 px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 text-sm"
                >
                  Close
                </button>
              </div>
            ) : (
              <form onSubmit={handleRecordPayment} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-1">
                    Booking *
                  </label>
                  <select
                    className="w-full border p-2 rounded text-sm"
                    value={payForm.bookingId || ""}
                    onChange={e => handleBookingSelect(Number(e.target.value))}
                    required
                  >
                    <option value="">Select confirmed booking…</option>
                    {unpaidBookings.map(b => (
                      <option key={b.id} value={b.id}>
                        {bookingLabel(b.id)} — {formatCurrency(b.totalPrice)}
                      </option>
                    ))}
                  </select>
                  <p className="text-xs text-gray-400 mt-1">
                    Only Confirmed bookings without a payment are shown.
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">
                    Amount Paid (R) *
                  </label>
                  <input
                    type="number"
                    min={0.01}
                    step={0.01}
                    className="w-full border p-2 rounded text-sm"
                    value={payForm.amountPaid || ""}
                    onChange={e => setPayForm({ ...payForm, amountPaid: Number(e.target.value) })}
                    required
                  />
                  <p className="text-xs text-gray-400 mt-1">
                    Auto-filled from booking price — adjust if different.
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">
                    Payment Method *
                  </label>
                  <div className="grid grid-cols-4 gap-2">
                    {PAYMENT_METHODS.map(method => (
                      <button
                        key={method}
                        type="button"
                        onClick={() => setPayForm({ ...payForm, paymentMethod: method })}
                        className={`py-2 rounded border text-sm font-medium transition-colors ${
                          payForm.paymentMethod === method
                            ? "bg-black text-white border-black"
                            : "border-gray-300 hover:bg-gray-50"
                        }`}
                      >
                        {method}
                      </button>
                    ))}
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium mb-1">
                    Notes <span className="font-normal text-gray-400">(optional)</span>
                  </label>
                  <input
                    className="w-full border p-2 rounded text-sm"
                    value={payForm.notes}
                    placeholder="e.g. Full payment received"
                    onChange={e => setPayForm({ ...payForm, notes: e.target.value })}
                  />
                </div>

                <div className="flex justify-end gap-2 pt-2">
                  <button
                    type="button"
                    onClick={() => setPayModalOpen(false)}
                    className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 text-sm"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 rounded bg-green-600 text-white hover:bg-green-700 text-sm"
                  >
                    Record Payment
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      )}

      {/* ── Refund Modal ────────────────────────────────────────── */}
      {refundModalOpen && refundSaleId && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-[400px] shadow-xl">
            <h2 className="text-xl font-bold mb-1">Issue Refund</h2>
            <p className="text-sm text-gray-500 mb-4">
              Sale #{refundSaleId} — {bookingLabel(
                sales.find(s => s.id === refundSaleId)?.bookingId ?? 0
              )}
            </p>

            {refundError && (
              <div className="mb-4 bg-red-50 border border-red-200 text-red-700 text-sm px-3 py-2 rounded">
                {refundError}
              </div>
            )}

            <form onSubmit={handleRefund} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">
                  Refund Amount (R) *
                </label>
                <input
                  type="number"
                  min={0.01}
                  step={0.01}
                  className="w-full border p-2 rounded text-sm"
                  value={refundForm.refundAmount || ""}
                  onChange={e => setRefundForm({ ...refundForm, refundAmount: Number(e.target.value) })}
                  required
                />
                <p className="text-xs text-gray-400 mt-1">
                  Pre-filled with the full sale amount. Reduce for partial refund.
                </p>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">
                  Reason <span className="font-normal text-gray-400">(optional)</span>
                </label>
                <input
                  className="w-full border p-2 rounded text-sm"
                  value={refundForm.notes}
                  placeholder="e.g. Customer unhappy with result"
                  onChange={e => setRefundForm({ ...refundForm, notes: e.target.value })}
                />
              </div>

              <div className="flex justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setRefundModalOpen(false)}
                  className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 text-sm"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 rounded bg-orange-500 text-white hover:bg-orange-600 text-sm"
                >
                  Issue Refund
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

// ── Summary card ──────────────────────────────────────────────────

function SummaryCard({
  label, value, color,
}: {
  label: string;
  value: string | number;
  color: string;
}) {
  return (
    <div className={`${color} text-white p-5 rounded-lg shadow`}>
      <p className="text-xs font-medium opacity-75 uppercase tracking-wide">{label}</p>
      <p className="text-2xl font-bold mt-1">{value}</p>
    </div>
  );
}
