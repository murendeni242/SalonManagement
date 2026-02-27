using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salon.Application.DTOs.Analytics
{
    public class DayOfWeekDto
    {
        public string Day { get; set; } = default!;  // "Mon", "Tue", etc.
        public int Bookings { get; set; }
    }
}
