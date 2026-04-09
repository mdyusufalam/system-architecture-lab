using MarketingPoc.Data;
using MarketingPoc.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketingPoc.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly AppDbContext _dbContext;

    public TenantRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> TenantExistsAsync(int tenantId)
    {
        return _dbContext.Tenants.AnyAsync(t => t.Id == tenantId);
    }

    public Task<Tenant?> GetTenantAsync(int tenantId)
    {
        return _dbContext.Tenants.FindAsync(tenantId).AsTask();
    }

    public Task<List<Tenant>> GetAllTenantsAsync()
    {
        return _dbContext.Tenants.AsNoTracking().ToListAsync();
    }
}