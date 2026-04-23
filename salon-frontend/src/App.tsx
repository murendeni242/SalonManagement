import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import Login           from "./auth/Login";
import RequireAuth     from "./auth/RequireAuth";
import DashboardLayout from "./layout/DashboardLayout";

import Dashboard          from "./pages/Dashboard";
import Calendar           from "./pages/Calendar";
import Customers          from "./pages/Customers";
import Bookings           from "./pages/Bookings";
import Services           from "./pages/Services";
import Staff              from "./pages/Staff";
import Sales              from "./pages/Sales";
import Users              from "./pages/Users";
import ChangePassword     from "./pages/ChangePassword";
import CommissionSettings from "./pages/CommissionSettings";
import CommissionEarnings from "./pages/CommissionEarnings";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Navigate to="/login" replace />} />

        {/* Public */}
        <Route path="/login" element={<Login />} />

        {/* Change password — any logged-in user, NO sidebar */}
        <Route
          path="/change-password"
          element={
            <RequireAuth>
              <ChangePassword />
            </RequireAuth>
          }
        />

        {/* Dashboard — all roles */}
        <Route path="/dashboard" element={
          <RequireAuth>
            <DashboardLayout><Dashboard /></DashboardLayout>
          </RequireAuth>
        } />

        {/* Calendar — all roles */}
        <Route path="/calendar" element={
          <RequireAuth>
            <DashboardLayout><Calendar /></DashboardLayout>
          </RequireAuth>
        } />

        {/* Bookings — all roles */}
        <Route path="/bookings" element={
          <RequireAuth>
            <DashboardLayout><Bookings /></DashboardLayout>
          </RequireAuth>
        } />

        {/* Customers — Owner + Reception */}
        <Route path="/customers" element={
          <RequireAuth allowedRoles={["Owner", "Reception"]}>
            <DashboardLayout><Customers /></DashboardLayout>
          </RequireAuth>
        } />

        {/* Services — Owner + Reception */}
        <Route path="/services" element={
          <RequireAuth allowedRoles={["Owner", "Reception"]}>
            <DashboardLayout><Services /></DashboardLayout>
          </RequireAuth>
        } />

        {/* Staff — Owner + Reception */}
        <Route path="/staff" element={
          <RequireAuth allowedRoles={["Owner", "Reception"]}>
            <DashboardLayout><Staff /></DashboardLayout>
          </RequireAuth>
        } />

        {/* Sales — Owner only */}
        <Route path="/sales" element={
          <RequireAuth allowedRoles={["Owner"]}>
            <DashboardLayout><Sales /></DashboardLayout>
          </RequireAuth>
        } />

        {/* Users — Owner only */}
        <Route path="/users" element={
          <RequireAuth allowedRoles={["Owner"]}>
            <DashboardLayout><Users /></DashboardLayout>
          </RequireAuth>
        } />

        {/* Commission Settings — Owner only */}
        <Route path="/commissions/settings" element={
          <RequireAuth allowedRoles={["Owner"]}>
            <DashboardLayout><CommissionSettings /></DashboardLayout>
          </RequireAuth>
        } />

        {/* Commission Earnings — Owner only */}
        <Route path="/commissions/earnings" element={
          <RequireAuth allowedRoles={["Owner"]}>
            <DashboardLayout><CommissionEarnings /></DashboardLayout>
          </RequireAuth>
        } />

        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
