using AsyncReportEngine.Shared.Dtos.Partners;

namespace AsyncReportEngine.Services.Abstraction;

public interface IPartnerService
{
    Task<IEnumerable<PartnerDto>> GetAllAsync();
    Task<PartnerDto?> GetByIdAsync(int id);
}
