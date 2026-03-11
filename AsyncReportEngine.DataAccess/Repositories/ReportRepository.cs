using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.Shared.Entities;
using AsyncReportEngine.Shared.Enum;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly ReportDbContext context;

    public ReportRepository(ReportDbContext context)
    {
        this.context = context;
    }

    public async Task<Guid> CreateRequestAsync(Guid requestId, string userId)
    {
        var request = new ReportRequest
        {
            Id = requestId,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await context.ReportRequests.AddAsync(request);
        await context.SaveChangesAsync();
        return requestId;
    }

    public async Task UpdateStatusAsync(Guid requestId, ReportStatus status, string? fileUrl = null, string? error = null)
    {
        var request = await context.ReportRequests.FindAsync(requestId);
        if (request != null)
        {
            request.Status = status;
            if (status == ReportStatus.Completed || status == ReportStatus.Failed)
            {
                request.FinishedAt = DateTime.UtcNow;
            }

            if (fileUrl != null) request.FileUrl = fileUrl;
            if (error != null) request.ErrorMessage = error;

            await context.SaveChangesAsync();
        }
    }

    public async Task<ReportRequest?> GetRequestByIdAsync(Guid requestId)
    {
        return await context.ReportRequests.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == requestId);
    }

    public async Task<List<ReportRequest>> GetUserRequestsAsync(string userId)
    {
        return await context.ReportRequests.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersForReportAsync(DateTime startDate, DateTime endDate)
    {
        return await context.Orders
            .AsNoTracking()
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .Include(o => o.Customer)
                .ThenInclude(c => c.Addresses)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Category)
            .Include(o => o.ShippingMethod)
            .Include(o => o.Transactions)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOrdersForReportAsync(DateTime startDate, DateTime endDate, int? customerId = null)
    {
        var query = context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Transactions)
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
            .AsNoTracking();

        if (customerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        return await query.ToListAsync();
    }
}
