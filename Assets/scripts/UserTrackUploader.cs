using System;
using System.IO;
using UnityEngine;
#if USE_STANDALONE_FILE_BROWSER
using StandaloneFileBrowser;
#endif

/// <summary>
/// Handles user audio uploads via file picker: copies selected file into UserTracks/{id}/ as original.mp3
/// and registers with LibraryManager. Local only; no playback or metadata extraction.
/// Requires StandaloneFileBrowser package and scripting define USE_STANDALONE_FILE_BROWSER for the picker to work.
/// </summary>
public class UserTrackUploader : MonoBehaviour
{
    private const string OriginalFileName = "original.mp3";
    private const string UnknownArtist = "Unknown";
    private const string FilePickerTitle = "Select audio file";
    private const string Mp3Filter = "mp3";

    [SerializeField] [Tooltip("If not set, will be resolved at runtime.")]
    private LibraryManager _libraryManager;

    /// <summary>
    /// Opens a file picker. If the user selects a file, copies it to UserTracks/{id}/ as original.mp3
    /// and adds a TrackData entry to the library (saved via LibraryManager).
    /// </summary>
    public void OpenFilePicker()
    {
        LibraryManager libraryManager = _libraryManager != null ? _libraryManager : FindObjectOfType<LibraryManager>();
        if (libraryManager == null)
        {
            Debug.LogError("[UserTrackUploader] LibraryManager not found.");
            return;
        }

#if USE_STANDALONE_FILE_BROWSER
        string[] paths = FileBrowser.OpenFilePanel(FilePickerTitle, "", Mp3Filter, false);
        if (paths == null || paths.Length == 0)
            return;

        string selectedPath = paths[0];
        if (string.IsNullOrWhiteSpace(selectedPath) || !File.Exists(selectedPath))
        {
            Debug.LogWarning("[UserTrackUploader] No valid file selected.");
            return;
        }
        ImportSelectedFile(selectedPath, libraryManager);
#else
        Debug.LogWarning("[UserTrackUploader] StandaloneFileBrowser not linked. Add the package and set scripting define USE_STANDALONE_FILE_BROWSER (Edit → Project Settings → Player → Script Compilation).");
#endif
    }

    private void ImportSelectedFile(string sourceFilePath, LibraryManager libraryManager)
    {
        string id = Guid.NewGuid().ToString();
        string userTracksPath = FileSystemManager.Instance.UserTracksPath;
        string trackFolder = Path.Combine(userTracksPath, id);
        string destFilePath = Path.Combine(trackFolder, OriginalFileName);

        try
        {
            Directory.CreateDirectory(trackFolder);
            File.Copy(sourceFilePath, destFilePath, overwrite: false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[UserTrackUploader] Failed to copy file: {ex.Message}");
            return;
        }

        string title = Path.GetFileNameWithoutExtension(sourceFilePath);
        string folderPath = $"UserTracks/{id}/";

        var track = new TrackData(
            id,
            title,
            UnknownArtist,
            folderPath,
            isBundled: false,
            stemsReady: false
        );
        libraryManager.AddTrack(track);
    }
}
