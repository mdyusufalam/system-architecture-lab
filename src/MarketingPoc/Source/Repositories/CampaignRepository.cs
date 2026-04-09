using MarketingPoc.Data;
using MarketingPoc.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketingPoc.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly AppDbContext _dbContext;

    private static readonly Func<AppDbContext, int, int, int, IAsyncEnumerable<Campaign>> _campaignsByTenantQuery =
        EF.CompileAsyncQuery((AppDbContext context, int tenantId, int page, int pageSize) =>
            context.Campaigns
                .AsNoTracking()
                .Where(c => c.TenantId == tenantId)
                .OrderBy(c => c.ScheduledAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize));

    public CampaignRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Campaign>> GetCampaignsByTenantAsync(int tenantId, int page = 1, int pageSize = 50)
    {
        return _campaignsByTenantQuery(_dbContext, tenantId, page, pageSize)
            .ToListAsync()
            .AsTask();
    }
}