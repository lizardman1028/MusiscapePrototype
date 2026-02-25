using System;
using UnityEngine;

public class SongSwitchButton : MonoBehaviour {
  [SerializeField]
  private GlassButton glassButton;
  [SerializeField]
  private SongConfig songConfig;
  
  private Color selectedColor = Color.green;
  private Color unselectedColor = Color.white;

  private void Start() {
    glassButton.OnClick.AddListener(SwapSong);
  }

  public void Setup(SongConfig config) {
    songConfig = config;
    glassButton.SetText(songConfig.songName);
    glassButton.OnClick.AddListener(SwapSong);
  }

  void SwapSong() {
    GuiController.instance.CurrentAudioChipManager.SetSongConfig(songConfig);
    PlayButtonGuiController playButton = FindFirstObjectByType<PlayButtonGuiController>();
    if (playButton != null) {
      playButton.ButtonToPlayState();
    }
    SongSelectionManager.instance.HideList();
  }

  void Update() {
    Color glassColor = GuiController.instance.CurrentAudioChipManager.CurrentSongConfig == songConfig 
      ? selectedColor : unselectedColor;
    glassButton.SetGlassColor(glassColor);
  }
}