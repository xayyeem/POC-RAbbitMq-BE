namespace WebApplication1.Models
{
    public class ReportQueueMessage
    {
        public Guid RequestId { get; set; }

        public GenerateReportRequest ReportRequest { get; set; }
    }
}
