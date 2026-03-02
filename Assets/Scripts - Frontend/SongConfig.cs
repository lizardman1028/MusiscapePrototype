using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SongConfig", menuName = "Scriptable Objects/SongConfig")]
public class SongConfig : ScriptableObject {
  public string songName;
  public List<AudioClip> clips;
  public List<Sprite> sprites;
  public List<string> trackNames;
}
