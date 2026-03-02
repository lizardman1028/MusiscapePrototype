using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class GuiController : MonoBehaviour {
  public static GuiController instance;

  public static float floorScale = 56;
  
  [SerializeField]
  private AudioChipManager singleTrack;
  [SerializeField]
  private AudioChipManager multiTrack;
  
  private AudioChipManager currentAudioChips;
  public AudioChipManager CurrentAudioChipManager => currentAudioChips;
  
  private bool singleTrackMode = true;
  private bool multiTrackMode = false;

  private void Awake() {
    if (instance == null) {
      instance = this;
    }
  }
  
  private void Start() {
    currentAudioChips = multiTrack;
  }
}