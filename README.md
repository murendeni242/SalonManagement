# 💇‍♀️ Salon Management System

A full-stack salon management platform built with **ASP.NET Core 8 (Clean Architecture)** and **React 18 + TypeScript**.

The system enables salon owners to manage bookings, customers, staff, services, sales, and user accounts through secure role-based access control and real-time business analytics dashboards.

---

## 🚀 Tech Stack

### Backend
- ASP.NET Core 8
- Clean Architecture
- Entity Framework Core 8
- SQL Server
- JWT Bearer Authentication
- BCrypt Password Hashing

### Frontend
- React 18 + TypeScript
- Tailwind CSS
- Axios (JWT interceptor)
- Recharts (Analytics dashboards)
- React Router v6

---

## 🔐 Core Features

- Role-based access control (Owner, Reception, Staff)
- JWT authentication with claims-based authorization
- Secure temporary password generation (Owner-created accounts)
- Forced password change on first login
- Booking lifecycle management  
  `Pending → Confirmed → Completed → Cancelled`
- Sales tracking with refund & void functionality
- Revenue & performance analytics dashboard
- Audit logging for state changes
- Clean separation between **User (login account)** and **Staff (employee record)**

---

## 🏗 Architecture Overview

The backend follows **Clean Architecture** principles:

- **Domain** — Entities and business rules (zero external dependencies)
- **Application** — Use case handlers, DTOs, business logic
- **Infrastructure** — EF Core, repositories, security services
- **API** — Controllers, middleware, dependency injection

Business rules are enforced inside domain methods (e.g., `booking.Complete()`) to prevent invalid state transitions and keep logic centralized.

---

## 📊 Dashboard Analytics

The Owner dashboard includes:

- Revenue over time (line chart)
- Bookings by status (donut chart)
- Busiest days of the week (bar chart)
- Top 5 services by revenue

Charts dynamically update based on selected date range.

---

## ▶️ Running Locally

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- SQL Server (LocalDB supported)

---

### Backend Setup

```bash
# Apply migrations
dotnet ef database update --project Salon.Infrastructure --startup-project Salon.API

# Run API
dotnet run --project Salon.API
```

Swagger:
```
https://localhost:7001/swagger
```

---

### Frontend Setup

```bash
cd salon-frontend
npm install
npm run dev
```

Frontend runs at:
```
http://localhost:5173
```

---

## 🔐 Authentication Flow

1. User logs in via `/api/auth/login`
2. Backend verifies password using BCrypt
3. JWT token is generated with:
   - `sub` (UserId)
   - `email`
   - `role`
4. Frontend stores auth object in localStorage
5. Axios interceptor attaches `Authorization: Bearer <token>`
6. 401 responses auto-redirect to `/login`

---

## 🚀 Engineering Roadmap

### Advanced Booking Controls
- Double-booking prevention (interval overlap validation)
- Staff availability & working hours configuration
- Buffer time enforcement
- Waitlist system

### Financial & Commission Engine
- Configurable commission strategies (percentage, fixed, tiered)
- Proportional commission adjustments on refunds
- Staff commission reporting
- Daily financial reconciliation
- Exportable PDF/Excel reports

### Multi-Branch Support
- Branch-scoped data isolation
- Branch-level access control
- Cross-branch reporting

### Domain Events & Decoupling
- Domain events (e.g., `SaleRecorded`, `BookingCompleted`)
- Event-driven commission calculation
- Event-driven audit logging

### Performance & Scalability
- Query optimization & indexing
- Pagination for large datasets
- Caching for analytics endpoints
- Background processing for heavy reports

### Production Readiness
- Docker containerization
- CI/CD pipeline
- Cloud deployment (Azure / AWS)
- Centralized logging & monitoring

### Testing & Code Quality
- Unit testing (xUnit + Moq)
- Integration testing
- Edge-case validation tests
- Static analysis & linting

---

## 🚧 Project Status

This project is actively under development and evolving toward production readiness.

---

## 📄 License

This project is licensed under the MIT License.

---

## 👨‍💻 Author

**Murendeni Mulaudzi**  
Full-Stack .NET Developer  
Johannesburg, South Africa  

GitHub: https://github.com/murendeni242
