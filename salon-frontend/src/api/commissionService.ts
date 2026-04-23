import api from "./axios";

// ── Types ──────────────────────────────────────────────────────────

export interface CommissionTierDto {
  id:          number;
  minServices: number;
  maxServices: number | null;
  percentage:  number;
}

export interface CommissionRuleDto {
  id:           number;
  staffId:      number;
  staffName:    string;
  type:         string;   // "Percentage" | "Fixed" | "Tiered"
  rateOrAmount: number;
  tiers:        CommissionTierDto[];
}

export interface CommissionSummaryDto {
  staffId:      number;
  staffName:    string;
  totalEarned:  number;
  totalPending: number;
  totalPaid:    number;
  totalRecords: number;
}

export interface UpsertRulePayload {
  type:         number;   // 1=Percentage, 2=Fixed, 3=Tiered
  rateOrAmount: number;
  tiers:        { minServices: number; maxServices: number | null; percentage: number }[];
}

// ── API calls ─────────────────────────────────────────────────────

export const commissionService = {
  /** Returns all commission rules — one per staff member. */
  getRules: () =>
    api.get<CommissionRuleDto[]>("/commissions/rules"),

  /** Creates or updates the commission rule for a staff member. */
  upsertRule: (staffId: number, payload: UpsertRulePayload) =>
    api.put<CommissionRuleDto>(`/commissions/rules/${staffId}`, payload),

  /** Returns commission summary for one staff member in a date range. */
  getStaffSummary: (staffId: number, from: string, to: string) =>
    api.get<CommissionSummaryDto>(`/commissions/staff/${staffId}`, {
      params: { from, to },
    }),

  /** Marks all pending commissions as paid for a staff member. */
  markAllPaid: (staffId: number) =>
    api.post(`/commissions/pay/${staffId}`, { commissionIds: [] }),

  /** Marks specific commission IDs as paid. */
  markSelectedPaid: (staffId: number, ids: number[]) =>
    api.post(`/commissions/pay/${staffId}`, { commissionIds: ids }),
};
