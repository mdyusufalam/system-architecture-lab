namespace MarketingPoc.Models.Dtos;

public class CreateTestResultRequest
{
    public string TestType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public long RequestCount { get; set; }
    public long ErrorCount { get; set; }
    public double P95Latency { get; set; }
    public double AvgCpuUsage { get; set; }
}

public class TestResultResponse
{
    public int Id { get; set; }
    public string TestType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public long RequestCount { get; set; }
    public long ErrorCount { get; set; }
    public double P95Latency { get; set; }
    public double AvgCpuUsage { get; set; }
    public double SuccessRate => RequestCount > 0 ? ((RequestCount - ErrorCount) / (double)RequestCount) * 100 : 0;
    public double DurationSeconds => (EndTime - StartTime).TotalSeconds;
}