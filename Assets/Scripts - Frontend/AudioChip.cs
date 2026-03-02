using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AudioChip : MonoBehaviour {
  [SerializeField]
  private AudioSource audioSource;

  [SerializeField]
  private Image trackIcon;

  [SerializeField]
  private Image glass;

  [SerializeField]
  private Image volumeIndicator;

  [SerializeField]
  private Image nameBacking;

  [SerializeField]
  private Image nameGlass;

  [SerializeField]
  private TMPro.TextMeshProUGUI nameText;


  private bool beingDragged = false;
  public bool BeingDragged => beingDragged;

  private Color glassColor = Color.green;

  // Used to compute rms and db
  private static int samplePowerOf2 = 7;
  private static int sampleCount = (int)Math.Pow(2, samplePowerOf2);
  private float[] samples = new float[sampleCount];
  private float[] samplesL = new float[sampleCount];
  private float[] samplesR = new float[sampleCount];
  private float rmsValue;
  private float dbValue;
  private float rmsValueOutput;
  public float RMSValueOutput => rmsValueOutput;
  private const float RefValue = 0.1f;

  // used for calculation of audiochip movement with mouse
  private static float zDepth = 15;

  public void Initialize(AudioClip clip, Sprite icon, string name) {
    audioSource.clip = clip;
    trackIcon.sprite = icon;
    nameText.text = name;
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

  private void AnalyzeVolume() {
    audioSource.GetOutputData(samplesL, 0);
    audioSource.GetOutputData(samplesR, 1);

    audioSource.clip.GetData(samples, audioSource.timeSamples);

    float sum = 0;
    float sumOutput = 0;
    for (int i = 0; i < sampleCount; i++) {
      sum += samples[i] * samples[i];
      sumOutput += samplesL[i] * samplesR[i];
    }

    rmsValue = Mathf.Sqrt(sum / sampleCount);
    rmsValueOutput = Mathf.Sqrt(sumOutput/sampleCount);

    dbValue = 20 * Mathf.Log10(rmsValue / RefValue);

    if (dbValue < -160) dbValue = -160;
  }

  private void AnalyzeFrequency() {
    // audioSource.GetSpectrumData()
    audioSource.GetSpectrumData(samplesL, 0, FFTWindow.Rectangular);
    float maxFreq = samplesL.Max();
    int maxFreqIndex = Array.IndexOf(samplesL, maxFreq);
    float freqAsFloat = ((float)maxFreqIndex * 0.8f) / (float)samplesL.Length;
    freqAsFloat = Mathf.Sqrt(freqAsFloat);
    glassColor = Color.HSVToRGB(freqAsFloat, 0.7f, 0.7f);
  }

  // Update is called once per frame
  void Update() {
    AnalyzeVolume();
    AnalyzeFrequency();
    // Debug.Log(rmsValue * 100);
    volumeIndicator.transform.localScale = new Vector3(1f + (rmsValue * 2), 1f + (rmsValue * 2), 1);
    // glassColor = Color.Lerp(Color.green, Color.red, rmsValue * 2);
    volumeIndicator.color = glassColor;
    glassColor.a = 0.6f;
    glass.color = glassColor;
    nameBacking.gameObject.SetActive(beingDragged);
    if (nameGlass.enabled) {
      nameGlass.color = glassColor;
    }
  }

  private void OnMouseDrag() {
    Vector3 inputMousePos = Input.mousePosition;
    inputMousePos.z = zDepth;
    Vector3 newPos = Camera.main.ScreenToWorldPoint(inputMousePos);
    Vector3 newLocalPos = transform.InverseTransformPoint(newPos);
    // newLocalPos.y = 0; // Set z o zero so not spawning in the camera
    // newLocalPos = newLocalPos / (GuiController.floorScale * 2);
    newPos.z = inputMousePos.z + Camera.main.transform.position.z;
    transform.position = newPos;
    // newPos.y = 0;
    // transform.position = newPos;
    // transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
    // Debug.Log(newPos);
    nameBacking.gameObject.SetActive(true);
    beingDragged = true;
  }

  private void OnMouseUp() {
    beingDragged = false;
  }
}