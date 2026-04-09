using MarketingPoc.Models;
using MarketingPoc.Models.Dtos;
using MarketingPoc.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MarketingPoc.Controllers;

[ApiController]
[Route("api/tests")]
public class TestObservationController : ControllerBase
{
    private readonly ITestResultRepository _testResultRepository;

    public TestObservationController(ITestResultRepository testResultRepository)
    {
        _testResultRepository = testResultRepository;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TestResultResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTestResult([FromBody] CreateTestResultRequest request)
    {
        var testResult = new TestResult
        {
            TestType = request.TestType,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            RequestCount = request.RequestCount,
            ErrorCount = request.ErrorCount,
            P95Latency = request.P95Latency,
            AvgCpuUsage = request.AvgCpuUsage
        };

        await _testResultRepository.AddTestResultAsync(testResult);

        var response = new TestResultResponse
        {
            Id = testResult.Id,
            TestType = testResult.TestType,
            StartTime = testResult.StartTime,
            EndTime = testResult.EndTime,
            RequestCount = testResult.RequestCount,
            ErrorCount = testResult.ErrorCount,
            P95Latency = testResult.P95Latency,
            AvgCpuUsage = testResult.AvgCpuUsage
        };

        return CreatedAtAction(nameof(GetTestResult), new { id = testResult.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TestResultResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTestResults([FromQuery] string? testType = null)
    {
        var results = await _testResultRepository.GetTestResultsAsync(testType);
        var responses = results.Select(r => new TestResultResponse
        {
            Id = r.Id,
            TestType = r.TestType,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            RequestCount = r.RequestCount,
            ErrorCount = r.ErrorCount,
            P95Latency = r.P95Latency,
            AvgCpuUsage = r.AvgCpuUsage
        }).ToList();

        return Ok(responses);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TestResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTestResult(int id)
    {
        var result = await _testResultRepository.GetTestResultAsync(id);
        if (result is null)
            return NotFound();

        var response = new TestResultResponse
        {
            Id = result.Id,
            TestType = result.TestType,
            StartTime = result.StartTime,
            EndTime = result.EndTime,
            RequestCount = result.RequestCount,
            ErrorCount = result.ErrorCount,
            P95Latency = result.P95Latency,
            AvgCpuUsage = result.AvgCpuUsage
        };

        return Ok(response);
    }
}