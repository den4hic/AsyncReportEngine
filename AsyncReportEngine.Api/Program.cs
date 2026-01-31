using AsyncReportEngine.Api.Extensions;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.DataAccess.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterDependencies(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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