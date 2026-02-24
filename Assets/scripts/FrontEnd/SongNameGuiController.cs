using UnityEngine;

public class SongNameGuiController : MonoBehaviour {
  [SerializeField]
  private GlassButton glassButton;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start() {
  }

  // Update is called once per frame
  void Update() {
    glassButton.SetText(GuiController.instance.CurrentAudioChipManager.CurrentSongConfig.songName);
  }
}