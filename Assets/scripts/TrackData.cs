using System;

/// <summary>
/// Represents a single track entry in the local library.
/// Used for JSON serialization via JsonUtility.
/// </summary>
[Serializable]
public class TrackData
{
    public string id;
    public string title;
    public string artist;
    public string folderPath;
    public bool isBundled;
    public bool stemsReady;

    public TrackData() { }

    public TrackData(string id, string title, string artist, string folderPath, bool isBundled = false, bool stemsReady = false)
    {
        this.id = id;
        this.title = title;
        this.artist = artist;
        this.folderPath = folderPath;
        this.isBundled = isBundled;
        this.stemsReady = stemsReady;
    }
}
