using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salon.Application.DTOs.Analytics
{
    public class SummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int NewCustomers { get; set; }
        public decimal AvgBookingValue { get; set; }
        public int CompletedBookings { get; set; }
    }
}
