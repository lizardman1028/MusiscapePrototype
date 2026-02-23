using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Runs once at startup: if the library is empty, copies demo assets from
/// StreamingAssets/DemoTracks into persistent DemoTracks and registers them.
/// Does not duplicate if library already has tracks. Logic kept separate from LibraryManager.
/// </summary>
public class DemoTrackInitializer : MonoBehaviour
{
    private const string StreamingDemoFolderName = "DemoTracks";
    private const string DemoArtistName = "Demo";

    [SerializeField] [Tooltip("If not set, will be resolved at runtime.")]
    private LibraryManager _libraryManager;

    private void Start()
    {
        LibraryManager libraryManager = _libraryManager != null ? _libraryManager : FindObjectOfType<LibraryManager>();
        if (libraryManager == null)
        {
            Debug.LogWarning("[DemoTrackInitializer] LibraryManager not found. Skipping demo initialization.");
            return;
        }

        if (libraryManager.GetAllTracks().Count > 0)
            return;

        string sourceRoot = Path.Combine(Application.streamingAssetsPath, StreamingDemoFolderName);
        if (!Directory.Exists(sourceRoot))
        {
            Debug.Log($"[DemoTrackInitializer] No demo source folder at {sourceRoot}. Skipping.");
            return;
        }

        string demoPersistentPath = FileSystemManager.Instance.DemoTracksPath;
        string[] entries;

        try
        {
            entries = Directory.GetFileSystemEntries(sourceRoot);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DemoTrackInitializer] Cannot read demo folder: {ex.Message}");
            return;
        }

        foreach (string sourcePath in entries)
        {
            string name = Path.GetFileName(sourcePath);
            if (string.IsNullOrEmpty(name) || name.StartsWith(".", StringComparison.Ordinal))
                continue;

            string id = Guid.NewGuid().ToString();
            string destFolder = Path.Combine(demoPersistentPath, id);

            try
            {
                Directory.CreateDirectory(destFolder);
                CopyToFolder(sourcePath, destFolder);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DemoTrackInitializer] Failed to copy demo '{name}': {ex.Message}");
                continue;
            }

            string title = Path.GetFileNameWithoutExtension(name);
            if (Directory.Exists(sourcePath))
                title = name;

            string relativeFolderPath = $"DemoTracks/{id}/";
            var track = new TrackData(
                id,
                title,
                DemoArtistName,
                relativeFolderPath,
                isBundled: true,
                stemsReady: false
            );
            libraryManager.AddTrack(track);
        }
    }

    /// <summary>
    /// Copies a file or directory into the target folder. For directories, copies contents (no extra nested folder).
    /// </summary>
    private static void CopyToFolder(string sourcePath, string destFolderPath)
    {
        if (File.Exists(sourcePath))
        {
            string destFile = Path.Combine(destFolderPath, Path.GetFileName(sourcePath));
            File.Copy(sourcePath, destFile, overwrite: false);
            return;
        }

        if (Directory.Exists(sourcePath))
        {
            CopyDirectoryContents(sourcePath, destFolderPath);
            return;
        }
    }

    private static void CopyDirectoryContents(string sourceDir, string destDir)
    {
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: false);
        }

        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string subDirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destDir, subDirName);
            Directory.CreateDirectory(destSubDir);
            CopyDirectoryContents(subDir, destSubDir);
        }
    }
}
