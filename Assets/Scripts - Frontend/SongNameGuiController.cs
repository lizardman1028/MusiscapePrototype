using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SongSelectionManager : MonoBehaviour {
  public static SongSelectionManager instance;
  
  [SerializeField]
  private GlassButton nameButton;
  
  [SerializeField]
  private RectTransform songListContainer;
  
  [SerializeField]
  private List<SongConfig> songConfigs;
  
  [SerializeField]
  private GameObject songSwitchButtonPrefab;
  
  [SerializeField]
  private Image upArrow;
  
  [SerializeField]
  private Image rightArrow;
  
  private List<SongSwitchButton> songSwitchButtons;
  
  private AudioChipManager audioChipManager => GuiController.instance.CurrentAudioChipManager;
  
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start() {
    if (instance == null) {
      instance = this;
    }
    
    songSwitchButtons = new List<SongSwitchButton>();
    
    nameButton.OnClick.AddListener(ToggleSongList);
    
    foreach (SongConfig songConfig in songConfigs) {
      GameObject buttonGameObject =  Instantiate(songSwitchButtonPrefab, songListContainer);
      SongSwitchButton songSwitchButton = buttonGameObject.GetComponent<SongSwitchButton>();
      
      if (songSwitchButton == null) {
        Debug.LogError("Prefab Improperly Set");
        return;
      }
      
      songSwitchButton.Setup(songConfig);
      songSwitchButtons.Add(songSwitchButton);
    }
    
    HideList();
    
    // audioChipManager.SetSongConfig(songConfigs[0]);
  }

  // Update is called once per frame
  void Update() {
    nameButton.SetText(GuiController.instance.CurrentAudioChipManager.CurrentSongConfig.songName);
  }

  private void ToggleSongList() {
    if (songListContainer.gameObject.activeSelf) {
      HideList();
      return;
    }

    ShowList();
  }

  public void ShowList() {
    songListContainer.gameObject.SetActive(true);
    upArrow.color = Color.white;
    rightArrow.color = Color.clear;
  }
  
  public void HideList() {
    songListContainer.gameObject.SetActive(false);
    upArrow.color = Color.clear;
    rightArrow.color = Color.white;
  }
}