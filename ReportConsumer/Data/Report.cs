using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportConsumer.Data
{
    public class Report
    {
        public string BatcheIds { get; set; }

        public Guid LicenseeId { get; set; }

        public string AgentIds { get; set; }

        public string SegmentIds { get; set; }

        public string ReportNames { get; set; }

        public Guid ReportId { get; set; }

        public bool IsZero { get; set; }

        public string PaymentType { get; set; }

        public string ReportType { get; set; }

        public bool IsSubTotal { get; set; }

        public string Email { get; set; }
    }
}
