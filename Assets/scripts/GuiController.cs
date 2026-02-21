using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class GuiController : MonoBehaviour {
  [SerializeField]
  private AudioChipManager singleTrack;
  [SerializeField]
  private AudioChipManager multiTrack;
  
  private AudioChipManager currentAudioChips;
  
  [SerializeField]
  private Button PlayPauseButton;
  [SerializeField]
  private TextMeshProUGUI PlayPauseText;
  
  // [SerializeField]
  private Sprite PlayButton;
  // [SerializeField]
  private Sprite PauseButton;
  
  [SerializeField]
  private String PlayButtonText = "Play";
  [SerializeField]
  private String PauseButtonText = "Pause";

  [SerializeField]
  private Button RestartButton;

  [SerializeField]
  private RectTransform progressBar;
  [SerializeField]
  private RectTransform progressBarBacking;

  private bool singleTrackMode = true;
  private bool multiTrackMode = false;

  private void Start() {
    currentAudioChips = multiTrack;
    PlayPauseButton.onClick.AddListener(PauseButtonPressed);
  }

  private void Update() {
    SetPauseButtonImage();
    SetProgressBar();
  }

  private void SetProgressBar() {
    // progressBar.sizeDelta = new Vector2(progressBarBacking.sizeDelta.x * currentAudioChips.GetProgress(), progressBarBacking.sizeDelta.y);
    progressBar.transform.localScale = new Vector3(currentAudioChips.GetProgress(), 1f, 1f);
  }
  
  private void PauseButtonPressed() {
    if (currentAudioChips.IsPlaying && !currentAudioChips.IsPaused) {
      currentAudioChips.Pause();
      return;
    }

    if (!currentAudioChips.IsPlaying && currentAudioChips.IsPaused) {
      currentAudioChips.Resume();
      return;
    }

    if (!currentAudioChips.IsPlaying && !currentAudioChips.IsPaused) {
      currentAudioChips.Restart();
      return;
    }
  }

  private void SetPauseButtonImage() {
    if (currentAudioChips.IsPlaying) {
      PlayPauseText.text = PauseButtonText;
      return;
    }
    PlayPauseText.text = PlayButtonText;
  }
  
  
}