using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using Salon.Infrastructure.Persistence;

namespace Salon.Infrastructure.Persistence;

/// <summary>
/// Seeds the entire database with salon data.
/// </summary>
public static class SalonSeeder
{
    private const int SeedUserId = 0;

    public static async Task SeedAsync(SalonDbContext context)
    {
        await SeedUsersAsync(context);
        await SeedStaffAsync(context);
        await SeedServicesAsync(context);
        await SeedCustomersAsync(context);
        await SeedStaffSchedulesAsync(context);
        await SeedCommissionRulesAsync(context);
        await SeedBookingsAsync(context);
        await SeedSalesAsync(context);
        await SeedCommissionsAsync(context);
        await SeedAuditLogsAsync(context);
    }

    // ── 1. USERS ─────────────────────────────────────────────────────

    private static async Task SeedUsersAsync(SalonDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var hash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

        context.Users.AddRange(
            new User("admin@salon.co.za", hash, "Owner", false),
            new User("reception@salon.co.za", hash, "Reception", false),
            new User("thandi@salon.co.za", hash, "Staff", false),
            new User("sipho@salon.co.za", hash, "Staff", false),
            new User("lerato@salon.co.za", hash, "Staff", false),
            new User("nomsa@salon.co.za", hash, "Staff", false),
            new User("bongani@salon.co.za", hash, "Staff", false)
        );

        await context.SaveChangesAsync();
        Console.WriteLine("Seeded 7 users  (password: Admin@123)");
    }

    // ── 2. STAFF ──────────────────────────────────────────────────────

    private static async Task SeedStaffAsync(SalonDbContext context)
    {
        if (await context.Staff.AnyAsync()) return;

        context.Staff.AddRange(
            new Staff("Thandi", "Dlamini", "0731234567", "Stylist", "thandi@salon.co.za"),
            new Staff("Sipho", "Ndlovu", "0712345678", "Colourist", "sipho@salon.co.za"),
            new Staff("Lerato", "Khumalo", "0821234505", "Therapist", "lerato@salon.co.za"),
            new Staff("Nomsa", "Mokoena", "0821234501", "Stylist", "nomsa@salon.co.za"),
            new Staff("Bongani", "Sithole", "0821234502", "Manager", "bongani@salon.co.za"),
            new Staff("Zanele", "Nkosi", "0821234503", "Colourist", null),
            new Staff("Kagiso", "Motsepe", "0821234504", "Therapist", null)
        );

        await context.SaveChangesAsync();
        Console.WriteLine("Seeded 7 staff members");
    }

    // ── 3. SERVICES ───────────────────────────────────────────────────

    private static async Task SeedServicesAsync(SalonDbContext context)
    {
        if (await context.Services.AnyAsync()) return;

        context.Services.AddRange(
            new Service("Wash & Blow Dry", 45, 280m, "Shampoo, conditioning and blow dry."),
            new Service("Full Haircut", 60, 350m, "Full cut and style for all hair types."),
            new Service("Full Colour Treatment", 120, 850m, "Root to tip colour with toner."),
            new Service("Root Touch-Up", 75, 450m, "Colour retouch at the roots only."),
            new Service("Relaxer Treatment", 90, 550m, "Professional relaxer with neutraliser."),
            new Service("Deep Conditioning", 45, 220m, "Protein deep conditioning treatment."),
            new Service("Braiding — Full Head", 180, 700m, "Box braids, cornrows or Senegalese twist."),
            new Service("Weave Installation", 150, 950m, "Sew-in weave with closure."),
            new Service("Keratin Treatment", 120, 1100m, "Smoothing keratin. Lasts 3 months."),
            new Service("Scalp Treatment", 30, 180m, "Targeted scalp massage and treatment."),
            new Service("Eyebrow Threading", 20, 80m, "Precise eyebrow shaping via threading."),
            new Service("Manicure", 45, 150m, "Classic manicure with nail polish."),
            new Service("Pedicure", 60, 200m, "Classic pedicure with soak and nail polish."),
            new Service("Gel Nails", 75, 320m, "Gel overlay on natural nails."),
            new Service("Facial — Classic", 60, 450m, "Deep cleanse, exfoliation and mask.")
        );

        await context.SaveChangesAsync();
        Console.WriteLine("Seeded 15 services");
    }

