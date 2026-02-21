using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AudioChip : MonoBehaviour {
  [SerializeField]
  private AudioSource audioSource;

  [SerializeField]
  private Image trackIcon;

  private static int samplePowerOf2 = 7;

  private static int sampleCount = (int)Math.Pow(2, samplePowerOf2);

  private float[] samplesL = new float[sampleCount];
  private float[] samplesR = new float[sampleCount];
  private float rmsValue;

  private float dbValue;

  // private static int QSamples = sampleCount;
  private const float RefValue = 0.1f;


  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start() {
    transform.position = Random.insideUnitCircle * 2;
  }

  public void Initialize(AudioClip clip, Sprite icon) {
    audioSource.clip = clip;
    trackIcon.sprite = icon;
  }

  public void StartChip() {
    audioSource.Play();
  }

  public void PauseChip() {
    audioSource.Pause();
  }
  
  public void ResumeChip() {
    audioSource.UnPause();
  }

  public void StopChip() {
    audioSource.Stop();
  }

  public float GetPlaybackProgress() {
    return (float)audioSource.timeSamples / (float)audioSource.clip.samples;
  }
  
  private void AnalyzeAudio() {
    audioSource.GetOutputData(samplesL, 0);
    audioSource.GetOutputData(samplesR, 1);
    float sum = 0;
    for (int i = 0; i < sampleCount; i++) {
      sum += samplesL[i] * samplesR[i];
    }

    rmsValue = Mathf.Sqrt(sum / sampleCount);

    dbValue = 20 * Mathf.Log10(rmsValue / RefValue);

    if (dbValue < -160) dbValue = -160;
  }

  // Update is called once per frame
  void Update() {
    AnalyzeAudio();
    Debug.Log(rmsValue * 100);
    transform.localScale = new Vector3(1 + (rmsValue * 10), 1 + (rmsValue*10), 1);
  }

  private void OnMouseDrag() {
    Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    newPos.z = 0; // Set z o zero so not spawning in the camera
    transform.position = newPos;
  }
}