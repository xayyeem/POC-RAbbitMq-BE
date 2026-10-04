using ReportConsumer;
using ReportConsumer.Data;
using ReportConsumer.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<SyncRepository>();
builder.Services.AddSingleton<ReportRepository>();
builder.Services.AddSingleton<ExcelReportGenerator>();
var host = builder.Build();
host.Run();
