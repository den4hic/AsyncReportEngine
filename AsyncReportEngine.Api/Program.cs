using AsyncReportEngine.Api.Extensions;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.DataAccess.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<ReportDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
//        b => b.MigrationsAssembly("AsyncReportEngine.DataAccess")));

//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//    .AddEntityFrameworkStores<ReportDbContext>()
//    .AddDefaultTokenProviders();

builder.Services.RegisterDependencies(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureStorage:blobServiceUri"]!).WithName("AzureStorage");
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureStorage:queueServiceUri"]!).WithName("AzureStorage");
    clientBuilder.AddTableServiceClient(builder.Configuration["AzureStorage:tableServiceUri"]!).WithName("AzureStorage");
});
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureStorage:blobServiceUri"]!).WithName("AzureStorage");
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureStorage:queueServiceUri"]!).WithName("AzureStorage");
    clientBuilder.AddTableServiceClient(builder.Configuration["AzureStorage:tableServiceUri"]!).WithName("AzureStorage");
});
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureStorage:blobServiceUri"]!).WithName("AzureStorage");
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureStorage:queueServiceUri"]!).WithName("AzureStorage");
    clientBuilder.AddTableServiceClient(builder.Configuration["AzureStorage:tableServiceUri"]!).WithName("AzureStorage");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ReportDbContext>();

        context.Database.EnsureCreated();

        await DataSeeder.SeedAsync(context);
    }
    catch (Exception)
    {
    }
}

app.Run();