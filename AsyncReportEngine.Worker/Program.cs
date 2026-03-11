using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.DataAccess.Repositories;
using AsyncReportEngine.Services;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Worker;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.EntityFrameworkCore;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddDbContext<ReportDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IBlobService, BlobService>();

        services.AddSingleton<QueueClient>(provider =>
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            return new QueueClient(connectionString, "report-jobs");
        });

        services.AddSingleton<BlobServiceClient>(provider =>
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");
            return new BlobServiceClient(connectionString);
        });

        services.AddHostedService<ReportWorker>();
    })
    .Build();

await host.RunAsync();