namespace Shopping.Core;

public interface IRepository<T, in T1> where T1: IEvent
{
    /// <summary>
    /// Returns a document for the specified PartitionKeys and Id
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T> GetByIdAsync(string partitionKey, string id, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a document for the specified PartitionKeys and Id
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey, CancellationToken cancellationToken);

    /// <summary>
    /// Batch update aggregate with events
    /// </summary>
    /// <param name="partitionKey"></param>
    /// <param name="aggregate"></param>
    /// <param name="events"></param>
    /// <returns></returns>
    Task BatchUpdate(string partitionKey, T aggregate, IEnumerable<T1> events);
}