using MarketingPoc.Models;

namespace MarketingPoc.Repositories;

public interface ITenantRepository
{
    Task<bool> TenantExistsAsync(int tenantId);
    Task<Tenant?> GetTenantAsync(int tenantId);
    Task<List<Tenant>> GetAllTenantsAsync();
}