using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Populates a ScrollView content with TrackItemUI instances from LibraryManager.
/// Refreshes when the library changes (add/remove/load). No playback or spatialization.
/// </summary>
public class LibraryUIController : MonoBehaviour
{
    [SerializeField] private LibraryManager _libraryManager;
    [SerializeField] [Tooltip("Prefab with TrackItemUI on root or child.")]
    private TrackItemUI _trackItemPrefab;
    [SerializeField] [Tooltip("Content under ScrollView (with VerticalLayoutGroup).")]
    private RectTransform _content;

    private void OnEnable()
    {
        if (_libraryManager != null)
            _libraryManager.OnLibraryChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (_libraryManager != null)
            _libraryManager.OnLibraryChanged -= Refresh;
    }

    /// <summary>
    /// Rebuilds the list: clears content and instantiates one TrackItemUI per track.
    /// </summary>
    public void Refresh()
    {
        if (_trackItemPrefab == null || _content == null)
            return;

        LibraryManager library = _libraryManager != null ? _libraryManager : FindObjectOfType<LibraryManager>();
        if (library == null)
            return;

        ClearContent();

        List<TrackData> tracks = library.GetAllTracks();
        foreach (TrackData track in tracks)
        {
            TrackItemUI item = Instantiate(_trackItemPrefab);
            item.transform.SetParent(_content, false);
            item.SetTrack(track);
        }
    }

    private void ClearContent()
    {
        for (int i = _content.childCount - 1; i >= 0; i--)
        {
            Transform child = _content.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
