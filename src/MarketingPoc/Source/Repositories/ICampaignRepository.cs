using MarketingPoc.Models;

namespace MarketingPoc.Repositories;

public interface ICampaignRepository
{
    Task<List<Campaign>> GetCampaignsByTenantAsync(int tenantId, int page = 1, int pageSize = 50);
}