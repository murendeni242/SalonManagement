// UPDATED — uses isLoggedIn() and getUserRole() from updated authUtils.
// These now read from "salon_auth" key instead of "token".
import { Navigate } from "react-router-dom";
import { isLoggedIn, getUserRole } from "./authUtils";

interface Props {
  children: any;
  allowedRoles?: string[];
}

export default function RequireAuth({ children, allowedRoles }: Props) {
  if (!isLoggedIn()) {
    return <Navigate to="/login" replace />;
  }

  const role = getUserRole();

  if (allowedRoles && (!role || !allowedRoles.includes(role))) {
    return <Navigate to="/dashboard" replace />;
  }

  return children;
}
