using System.Collections.Generic;
using UnityEngine;

public class AudioChipManager : MonoBehaviour {
  [SerializeField]
  private List<AudioClip> clips;
  
  [SerializeField]
  private List<Sprite> sprites;
  
  [SerializeField]
  private GameObject audioChipPrefab;
  
  private List<AudioChip> audioChips;

  private bool isPlaying;
  private bool isPaused;
  
  public bool IsPlaying => isPlaying;
  public bool IsPaused => isPaused;
  
  void Start() {
    audioChips = new List<AudioChip>();
    StartMultiTrack();
  }
  
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void StartMultiTrack() {
    for (int i = 0; i < clips.Count; i++) {
      GameObject audioChip = Instantiate(audioChipPrefab);
      audioChip.GetComponent<AudioChip>().Initialize(clips[i], sprites[i]);
      audioChips.Add(audioChip.GetComponent<AudioChip>());
    }
    // Restart();
  }

  public void Pause() {
    foreach (AudioChip audioChip in audioChips) {
      audioChip.PauseChip();
    }
    isPlaying = false;
    isPaused = true;
  }

  public void Resume() {
    foreach (AudioChip audioChip in audioChips) {
      audioChip.ResumeChip();
    }
    isPlaying = true;
    isPaused = false;
  }

  public void Stop() {
    foreach (AudioChip audioChip in audioChips) {
      audioChip.StopChip();
    }
    isPlaying = false;
    isPaused = false;
  }

  public void Restart() {
    foreach (AudioChip audioChip in audioChips) {
      audioChip.StartChip();
    }
    isPlaying = true;
    isPaused = false;
  }
  
  // public void GetProgress()
  public float GetProgress() {
    return audioChips[0].GetPlaybackProgress();
  }
  

  // Update is called once per frame
  void Update() { }
}