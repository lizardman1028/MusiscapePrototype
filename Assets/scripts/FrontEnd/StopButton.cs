using System;
using UnityEngine;

public class StopButton : MonoBehaviour {
  [SerializeField]
  private GlassButton glassButton;

  private Color glassColor = Color.red;
  
  private AudioChipManager audioChipManager => GuiController.instance.CurrentAudioChipManager;
  
  private void Start() {
    glassButton.OnClick.AddListener(StopButtonPressed);
    glassButton.SetGlassColor(glassColor);
  }

  private void StopButtonPressed() {
    PlayButtonGuiController.instance.ButtonToPlayState();
    audioChipManager.Stop();
  }
}