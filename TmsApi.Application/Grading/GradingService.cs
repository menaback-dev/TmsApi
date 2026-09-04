namespace TmsApi.Application.Grading;

public class GradingService
{
    public const decimal DistinctionThreshold = 70m;
    public const decimal PassThreshold = 50m;
    public GradeLevel CalculateLetterGrade(decimal score, decimal maxScore)
    {
        if (maxScore <= 0m || score < 0m || score > maxScore)
            return GradeLevel.Invalid;

        var pct = score / maxScore * 100m;
        return pct >= DistinctionThreshold ? GradeLevel.Distiniction
              : pct >= PassThreshold ? GradeLevel.Pass
              : GradeLevel.Fail;
    }
    public GradeLevel CalculateFromEnrollmentGrade (decimal? enrollmentGradePercent)
    {
        if (enrollmentGradePercent is null) return GradeLevel.Invalid;
        return CalculateLetterGrade(enrollmentGradePercent.Value, maxScore: 100m);
    }
}