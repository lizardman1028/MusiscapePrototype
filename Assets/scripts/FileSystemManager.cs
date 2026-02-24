using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages the application file system structure under Application.persistentDataPath.
/// Creates and exposes paths for app data, demo tracks, user tracks, and library file.
/// Thread-safe singleton; structure is ensured on first access.
/// </summary>
public sealed class FileSystemManager
{
    private const string AppFolderName = "YourApp";
    private const string DemoTracksFolderName = "DemoTracks";
    private const string UserTracksFolderName = "UserTracks";
    private const string LibraryFileName = "library.json";

    private static readonly Lazy<FileSystemManager> LazyInstance =
        new Lazy<FileSystemManager>(() => new FileSystemManager());

    private readonly string _rootPath;
    private readonly string _demoTracksPath;
    private readonly string _userTracksPath;
    private readonly string _libraryFilePath;

    private FileSystemManager()
    {
        _rootPath = Path.Combine(Application.persistentDataPath, AppFolderName);
        _demoTracksPath = Path.Combine(_rootPath, DemoTracksFolderName);
        _userTracksPath = Path.Combine(_rootPath, UserTracksFolderName);
        _libraryFilePath = Path.Combine(_rootPath, LibraryFileName);
        EnsureStructure();
    }

    /// <summary>
    /// Singleton instance. Access this to ensure the file structure is created on first use.
    /// </summary>
    public static FileSystemManager Instance => LazyInstance.Value;

    /// <summary>
    /// Root folder for the app: persistentDataPath/YourApp/
    /// </summary>
    public string RootPath => _rootPath;

    /// <summary>
    /// Folder for demo tracks: persistentDataPath/YourApp/DemoTracks/
    /// </summary>
    public string DemoTracksPath => _demoTracksPath;

    /// <summary>
    /// Folder for user tracks: persistentDataPath/YourApp/UserTracks/
    /// </summary>
    public string UserTracksPath => _userTracksPath;

    /// <summary>
    /// Path to the library file: persistentDataPath/YourApp/library.json
    /// </summary>
    public string LibraryFilePath => _libraryFilePath;

    /// <summary>
    /// Creates directories and library file if they do not exist. Safe to call multiple times.
    /// </summary>
    public void EnsureStructure()
    {
        try
        {
            if (!Directory.Exists(_rootPath))
                Directory.CreateDirectory(_rootPath);

            if (!Directory.Exists(_demoTracksPath))
                Directory.CreateDirectory(_demoTracksPath);

            if (!Directory.Exists(_userTracksPath))
                Directory.CreateDirectory(_userTracksPath);

            if (!File.Exists(_libraryFilePath))
                File.WriteAllText(_libraryFilePath, "{}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FileSystemManager] Failed to ensure structure: {ex.Message}");
            throw;
        }
    }
}
