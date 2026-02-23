using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Single row in the library list: title, artist, optional bundled icon. Click to play (original.mp3).
/// No spatialization or stems.
/// </summary>
[RequireComponent(typeof(Button))]
public class TrackItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _artistText;
    [SerializeField] [Tooltip("Optional. Shown only when track is bundled.")]
    private GameObject _bundledIcon;
    [SerializeField] [Tooltip("If not set, resolved at runtime.")]
    private LibraryPlayback _playback;

    private string _trackId;
    private string _folderPath;

    /// <summary>
    /// Id of the track currently displayed (for remove/selection). Empty if not set.
    /// </summary>
    public string TrackId => _trackId;

    private void Awake()
    {
        Button button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// Fills the row with track data and shows/hides the bundled icon.
    /// </summary>
    public void SetTrack(TrackData track)
    {
        if (track == null)
        {
            _trackId = null;
            _folderPath = null;
            if (_titleText != null) _titleText.text = "";
            if (_artistText != null) _artistText.text = "";
            if (_bundledIcon != null) _bundledIcon.SetActive(false);
            return;
        }

        _trackId = track.id;
        _folderPath = track.folderPath;
        if (_titleText != null) _titleText.text = string.IsNullOrEmpty(track.title) ? "—" : track.title;
        if (_artistText != null) _artistText.text = string.IsNullOrEmpty(track.artist) ? "—" : track.artist;
        if (_bundledIcon != null) _bundledIcon.SetActive(track.isBundled);
    }

    private void OnClick()
    {
        if (string.IsNullOrEmpty(_folderPath))
            return;

        LibraryPlayback playback = _playback != null ? _playback : FindObjectOfType<LibraryPlayback>();
        if (playback != null)
            playback.RequestPlay(_folderPath);
    }
}
