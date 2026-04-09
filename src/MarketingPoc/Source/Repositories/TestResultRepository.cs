using MarketingPoc.Data;
using MarketingPoc.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketingPoc.Repositories;

public class TestResultRepository : ITestResultRepository
{
    private readonly AppDbContext _dbContext;

    public TestResultRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddTestResultAsync(TestResult testResult)
    {
        await _dbContext.TestResults.AddAsync(testResult);
        await _dbContext.SaveChangesAsync();
    }

    public Task<List<TestResult>> GetTestResultsAsync(string? testType = null)
    {
        var query = _dbContext.TestResults.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(testType))
        {
            query = query.Where(t => t.TestType == testType);
        }

        return query.OrderByDescending(t => t.StartTime).ToListAsync();
    }

    public Task<TestResult?> GetTestResultAsync(int id)
    {
        return _dbContext.TestResults.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    }
}