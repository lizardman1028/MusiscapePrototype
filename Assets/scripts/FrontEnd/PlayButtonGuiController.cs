using System;
using UnityEngine;

public class PlayButtonGuiController : MonoBehaviour {
  public static PlayButtonGuiController instance;
  [SerializeField]
  private GlassButton glassButton;

  private Color playGlassColor = Color.green;
  private Color pauseGlassColor = Color.yellow;

  private static string pauseText = "Pause";
  private static string playText = "Play";

  private AudioChipManager audioChipManager => GuiController.instance.CurrentAudioChipManager;

  private void Start() {
    if (instance == null) {
      instance = this;
    }
    glassButton.OnClick.AddListener(ButtonPressed);
    glassButton.SetText(playText);
    glassButton.SetGlass(false);
    ButtonToPlayState();
  }

  private void ButtonPressed() {
    // Pause clicked -> pause chip + set button to play
    if (audioChipManager.IsPlaying && !audioChipManager.IsPaused) {
      ButtonToPlayState();
      audioChipManager.Pause();
      return;
    }

    // Play clicked -> resume chips + set button to pause
    if (!audioChipManager.IsPlaying && audioChipManager.IsPaused) {
      ButtonToPauseState();
      audioChipManager.Resume();
      return;
    }

    // Play clicked (song not started) -> start song + set button to pause
    if (!audioChipManager.IsPlaying && !audioChipManager.IsPaused) {
      ButtonToPauseState();
      audioChipManager.Restart();
      return;
    }
  }

  // Set button so clicking -> pause
  private void ButtonToPauseState() {
    glassButton.SetGlass(true);
    glassButton.SetText(pauseText);
    glassButton.SetGlassColor(pauseGlassColor);
  }

  // Set button so clicking -> play
  public void ButtonToPlayState() {
    glassButton.SetGlass(true);
    glassButton.SetText(playText);
    glassButton.SetGlassColor(playGlassColor);
  }
}