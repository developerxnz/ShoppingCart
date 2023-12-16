namespace Shopping.Core;

public record QueuePayload<T>(CorrelationId CorrelationId, T Data);

public record PointsEarned(DateTime EarnedOnUtc, uint Points);

public class x
{
    public x()
    {
        var data = new PointsEarned(DateTime.UtcNow, 100);
        var t = new QueuePayload<PointsEarned>(new CorrelationId(Guid.NewGuid()), data);
    }
}