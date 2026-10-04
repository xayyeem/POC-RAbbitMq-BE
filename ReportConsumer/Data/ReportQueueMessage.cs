using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportConsumer.Data
{
    public class ReportQueueMessage
    {
        public Guid RequestId { get; set; }

        public GenerateReportRequest ReportRequest { get; set; }
    }
}