    // ── 4. CUSTOMERS ──────────────────────────────────────────────────

    private static async Task SeedCustomersAsync(SalonDbContext context)
    {
        if (await context.Customers.AnyAsync()) return;

        var list = new List<Customer>
        {
            new Customer("Lindiwe",    "Khumalo",   "0821234505", "lindiwe.khumalo@gmail.com",   new DateTime(1990,  3, 15)),
            new Customer("Priya",      "Naidoo",    "0821234506", "priya.naidoo@gmail.com",      new DateTime(1988,  7, 22)),
            new Customer("Fatima",     "Mohamed",   "0821234507", "fatima.mohamed@gmail.com",    new DateTime(1995, 11,  8)),
            new Customer("Busisiwe",   "Zulu",      "0821234508", "busisiwe.zulu@gmail.com",     new DateTime(1992,  5, 30)),
            new Customer("Nonhlanhla", "Mthembu",   "0821234509", "nonhlanhla@gmail.com",        new DateTime(1985,  1, 14)),
            new Customer("Ayasha",     "Patel",     "0821234510", "ayasha.patel@gmail.com",      new DateTime(1997,  9,  3)),
            new Customer("Zanele",     "Moyo",      "0821234511", "zanele.moyo@gmail.com",       new DateTime(1993,  4, 18)),
            new Customer("Precious",   "Ndlovu",    "0821234512", "precious.ndlovu@gmail.com",   new DateTime(1991, 12, 25)),
            new Customer("Thandeka",   "Cele",      "0821234513", "thandeka.cele@gmail.com",     new DateTime(1989,  6,  7)),
            new Customer("Nomvula",    "Shabalala", "0821234514", "nomvula.shabalala@gmail.com", new DateTime(1994,  2, 19)),
            new Customer("Kefilwe",    "Motsepe",   "0821234515", "kefilwe.motsepe@gmail.com",   new DateTime(1996,  8, 11)),
            new Customer("Siphokazi",  "Ntuli",     "0821234516", "siphokazi.ntuli@gmail.com",   new DateTime(1987, 10,  5)),
            new Customer("Rethabile",  "Molefe",    "0821234517", "rethabile.molefe@gmail.com",  new DateTime(1999,  3, 28)),
            new Customer("Nokwanda",   "Mthethwa",  "0821234518", "nokwanda@gmail.com",          new DateTime(1990,  7, 16)),
            new Customer("Dineo",      "Ramaphosa", "0821234519", "dineo.r@gmail.com",           new DateTime(1993, 11,  2)),
            new Customer("Palesa",     "Lekgethoa", "0821234520", "palesa.l@gmail.com",          new DateTime(1986,  4,  9)),
            new Customer("Zinhle",     "Khumalo",   "0821234521", "zinhle.k@gmail.com",          new DateTime(1998,  1, 22)),
            new Customer("Mmabatho",   "Seabi",     "0821234522", "mmabatho@gmail.com",          new DateTime(1992,  6, 14)),
            new Customer("Puleng",     "Mahlaba",   "0821234523", "puleng.m@gmail.com",          new DateTime(1995,  9, 30)),
            new Customer("Boitumelo",  "Dlamini",   "0821234524", "boitumelo@gmail.com",         new DateTime(1991, 12,  7))
        };

        list[0].UpdateNotes("Allergic to PPD. Use ammonia-free colour only.");
        list[2].UpdateNotes("Sensitive scalp. Avoid strong relaxers.");
        list[4].UpdateNotes("Colour formula: L'Oreal 6.35 + 20vol.");
        list[7].UpdateNotes("Has alopecia patches — handle scalp gently. No heat styling.");
        list[11].UpdateNotes("Regular client since 2019. Prefers Saturday mornings.");

        context.Customers.AddRange(list);
        await context.SaveChangesAsync();
        Console.WriteLine("Seeded 20 customers");
    }

