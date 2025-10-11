using System;
using System.IO;

namespace LifTools.Services;

public interface IVersionService
{
    string GetVersion();
}

public class VersionService : IVersionService
{
    private readonly string _versionFilePath;
    private string? _cachedVersion;

    public VersionService()
    {
        // Look for VERSION.txt in the application directory
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _versionFilePath = Path.Combine(appDirectory, "VERSION.txt");
    }

    public string GetVersion()
    {
        if (_cachedVersion != null)
        {
            return _cachedVersion;
        }

        try
        {
            if (File.Exists(_versionFilePath))
            {
                var versionText = File.ReadAllText(_versionFilePath).Trim();
                if (!string.IsNullOrEmpty(versionText))
                {
                    _cachedVersion = versionText;
                    return _cachedVersion;
                }
            }
        }
        catch (Exception)
        {
            // If we can't read the version file, fall back to default
        }

        // Fallback to a default version if file doesn't exist or can't be read
        _cachedVersion = "Unknown";
        return _cachedVersion;
    }
}
