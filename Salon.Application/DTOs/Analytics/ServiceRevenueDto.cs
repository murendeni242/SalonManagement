using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salon.Application.DTOs.Analytics
{
    public class ServiceRevenueDto
    {
        public string ServiceName { get; set; } = default!;
        public decimal Revenue { get; set; }
        public int Bookings { get; set; }
    }
}
