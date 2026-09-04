using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.Grading;

namespace TmsApi.Tests;

public class GradingServiceTests
{
    [Fact]
    public void CalculateLetterGrade_HighScore_ReturnDistinction()
    {
        var service = new GradingService();
        var result = service.CalculateLetterGrade(score: 85m, maxScore: 100m);
        Assert.Equal(GradeLevel.Distiniction, result);
    }

    [Theory]
    [InlineData(0, 100, GradeLevel.Fail)]
    [InlineData(70, 100, GradeLevel.Distiniction)]
    [InlineData(50, 100, GradeLevel.Pass)]
    [InlineData(-1, 100, GradeLevel.Invalid)]
    [InlineData(101, 100, GradeLevel.Invalid)]
    [InlineData(50, 0, GradeLevel.Invalid)]

    public void CalculateLetterGrade_VariousInputs_ReturnsExpectedLevel (
        decimal score , decimal maxScore, GradeLevel expected)
    {
        var service = new GradingService();
        var result = service.CalculateLetterGrade(score, maxScore);
        Assert.Equal(expected, result);
    }
}