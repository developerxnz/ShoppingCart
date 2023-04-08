namespace Shopping.Core;

public abstract record Aggregate<T> where T : Aggregate<T>
{
    // Ensure we're only able to use this class in the domain itself
    internal Aggregate() { }
}