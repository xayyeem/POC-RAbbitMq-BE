namespace WebApplication1.Models
{
    public class GenerateReportRequest
    {
        public Report Report { get; set; }
        public DateTime? ArchiveStartDate { get; set; }
        public DateTime? ArchiveEndDate { get; set; }
        public Guid UserCredentialId { get; set; }
    }

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
