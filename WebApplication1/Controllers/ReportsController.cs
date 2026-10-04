using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly RabbitMqProducer _rabbitMqProducer;
        private readonly ReportRepository _reportRepository;

        public ReportsController(
            RabbitMqProducer rabbitMqProducer,
            ReportRepository reportRepository)
        {
            _rabbitMqProducer = rabbitMqProducer;
            _reportRepository = reportRepository;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport(
            [FromBody] GenerateReportRequest request)
        {
            var requestId = Guid.NewGuid();

            var reportName = request.Report?.ReportNames ?? "Unknown";

            // 1. Save request in DB
            await _reportRepository.InsertRequestAsync(
                requestId,
                reportName);

            // 2. Create RabbitMQ message
            var queueMessage = new ReportQueueMessage
            {
                RequestId = requestId,
                ReportRequest = request
            };

            // 3. Publish to RabbitMQ
            await _rabbitMqProducer.PublishAsync(queueMessage);

            return Ok(new
            {
                RequestId = requestId,
                Status = "In-Queue",
                Message = "Report request has been queued successfully."
            });
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetRequests()
        {
            var requests = await _reportRepository.GetRequestsAsync();

            return Ok(requests);
        }

        [HttpGet("{requestId:guid}/download")]
        public async Task<IActionResult> DownloadReport(Guid requestId)
        {
            var request =
                await _reportRepository.GetRequestByIdAsync(requestId);

            if (request == null)
            {
                return NotFound("Report request not found.");
            }

            if (request.Status != "Completed")
            {
                return BadRequest(
                    "Report is not completed yet.");
            }

            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return NotFound(
                    "Report file path was not found.");
            }

            if (!System.IO.File.Exists(request.FilePath))
            {
                return NotFound(
                    "Report file no longer exists.");
            }

            // 24-hour expiry check
            if (request.ExpiresOn.HasValue &&
                request.ExpiresOn.Value < DateTime.UtcNow)
            {
                return BadRequest(
                    "Report download has expired.");
            }

            var fileBytes =
                await System.IO.File.ReadAllBytesAsync(
                    request.FilePath);

            var fileName =
                Path.GetFileName(request.FilePath);

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}