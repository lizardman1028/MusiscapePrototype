using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TrackVolumeSlider : MonoBehaviour
{
    [SerializeField]
    private Image trackIcon;

    [SerializeField]
    private Image iconGlass;

    [SerializeField]
    private Image sliderGlass;
    
    [SerializeField]
    private SliderHandle sliderHandle;
    
    private AudioChip audioChip;

    public void Setup(AudioChip audioChip) {
      this.audioChip = audioChip;
      sliderHandle.onSliderDrag = new UnityEvent();
      
      audioChip.PositionChanged.AddListener(SetSliderOffOfChipPos);
      audioChip.PositionChanged.AddListener(SetSliderGlass);
      
      sliderHandle.onSliderDrag.AddListener(SetChipOffSliderPos);
      sliderHandle.onSliderDrag.AddListener(SetSliderGlass);
      
      sliderHandle.SetSliderValue(audioChip.VolumeDistanceScalar);
      SetSliderGlass();
    }

    private void Update() {
      trackIcon.sprite = audioChip.trackIcon.sprite;
      iconGlass.color = audioChip.glass.color;
      sliderGlass.color = audioChip.glass.color;
    }

    private void SetSliderOffOfChipPos() {
      sliderHandle.SetSliderValue(audioChip.VolumeDistanceScalar);
    }

    private void SetChipOffSliderPos() {
      Debug.Log(sliderHandle.GetSliderValue());
      audioChip.SetVolumeDistance(sliderHandle.GetSliderValue());
    }

    private void SetSliderGlass() {
      sliderGlass.rectTransform.offsetMax = new Vector2(sliderHandle.myTransform.anchoredPosition.x, sliderGlass.rectTransform.offsetMax.y);
      // progressBarRect.offsetMax = new Vector2(-progressBarHorizontalOffset, progressBarRect.offsetMax.y);
    }
}
