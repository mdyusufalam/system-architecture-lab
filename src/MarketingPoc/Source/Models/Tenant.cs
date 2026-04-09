namespace MarketingPoc.Models;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int RateLimit { get; set; }

    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}