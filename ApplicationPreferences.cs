namespace RealmStudioX.Infrastructure;

public static class ApplicationPreferences
{
    private static IApplicationPreferences? _current;

    public static IApplicationPreferences Current =>
        _current ?? throw new InvalidOperationException(
            "ApplicationPreferences has not been initialized.");

    public static void Initialize(
        IApplicationPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        if (_current is not null)
        {
            throw new InvalidOperationException(
                "ApplicationPreferences has already been initialized.");
        }

        _current = preferences;
    }
}