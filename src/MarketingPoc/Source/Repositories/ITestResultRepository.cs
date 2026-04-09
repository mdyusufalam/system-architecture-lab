using MarketingPoc.Models;

namespace MarketingPoc.Repositories;

public interface ITestResultRepository
{
    Task AddTestResultAsync(TestResult testResult);
    Task<List<TestResult>> GetTestResultsAsync(string? testType = null);
    Task<TestResult?> GetTestResultAsync(int id);
}