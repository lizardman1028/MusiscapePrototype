using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Single AudioSource playback for library tracks. Loads original.mp3 via file:// and plays.
/// No spatialization or stems.
/// </summary>
public class LibraryPlayback : MonoBehaviour
{
    private const string OriginalFileName = "original.mp3";

    [SerializeField] private AudioSource _audioSource;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Loads original.mp3 from the track folder (folderPath relative to app root) and plays.
    /// </summary>
    public void RequestPlay(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
            return;

        string root = FileSystemManager.Instance.RootPath;
        string fullPath = Path.Combine(root, folderPath, OriginalFileName);
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"[LibraryPlayback] File not found: {fullPath}");
            return;
        }

        string uri = "file:///" + fullPath.Replace("\\", "/");
        StartCoroutine(LoadAndPlay(uri));
    }

    private System.Collections.IEnumerator LoadAndPlay(string fileUri)
    {
        using (var request = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[LibraryPlayback] Load failed: {request.error}");
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            if (clip != null && _audioSource != null)
            {
                _audioSource.clip = clip;
                _audioSource.spatialBlend = 0f;
                _audioSource.Play();
            }
        }
    }

    /// <summary>
    /// Stops playback and clears the current clip.
    /// </summary>
    public void Stop()
    {
        if (_audioSource != null)
        {
            _audioSource.Stop();
            _audioSource.clip = null;
        }
    }
}
