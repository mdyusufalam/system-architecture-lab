using MarketingPoc.Models;
using MarketingPoc.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MarketingPoc.Controllers;

[ApiController]
[Route("api/tenants/{tenantId}/campaigns")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ITenantRepository _tenantRepository;

    public CampaignsController(ICampaignRepository campaignRepository, ITenantRepository tenantRepository)
    {
        _campaignRepository = campaignRepository;
        _tenantRepository = tenantRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetCampaigns(int tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (!await _tenantRepository.TenantExistsAsync(tenantId))
        {
            return NotFound(new { Message = $"Tenant {tenantId} not found" });
        }

        var campaigns = await _campaignRepository.GetCampaignsByTenantAsync(tenantId, page, pageSize);
        return Ok(campaigns);
    }
}