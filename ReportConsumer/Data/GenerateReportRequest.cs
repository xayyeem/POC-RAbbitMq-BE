using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportConsumer.Data
{
    public class GenerateReportRequest
    {
        public Report Report { get; set; }

        public DateTime? ArchiveStartDate { get; set; }

        public DateTime? ArchiveEndDate { get; set; }

        public Guid UserCredentialId { get; set; }
    }
}
