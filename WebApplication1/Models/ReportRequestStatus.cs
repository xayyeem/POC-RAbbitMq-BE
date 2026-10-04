namespace WebApplication1.Models
{
    public class ReportRequestStatus
    {
        public Guid RequestId { get; set; }

        public string ReportName { get; set; }

        public string Status { get; set; }

        public DateTime RequestedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public DateTime? ExpiresOn { get; set; }

        public string? FilePath { get; set; }
    }
}
