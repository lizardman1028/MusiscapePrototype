using System;
using System.Collections.Generic;

/// <summary>
/// Root container for library JSON. Serialized to/from library.json via JsonUtility.
/// </summary>
[Serializable]
public class TrackLibrary
{
    public List<TrackData> tracks;

    public TrackLibrary()
    {
        tracks = new List<TrackData>();
    }

    public TrackLibrary(List<TrackData> tracks)
    {
        this.tracks = tracks ?? new List<TrackData>();
    }
}
