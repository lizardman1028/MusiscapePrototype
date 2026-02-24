using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarGuiController : MonoBehaviour
{
  [SerializeField]
  private RectTransform progressBarRect;
  
  [SerializeField]
  private RectTransform progressBarBacking;
  
  private float progressBarHorizontalOffset;

  [SerializeField]
  private float progressBarBorderWidth = 19; 
  private float progressBarRightEmpty;
  private float progressBarRightFull;

  private void Start() {
    progressBarHorizontalOffset = progressBarRect.offsetMin[0];
    progressBarRightEmpty = progressBarBacking.rect.width - (progressBarHorizontalOffset) - (2*progressBarBorderWidth);
    progressBarRightFull = progressBarHorizontalOffset;
    // Set Left Edge
    progressBarRect.offsetMin = new Vector2(progressBarHorizontalOffset, progressBarRect.offsetMin.y);
    // Set Right Edge
    progressBarRect.offsetMax = new Vector2(-progressBarHorizontalOffset, progressBarRect.offsetMax.y);
  }

  private void SetProgress(float value) {
    // Set Left Edge
    progressBarRect.offsetMin = new Vector2(progressBarHorizontalOffset, progressBarRect.offsetMin.y);
    
    float newRightOffset = progressBarRightEmpty * (1 - value) + progressBarRightFull * (value);
    // Set Right Edge
    progressBarRect.offsetMax = new Vector2(-newRightOffset, progressBarRect.offsetMax.y);
  }

  private void Update() {
    SetProgress(GuiController.instance.CurrentAudioChipManager.GetProgress());
  }
}
