namespace MarketingPoc.Models;

public class TestResult
{
    public int Id { get; set; }
    public string TestType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public long RequestCount { get; set; }
    public long ErrorCount { get; set; }
    public double P95Latency { get; set; }
    public double AvgCpuUsage { get; set; }
}
