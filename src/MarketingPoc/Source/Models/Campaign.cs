namespace MarketingPoc.Models;

public class Campaign
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }

    public Tenant? Tenant { get; set; }
}