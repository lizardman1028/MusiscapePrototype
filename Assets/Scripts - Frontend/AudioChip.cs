using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AudioChip : MonoBehaviour {
  [SerializeField]
  private AudioSource audioSource;

  [SerializeField]
  public Image trackIcon;

  [SerializeField]
  public Image glass;

  [SerializeField]
  private Image volumeIndicator;

  [SerializeField]
  private Image nameBacking;

  [SerializeField]
  private Image nameGlass;

  [SerializeField]
  private TMPro.TextMeshProUGUI nameText;

  [SerializeField]
  private SpriteRenderer worldIcon;

  [SerializeField]
  private SpriteRenderer worldGlass;
  
  [SerializeField]
  private SpriteRenderer worldIcon2;

  [SerializeField]
  private SpriteRenderer worldGlass2;
  
  [SerializeField]
  private SpriteRenderer worldVolumeIndicator;

  [SerializeField]
  private Light worldVolumeLight;

  private bool beingDragged = false;
  public bool BeingDragged => beingDragged;

  [HideInInspector]
  public UnityEvent PositionChanged;

  private float volumeDistanceScalar;
  public float VolumeDistanceScalar => volumeDistanceScalar;

  private Color glassColor = Color.green;

  // Used to compute rms and db
  private static int samplePowerOf2 = 7;
  private static int sampleCount = (int)Math.Pow(2, samplePowerOf2);
  private float[] samples = new float[sampleCount];
  private float[] samplesL = new float[sampleCount];
  private float[] samplesR = new float[sampleCount];
  private float[] samplesFreqL = new float[sampleCount*4];
  private float[] samplesFreqR = new float[sampleCount*4];
  private float rmsValue;
  private float dbValue;
  private float rmsValueOutput;
  public float RMSValueOutput => rmsValueOutput;
  private const float RefValue = 0.1f;

  // used for calculation of audiochip movement with mouse
  private static float zDepth = 15;

  public void Initialize(AudioClip clip, Sprite icon, string name) {
    PositionChanged = new UnityEvent();
    audioSource.clip = clip;
    trackIcon.sprite = icon;
    worldIcon.sprite = icon;
    worldIcon2.sprite = icon;
    nameText.text = name;
    UpdateVolumeDistance();
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

  public void ScrubChip(float value) {
    audioSource.timeSamples = Mathf.RoundToInt(value * (float)audioSource.clip.samples);
  }

  public float GetPlaybackProgress() {
    return (float)audioSource.timeSamples / (float)audioSource.clip.samples;
  }

  private void AnalyzeGain() {
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
    audioSource.GetSpectrumData(samplesFreqL, 0, FFTWindow.Rectangular);
    audioSource.GetSpectrumData(samplesFreqR, 0, FFTWindow.Rectangular);
    float numerator = 0;
    float sumTot = 0;
    for (int i = 0; i < samplesFreqL.Length; i++) {
      numerator += Mathf.Sqrt(samplesFreqL[i] * i + samplesFreqR[i] * i);
      sumTot += samplesFreqL[i] + samplesFreqR[i];
    }
    
    // Debug.Log(numerator / sumTot);
    
    float maxFreq = samplesFreqL.Max();
    float maxFreqR = samplesFreqR.Max();
    
    int maxFreqIndex = Array.IndexOf(samplesFreqL, maxFreq);
    int maxFreqIndexR = Array.IndexOf(samplesFreqR, maxFreq);
    float freqAsFloat = ((float)maxFreqIndex * 0.8f) / (float)samplesFreqL.Length;
    float freqAsFloatR = ((float)maxFreqIndex * 0.8f) / (float)samplesFreqR.Length;
    // freqAsFloat = Mathf.Sqrt(freqAsFloat);
    // freqAsFloatR = Mathf.Sqrt(freqAsFloatR);
    freqAsFloat = (freqAsFloat + freqAsFloatR) / 2f;
    // freqAsFloat *= 5;
    // freqAsFloat %= 1;
    float actualNum = Mathf.Sqrt(numerator / sumTot)/50;
    // Debug.Log(actualNum);
    glassColor = Color.HSVToRGB(actualNum % 1, 0.7f, 0.7f);
    if (actualNum > 10) {
      glassColor = Color.white;
    }
  }
  
  private void UpdateVolumeDistance() {
    AudioListener playerListener = FindFirstObjectByType<AudioListener>();
    if (playerListener == null) {
      volumeDistanceScalar = -1;
    }
    Transform player = playerListener.transform;
    Vector3 displacement = transform.localPosition - player.localPosition;
    Vector2 displacement2D = new Vector2(displacement.x, displacement.z);
    float distance = displacement2D.magnitude;
    distance = (audioSource.maxDistance - distance) / audioSource.maxDistance;
    if (distance < 0) {
      volumeDistanceScalar = 0;
    }
    volumeDistanceScalar = distance;
  }

  // To be called when the slider changes values
  public void SetVolumeDistance(float distanceScalar) {
    // Add a little bit of buffer so we don't lose information when chips are located directly on the player
    distanceScalar = (1 - distanceScalar) + 0.01f;
    AudioListener playerListener = FindFirstObjectByType<AudioListener>();
    if (playerListener == null) {
      return;
    }
    Transform player = playerListener.transform;
    Vector3 displacement = transform.localPosition - player.localPosition;
    Vector2 displacement2D = new Vector2(displacement.x, displacement.z);
    Vector2 displacementDirection =  displacement2D.normalized;
    float newDistance = distanceScalar * (audioSource.maxDistance + 1f);
    displacement2D = displacementDirection * newDistance;
    Vector3 newPosition = new Vector3(displacement2D.x, transform.localPosition.y, displacement2D.y) + player.localPosition;
    transform.localPosition = newPosition;
  }
  
  // Update is called once per frame
  void Update() {
    AnalyzeGain();
    AnalyzeFrequency();
    UpdateVolumeDistance();
    // if (Input.GetKeyDown(KeyCode.Space)) {
    //   SetVolumeDistance(1f);
    // }
    // Debug.Log(rmsValue * 100);
    volumeIndicator.transform.localScale = new Vector3(1f + (rmsValue * 2), 1f + (rmsValue * 2), 1);
    worldVolumeIndicator.transform.localScale = new Vector3(0.95f + (rmsValue * 2), 0.95f + (rmsValue * 2), 0.95f);
    worldVolumeLight.intensity = 1.3f + (rmsValue * 4);
    // glassColor = Color.Lerp(Color.green, Color.red, rmsValue * 2);
    if (!audioSource.isPlaying || glassColor == Color.white) {
      glassColor = Color.white;
      worldVolumeLight.intensity = 0;
    }
    volumeIndicator.color = glassColor;
    worldVolumeIndicator.color = glassColor;
    worldVolumeLight.color = glassColor;
    glassColor.a = 0.7f;
    glass.color = glassColor;
    worldGlass.color = glassColor;
    worldGlass2.color = glassColor;
    nameBacking.gameObject.SetActive(beingDragged);
    // nameBacking.color = new Color(nameBacking.color.r, nameBacking.color.g, nameBacking.color.b, volumeDistanceScalar);
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
    PositionChanged.Invoke();
  }

  private void OnMouseUp() {
    beingDragged = false;
  }
}