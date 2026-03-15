using AsyncReportEngine.DataAccess.Abstraction.Repositories;
using AsyncReportEngine.Services.Abstraction;
using AsyncReportEngine.Shared.Dtos.Partners;
using AutoMapper;

namespace AsyncReportEngine.Services;

public class PartnerService : IPartnerService
{
    private readonly IPartnerRepository customerRepository;
    private readonly IMapper mapper;

    public PartnerService(IPartnerRepository customerRepository, IMapper mapper)
    {
        this.customerRepository = customerRepository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<PartnerDto>> GetAllAsync()
    {
        var partners = await customerRepository.GetAllAsync();

        return mapper.Map<IEnumerable<PartnerDto>>(partners);
    }

    public async Task<PartnerDto?> GetByIdAsync(int id)
    {
        var partner = await customerRepository.GetByIdAsync(id);

        return mapper.Map<PartnerDto>(partner);
    }
}
