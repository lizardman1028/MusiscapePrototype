using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlassButton : MonoBehaviour {
  [Header("UI Elements")]
  [SerializeField]
  private Button button;

  [SerializeField]
  private TextMeshProUGUI buttonText;

  [SerializeField]
  private Image buttonImage;

  [SerializeField]
  private Image glassImage;

  public bool GlassOn => glassImage.color.a > 0f;

  public Button.ButtonClickedEvent OnClick => button.onClick;

  public void SetGlassColor(Color color) {
    glassImage.color = new Color(color.r, color.g, color.b, glassImage.color.a);
  }

  public void SetText(string text) {
    buttonText.text = text;
  }

  public void SetTextColor(Color color) {
    buttonText.color = color;
  }

  public void SetGlass(bool on) {
    Color color = glassImage.color;
    color.a = on ? 1f : 0f;
    glassImage.color = color;
  }
}