    // ── 5. STAFF SCHEDULES ────────────────────────────────────────────

    private static async Task SeedStaffSchedulesAsync(SalonDbContext context)
    {
        if (await context.StaffSchedules.AnyAsync()) return;

        var ids = await context.Staff.OrderBy(s => s.Id).Select(s => s.Id).ToListAsync();

        var s9 = new TimeSpan(9, 0, 0);
        var s10 = new TimeSpan(10, 0, 0);
        var e17 = new TimeSpan(17, 0, 0);
        var e18 = new TimeSpan(18, 0, 0);
        var e19 = new TimeSpan(19, 0, 0);

        var schedules = new List<StaffSchedule>();

        void AddDays(int staffId, DayOfWeek[] days, TimeSpan start, TimeSpan end)
        {
            foreach (var d in days)
                schedules.Add(new StaffSchedule(staffId, d, start, end));
        }

        AddDays(ids[0], new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday }, s9, e18);
        AddDays(ids[1], new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }, s9, e17);
        AddDays(ids[2], new[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday }, s10, e18);
        AddDays(ids[3], new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Saturday }, s9, e17);
        AddDays(ids[4], new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }, s9, e19);
        AddDays(ids[5], new[] { DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday }, s10, e18);
        AddDays(ids[6], new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday }, s9, e17);

        context.StaffSchedules.AddRange(schedules);
        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {schedules.Count} staff schedule rows");
    }

    // ── 6. COMMISSION RULES ───────────────────────────────────────────

    private static async Task SeedCommissionRulesAsync(SalonDbContext context)
    {
        if (await context.CommissionRules.AnyAsync()) return;

        var ids = await context.Staff.OrderBy(s => s.Id).Select(s => s.Id).ToListAsync();

        context.CommissionRules.AddRange(
            new CommissionRule(ids[0], CommissionType.Percentage, 40m),
            new CommissionRule(ids[1], CommissionType.Percentage, 35m),
            new CommissionRule(ids[2], CommissionType.Fixed, 80m),
            new CommissionRule(ids[4], CommissionType.Percentage, 25m),
            new CommissionRule(ids[5], CommissionType.Percentage, 38m),
            new CommissionRule(ids[6], CommissionType.Fixed, 60m)
        );

        var nomsaRule = new CommissionRule(ids[3]);
        context.CommissionRules.Add(nomsaRule);
        await context.SaveChangesAsync();

        var nomsaId = await context.CommissionRules
            .Where(r => r.StaffId == ids[3])
            .Select(r => r.Id)
            .FirstAsync();

        context.CommissionTiers.AddRange(
            new CommissionTier(nomsaId, 0, 10, 30m),
            new CommissionTier(nomsaId, 11, 30, 40m),
            new CommissionTier(nomsaId, 31, null, 50m)
        );

        await context.SaveChangesAsync();
        Console.WriteLine("Seeded 7 commission rules (Percentage / Fixed / Tiered)");
    }

    // ── 7. BOOKINGS ───────────────────────────────────────────────────

    private static async Task SeedBookingsAsync(SalonDbContext context)
    {
        if (await context.Bookings.IgnoreQueryFilters().AnyAsync()) return;

        var sIds = await context.Staff.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
        var svIds = await context.Services.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
        var cIds = await context.Customers.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();

        // (customerId, staffId, serviceId, date, start, end, price, status)
        // Status: 1=Pending, 2=Confirmed, 3=Completed, 4=Cancelled
        var rows = new (int c, int st, int sv, string date, string s, string e, decimal p, int status)[]
        {
            (cIds[0],  sIds[0], svIds[1],  "2026-01-06", "09:00", "10:00",  350m, 3),
            (cIds[1],  sIds[1], svIds[4],  "2026-01-06", "10:00", "11:30",  550m, 3),
            (cIds[2],  sIds[0], svIds[2],  "2026-01-07", "09:00", "11:00",  850m, 3),
            (cIds[3],  sIds[2], svIds[3],  "2026-01-08", "11:00", "12:15",  450m, 3),
            (cIds[4],  sIds[3], svIds[6],  "2026-01-09", "14:00", "17:00",  700m, 3),
            (cIds[5],  sIds[0], svIds[0],  "2026-01-10", "09:00", "09:45",  280m, 3),
            (cIds[6],  sIds[1], svIds[7],  "2026-01-12", "10:00", "12:30",  950m, 3),
            (cIds[7],  sIds[4], svIds[8],  "2026-01-13", "09:00", "11:00", 1100m, 3),
            (cIds[8],  sIds[2], svIds[9],  "2026-01-14", "11:00", "11:30",  180m, 3),
            (cIds[9],  sIds[3], svIds[5],  "2026-01-15", "14:00", "14:45",  220m, 3),

            (cIds[10], sIds[0], svIds[1],  "2026-02-02", "09:00", "10:00",  350m, 3),
            (cIds[11], sIds[1], svIds[4],  "2026-02-03", "10:00", "11:30",  550m, 3),
            (cIds[12], sIds[0], svIds[2],  "2026-02-04", "09:00", "11:00",  850m, 3),
            (cIds[13], sIds[2], svIds[3],  "2026-02-05", "11:00", "12:15",  450m, 3),
            (cIds[14], sIds[3], svIds[6],  "2026-02-06", "14:00", "17:00",  700m, 3),
            (cIds[15], sIds[4], svIds[0],  "2026-02-09", "09:00", "09:45",  280m, 4), // Cancelled
            (cIds[16], sIds[0], svIds[7],  "2026-02-10", "10:00", "12:30",  950m, 3),
            (cIds[17], sIds[1], svIds[8],  "2026-02-11", "09:00", "11:00", 1100m, 3),
            (cIds[18], sIds[2], svIds[9],  "2026-02-12", "11:00", "11:30",  180m, 3),
            (cIds[19], sIds[3], svIds[5],  "2026-02-13", "14:00", "14:45",  220m, 3),

            (cIds[0],  sIds[0], svIds[14], "2026-03-03", "09:00", "10:00",  450m, 3),
            (cIds[1],  sIds[1], svIds[1],  "2026-03-04", "10:00", "11:00",  350m, 3),
            (cIds[2],  sIds[2], svIds[2],  "2026-03-05", "11:00", "13:00",  850m, 3),
            (cIds[3],  sIds[3], svIds[3],  "2026-03-06", "14:00", "15:15",  450m, 3),
            (cIds[4],  sIds[4], svIds[6],  "2026-03-10", "09:00", "12:00",  700m, 3),
            (cIds[5],  sIds[0], svIds[0],  "2026-03-11", "10:00", "10:45",  280m, 3),
            (cIds[6],  sIds[1], svIds[7],  "2026-03-12", "11:00", "13:30",  950m, 3),
            (cIds[7],  sIds[2], svIds[8],  "2026-03-13", "14:00", "16:00", 1100m, 3),
            (cIds[8],  sIds[3], svIds[10], "2026-03-17", "09:00", "09:20",   80m, 3),
            (cIds[9],  sIds[4], svIds[13], "2026-03-18", "10:00", "11:15",  320m, 3),

            (cIds[10], sIds[0], svIds[14], "2026-04-01", "09:00", "10:00",  450m, 3),
            (cIds[11], sIds[1], svIds[0],  "2026-04-02", "10:00", "10:45",  280m, 3),
            (cIds[12], sIds[2], svIds[1],  "2026-04-03", "11:00", "12:00",  350m, 3),
            (cIds[13], sIds[3], svIds[2],  "2026-04-04", "14:00", "16:00",  850m, 4), // Cancelled
            (cIds[14], sIds[4], svIds[3],  "2026-04-07", "09:00", "10:15",  450m, 3),
            (cIds[15], sIds[0], svIds[4],  "2026-04-08", "10:00", "11:30",  550m, 3),
            (cIds[16], sIds[1], svIds[5],  "2026-04-09", "11:00", "11:45",  220m, 3),

            (cIds[17], sIds[2], svIds[6],  "2026-05-05", "14:00", "17:00",  700m, 1),
            (cIds[18], sIds[3], svIds[7],  "2026-05-06", "09:00", "11:30",  950m, 2),
            (cIds[19], sIds[4], svIds[8],  "2026-05-07", "10:00", "12:00", 1100m, 2),
        };

        foreach (var r in rows)
        {
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Bookings
                    (CustomerId, StaffId, ServiceId, BookingDate,
                     StartTime,  EndTime, TotalPrice, Status,
                     Notes, IsDeleted, DeletedAt,
                     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
                VALUES
                    ({0},{1},{2},{3},
                     {4},{5},{6},{7},
                     NULL, 0, NULL,
                     GETUTCDATE(),{8},GETUTCDATE(),{8})",
                r.c, r.st, r.sv,
                DateTime.Parse(r.date),
                TimeSpan.Parse(r.s),
                TimeSpan.Parse(r.e),
                r.p, r.status,
                SeedUserId);
        }

        Console.WriteLine($"Seeded {rows.Length} bookings");
    }

    // ── 8. SALES ──────────────────────────────────────────────────────

    private static async Task SeedSalesAsync(SalonDbContext context)
    {
        if (await context.Sales.AnyAsync()) return;

        // Use raw SQL to get completed booking IDs — avoids EF enum mapping issues
        var completedBookings = await context.Database
            .SqlQueryRaw<BookingRow>(
                "SELECT Id, StaffId, TotalPrice FROM Bookings WHERE Status = 3 AND IsDeleted = 0")
            .ToListAsync();

        if (completedBookings.Count == 0)
        {
            Console.WriteLine("No completed bookings found — sales not seeded. Check bookings table.");
            return;
        }

        var methods = new[] { "Cash", "Card", "EFT", "Card", "Cash", "Card", "EFT", "Card" };
        int count = 0;

        foreach (var (booking, i) in completedBookings.Select((b, i) => (b, i)))
        {
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Sales
                    (BookingId, AmountPaid, PaymentMethod, Status,
                     PaidAt, ProcessedByStaffId, Notes, OriginalSaleId,
                     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
                VALUES
                    ({0},{1},{2},1,
                     GETUTCDATE(),{3},NULL,NULL,
                     GETUTCDATE(),{4},GETUTCDATE(),{4})",
                booking.Id,
                booking.TotalPrice,
                methods[i % methods.Length],
                booking.StaffId,
                SeedUserId);

            count++;
        }

        // Add one partial refund on the 3rd sale
        var thirdSale = await context.Database
            .SqlQueryRaw<SaleRow>(
                "SELECT Id, BookingId, AmountPaid, PaymentMethod FROM Sales ORDER BY Id OFFSET 2 ROWS FETCH NEXT 1 ROWS ONLY")
            .FirstOrDefaultAsync();


        if (thirdSale != null)
        {
            // Mark original as Refunded (Status=2)
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE Sales SET Status = 2, UpdatedAt = GETUTCDATE(), UpdatedBy = {0} WHERE Id = {1}",
                SeedUserId, thirdSale.Id);

            // Insert refund record — negative amount, Status=2
            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Sales
                    (BookingId, AmountPaid, PaymentMethod, Status,
                     PaidAt, ProcessedByStaffId, Notes, OriginalSaleId,
                     CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
                VALUES
                    ({0},{1},{2},2,
                     GETUTCDATE(),NULL,
                     'Client unhappy with colour result — partial refund agreed',
                     {3},
                     GETUTCDATE(),{4},GETUTCDATE(),{4})",
                thirdSale.BookingId,
                -(thirdSale.AmountPaid / 2m),
                thirdSale.PaymentMethod,
                thirdSale.Id,
                SeedUserId);

            count++;
        }

        Console.WriteLine($"Seeded {count} sales records (including 1 partial refund)");
    }

    // ── 9. COMMISSIONS ────────────────────────────────────────────────

    private static async Task SeedCommissionsAsync(SalonDbContext context)
    {
        if (await context.Commissions.AnyAsync()) return;

        // Get paid sales with booking staff info via raw SQL
        var paidSales = await context.Database
            .SqlQueryRaw<SaleWithStaff>(@"
                SELECT s.Id, s.BookingId, s.AmountPaid, b.StaffId
                FROM Sales s
                INNER JOIN Bookings b ON b.Id = s.BookingId
                WHERE s.AmountPaid > 0 AND s.Status = 1")
            .ToListAsync();

        var rules = await context.CommissionRules
            .Include(r => r.Tiers)
            .ToListAsync();

        int count = 0;

        foreach (var (sale, i) in paidSales.Select((s, i) => (s, i)))
        {
            var rule = rules.FirstOrDefault(r => r.StaffId == sale.StaffId);
            if (rule == null) continue;

            var amount = rule.Type switch
            {
                CommissionType.Percentage => Math.Round(sale.AmountPaid * rule.RateOrAmount / 100, 2),
                CommissionType.Fixed => Math.Min(rule.RateOrAmount, sale.AmountPaid),
                CommissionType.Tiered => CalculateTiered(sale.AmountPaid, rule, 5),
                _ => 0m
            };

            // Every 3rd commission is Paid — rest are Pending
            int status = (i % 3 == 0) ? 2 : 1;
            var paidAt = status == 2 ? (object)DateTime.UtcNow : null;
            var paidBy = status == 2 ? (object)"admin@salon.co.za" : null;

            await context.Database.ExecuteSqlRawAsync(@"
                INSERT INTO Commissions
                    (SaleId, StaffId, GrossAmount, Amount,
                     RateApplied, Type, Status,
                     PaidAt, PaidBy, CreatedAt)
                VALUES
                    ({0},{1},{2},{3},
                     {4},{5},{6},
                     {7},{8},GETUTCDATE())",
                sale.Id,
                sale.StaffId,
                amount,
                amount,
                rule.RateOrAmount,
                (int)rule.Type,
                status,
                paidAt,
                paidBy);

            count++;
        }

        Console.WriteLine($"Seeded {count} commission records");
    }

    private static decimal CalculateTiered(decimal amount, CommissionRule rule, int completedThisMonth)
    {
        if (!rule.Tiers.Any()) return 0m;
        var tier = rule.Tiers
            .OrderByDescending(t => t.MinServices)
            .FirstOrDefault(t =>
                completedThisMonth >= t.MinServices &&
                (t.MaxServices == null || completedThisMonth <= t.MaxServices));
        return tier == null ? 0m : Math.Round(amount * tier.Percentage / 100, 2);
    }

    // ── 10. AUDIT LOGS ────────────────────────────────────────────────

    private static async Task SeedAuditLogsAsync(SalonDbContext context)
    {
        if (await context.AuditLogs.AnyAsync()) return;

        // Get completed booking IDs via raw SQL
        var bookingIds = await context.Database
            .SqlQueryRaw<int>("SELECT TOP 15 Id FROM Bookings WHERE Status = 3 AND IsDeleted = 0 ORDER BY Id")
            .ToListAsync();

        var logs = new List<AuditLog>();

        foreach (var id in bookingIds)
        {
            logs.Add(new AuditLog("Booking", id, "Created",
                $"Booking #{id} created.", "admin@salon.co.za"));
            logs.Add(new AuditLog("Booking", id, "Completed",
                $"Booking #{id} marked as completed.", "admin@salon.co.za"));
        }

        context.AuditLogs.AddRange(logs);
        await context.SaveChangesAsync();
        Console.WriteLine($"Seeded {logs.Count} audit log entries");
    }

    // ── Projection types for SqlQueryRaw ──────────────────────────────

    private class BookingRow
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public decimal TotalPrice { get; set; }
    }

    private class SaleRow
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = default!;
    }

    private class SaleWithStaff
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal AmountPaid { get; set; }
        public int StaffId { get; set; }
    }
}
