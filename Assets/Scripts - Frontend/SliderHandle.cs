using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SliderHandle : MonoBehaviour, IDragHandler {
  private float zDepth = 15f;
  [SerializeField]
  public RectTransform myTransform;

  private static float leftBound = -200f;
  private static float rightBound = -10f;
  
  [HideInInspector]
  public UnityEvent onSliderDrag;

  public void Start() {
    // onSliderDrag = new UnityEvent();
  }

  public float GetSliderValue() {
    float sliderWidth = (leftBound - rightBound);
    return (sliderWidth - (myTransform.anchoredPosition.x - rightBound)) / sliderWidth;
  }

  public void SetSliderValue(float value) {
    float sliderWidth = (leftBound - rightBound);
    value *= sliderWidth;
    value -= sliderWidth;
    value *= -1;
    value += rightBound;
    myTransform.anchoredPosition = new Vector2(value, myTransform.anchoredPosition.y);
    
    if (myTransform.anchoredPosition.x > rightBound) {
      myTransform.anchoredPosition = new Vector2(rightBound, myTransform.anchoredPosition.y);
    }

    if (myTransform.anchoredPosition.x < leftBound) {
      myTransform.anchoredPosition = new Vector2(leftBound, myTransform.anchoredPosition.y);
    }
  }

  public void OnDrag(PointerEventData eventData) {
    Vector3 inputMousePos = Input.mousePosition;
    inputMousePos.z = zDepth;
    Vector3 newPos = Camera.main.ScreenToWorldPoint(inputMousePos);
    Vector3 newLocalPos = myTransform.InverseTransformPoint(newPos);
    
    newPos.z = inputMousePos.z + Camera.main.transform.position.z;
    newPos.y = transform.position.y;
    myTransform.position = newPos;
    
    if (myTransform.anchoredPosition.x > rightBound) {
      myTransform.anchoredPosition = new Vector2(rightBound, myTransform.anchoredPosition.y);
    }

    if (myTransform.anchoredPosition.x < leftBound) {
      myTransform.anchoredPosition = new Vector2(leftBound, myTransform.anchoredPosition.y);
    }
    
    onSliderDrag.Invoke();
  }
  
  
}