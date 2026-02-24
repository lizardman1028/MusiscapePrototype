using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Manages the local track library: load/save library.json and expose add/remove/query.
/// Desktop only; uses FileSystemManager paths and JsonUtility. No API integration.
/// Fires OnLibraryChanged when the library is loaded, or after add/remove.
/// </summary>
public class LibraryManager : MonoBehaviour
{
    private const string EmptyLibraryJson = "{\"tracks\":[]}";

    private TrackLibrary _library;
    private string _libraryFilePath;

    /// <summary>
    /// Raised when the library data has changed (after load, add, or remove). Use to refresh UI.
    /// </summary>
    public event Action OnLibraryChanged;

    private void Awake()
    {
        _libraryFilePath = FileSystemManager.Instance.LibraryFilePath;
        LoadLibrary();
    }

    /// <summary>
    /// Loads library.json from disk. If missing or empty, initializes with an empty track list.
    /// </summary>
    private void LoadLibrary()
    {
        try
        {
            if (!File.Exists(_libraryFilePath))
            {
                _library = new TrackLibrary();
                SaveLibrary();
                OnLibraryChanged?.Invoke();
                return;
            }

            string json = File.ReadAllText(_libraryFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                _library = new TrackLibrary();
                SaveLibrary();
                OnLibraryChanged?.Invoke();
                return;
            }

            _library = JsonUtility.FromJson<TrackLibrary>(json);
            if (_library == null || _library.tracks == null)
            {
                _library = new TrackLibrary();
                SaveLibrary();
                OnLibraryChanged?.Invoke();
                return;
            }

            OnLibraryChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LibraryManager] Load failed, initializing empty library: {ex.Message}");
            _library = new TrackLibrary();
            SaveLibrary();
            OnLibraryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Writes the current library to library.json using JsonUtility.
    /// </summary>
    private void SaveLibrary()
    {
        try
        {
            string json = _library.tracks == null || _library.tracks.Count == 0
                ? EmptyLibraryJson
                : JsonUtility.ToJson(_library, prettyPrint: true);
            File.WriteAllText(_libraryFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LibraryManager] Save failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Adds a track to the library and saves library.json.
    /// </summary>
    public void AddTrack(TrackData track)
    {
        if (track == null)
        {
            Debug.LogWarning("[LibraryManager] AddTrack: track is null.");
            return;
        }

        if (_library.tracks == null)
            _library.tracks = new List<TrackData>();

        int existing = _library.tracks.FindIndex(t => string.Equals(t.id, track.id, StringComparison.Ordinal));
        if (existing >= 0)
        {
            _library.tracks[existing] = track;
        }
        else
        {
            _library.tracks.Add(track);
        }

        SaveLibrary();
        OnLibraryChanged?.Invoke();
    }

    /// <summary>
    /// Removes the track with the given id and saves library.json.
    /// </summary>
    public void RemoveTrack(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (_library.tracks == null) return;

        int removed = _library.tracks.RemoveAll(t => string.Equals(t.id, id, StringComparison.Ordinal));
        if (removed > 0)
        {
            SaveLibrary();
            OnLibraryChanged?.Invoke();
        }
    }

    /// <summary>
    /// Returns all tracks in the library. Returns a new list so callers cannot mutate the internal list.
    /// </summary>
    public List<TrackData> GetAllTracks()
    {
        if (_library?.tracks == null)
            return new List<TrackData>();
        return new List<TrackData>(_library.tracks);
    }
}
