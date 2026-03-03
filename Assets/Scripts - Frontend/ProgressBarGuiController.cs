using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProgressBarGuiController : MonoBehaviour, IDragHandler, IPointerClickHandler
{
  [SerializeField]
  private RectTransform progressBarRect;
  
  [SerializeField]
  private RectTransform progressBarBacking;
  
  private float progressBarHorizontalOffset;

  private float zDepth = 15;

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

  public void OnDrag(PointerEventData eventData) {
    Vector3 inputMousePos = Input.mousePosition;
    
    // progressBarRect.anchoredPosition = new Vector2(,  progressBarRect.anchoredPosition.y);
    float newProgress = (inputMousePos.x+20-(3*progressBarBorderWidth)) / (Screen.width-(3*progressBarBorderWidth));
    if (newProgress < 0) {
      newProgress = 0;
    }

    if (newProgress > 1) {
      newProgress = 1;
    }
    
    GuiController.instance.CurrentAudioChipManager.Scrub(newProgress);
  }

  public void OnPointerClick(PointerEventData eventData) {
    OnDrag(eventData);
  }
}
