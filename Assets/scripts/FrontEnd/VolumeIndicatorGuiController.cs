using UnityEngine;
using UnityEngine.UI;

public class VolumeIndicatorGuiController : MonoBehaviour {
  [SerializeField]
  private Image volumeIndicator;
  
  void Update() {
    if (GuiController.instance.CurrentAudioChipManager.ChipBeingDragged()) {
      volumeIndicator.color = Color.white;
      return;
    }
    volumeIndicator.color = Color.clear;
  }
}