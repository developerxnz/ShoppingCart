using System.Text.Json.Serialization;

namespace Shopping.Infrastructure.Interfaces;

public interface IPersistenceIdentifier
{
    [JsonPropertyName("id")]
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
    Task BatchUpdateAsync(T aggregate, IEnumerable<Infrastructure.Interfaces.IEvent> events, CancellationToken cancellationToken);
}