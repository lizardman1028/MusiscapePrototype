using System;
using UnityEngine;

public class DancingSpeedChanger : MonoBehaviour {
  [SerializeField]
  Animator animator;
  
  [SerializeField]
  AudioListener listener;

  [SerializeField]
  private float rmsSpeedMultiplier;

  private void Update() {
    CalculateSpeed();
  }

  private void CalculateSpeed() {
    float rmsSum = GuiController.instance.CurrentAudioChipManager.GetRMSSum();
    rmsSum = rmsSum * rmsSpeedMultiplier;
    if (float.IsNaN(rmsSum)) {rmsSum = 0;}
    // if (rmsSum <= 0) {rmsSum = 0.1f;}
    // if (rmsSum > 1) {rmsSum = 1;}
    animator.SetFloat("Speed", rmsSum);
  }
}