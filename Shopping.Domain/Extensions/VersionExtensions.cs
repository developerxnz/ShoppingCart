namespace Shopping.Domain.Extensions;

public static class VersionExtensions
{
    public static Core.Version Increment(this Core.Version version)
    {
         uint incrementedVersion = version.Value+1;
         return new(incrementedVersion);
    }
}