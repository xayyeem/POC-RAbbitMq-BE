using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportConsumer.Services
{
    public class ExcelReportGenerator
    {
        private readonly IConfiguration _configuration;

        public ExcelReportGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Generate(
            Guid requestId,
            string reportName)
        {
            var reportFolder =
                _configuration["ReportSettings:ReportFolder"];

            if (string.IsNullOrWhiteSpace(reportFolder))
            {
                reportFolder = Path.Combine(
                    AppContext.BaseDirectory,
                    "Reports");
            }

            Directory.CreateDirectory(reportFolder);

            var fileName =
                $"{reportName}_{requestId}.xlsx";

            var filePath =
                Path.Combine(reportFolder, fileName);

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Report");

            worksheet.Cell(1, 1).Value = "RequestId";
            worksheet.Cell(1, 2).Value = "ReportName";
            worksheet.Cell(1, 3).Value = "GeneratedOn";

            worksheet.Cell(2, 1).Value =
                requestId.ToString();

            worksheet.Cell(2, 2).Value =
                reportName;

            worksheet.Cell(2, 3).Value =
                DateTime.UtcNow;

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);

            return filePath;
        }
    }
}
