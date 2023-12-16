namespace Shopping.Domain.Extensions;

public static class EnumerableExtensions
{
    public static T2 Fold<T1, T2>(this IEnumerable<T1> items, Func<T1, T2, T2> folder, T2 aggregate)
    {
        return items.Aggregate(aggregate, (current, item) => folder(item, current));
    }
}