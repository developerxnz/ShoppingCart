namespace Shopping.Core.Persistence;

public record Metadata(
    string StreamId, 
    
    uint Version, 
    
    DateTime Timestamp);