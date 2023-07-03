using Newtonsoft.Json;

namespace Shopping.Core;

public interface IPersistenceIdentifier
{
    [JsonProperty("PartitionKey")]
    string PartitionKey { get; }
    
    [JsonProperty("id")]
    string Id { get; }
}

public interface IRepository<T> where T:IPersistenceIdentifier
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
    /// <param name="aggregate"></param>
    /// <param name="events"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task BatchUpdateAsync(T aggregate, IEnumerable<IEvent> events, CancellationToken cancellationToken);
}