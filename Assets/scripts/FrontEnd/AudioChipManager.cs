using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioChipManager : MonoBehaviour {
  [SerializeField]
  private GameObject audioChipPrefab;

  [SerializeField]
  private SongConfig songConfig;

  public SongConfig CurrentSongConfig => songConfig;

  private List<AudioChip> audioChips;
  
  [SerializeField]
  private Transform floor;

  private bool isPlaying;
  private bool isPaused;

  public bool IsPlaying => isPlaying;
  public bool IsPaused => isPaused;

  public void SetSongConfig(SongConfig config) {
    if (config == songConfig) {
      return;
    }
    songConfig = config;
    StartMultiTrack();
  }

  void Start() {
    audioChips = new List<AudioChip>();
    StartMultiTrack();
  }

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void StartMultiTrack() {
    Stop();
    foreach (AudioChip audioChip in audioChips) {
      Destroy(audioChip.gameObject);
    }
    audioChips = new List<AudioChip>();
    for (int i = 0; i < songConfig.clips.Count; i++) {
      GameObject audioChip = Instantiate(audioChipPrefab, floor);
      audioChip.transform.localPosition = new Vector3(-2.5f + i, 0, 2);
      audioChip.GetComponent<AudioChip>().Initialize(songConfig.clips[i], songConfig.sprites[i], songConfig.trackNames[i]);
      audioChips.Add(audioChip.GetComponent<AudioChip>());
    }
    // Restart();
  }

  public float GetRMSSum() {
    float sum = 0;
    foreach (AudioChip audioChip in audioChips) {
      sum += audioChip.RMSValueOutput;
    }
    return sum;
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
    if (audioChips == null || audioChips.Count <= 0) {
      return 0;
    }

    return audioChips[0].GetPlaybackProgress();
  }

  public bool ChipBeingDragged() {
    bool dragged = false;
    foreach (AudioChip audioChip in audioChips) {
      dragged |= audioChip.BeingDragged;
    }
    return dragged;
  }

  // Update is called once per frame
  void Update() { }
}