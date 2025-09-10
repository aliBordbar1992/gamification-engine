namespace GamificationEngine.Shared;

public enum PointCategoryAggregation
{
    Sum,
    Max,
    Min,
    Avg,
    Count
}

public static class PointCategoryAggregationExtensions
{
    public static PointCategoryAggregation ToPointCategoryAggregation(this string aggregation)
    {
        return aggregation switch
        {
            "sum" => PointCategoryAggregation.Sum,
            "max" => PointCategoryAggregation.Max,
            "min" => PointCategoryAggregation.Min,
            "avg" => PointCategoryAggregation.Avg,
            "count" => PointCategoryAggregation.Count,
            _ => throw new ArgumentException($"Invalid aggregation: {aggregation}")
        };
    }

    public static string ToAggregationString(this PointCategoryAggregation aggregation)
    {
        return aggregation switch
        {
            PointCategoryAggregation.Sum => "sum",
            PointCategoryAggregation.Max => "max",
            PointCategoryAggregation.Min => "min",
            PointCategoryAggregation.Avg => "avg",
            PointCategoryAggregation.Count => "count",
            _ => throw new ArgumentException($"Invalid aggregation: {aggregation}")
        };
    }
}