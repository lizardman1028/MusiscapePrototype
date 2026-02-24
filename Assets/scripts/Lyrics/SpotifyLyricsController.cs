using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpotifyLyricsController : MonoBehaviour
{
    public AudioSource audioSource;

    public ScrollRect scrollRect;
    public RectTransform content;

    public GameObject linePrefab;
    public GameObject headerPrefab;

    private List<TextMeshProUGUI> lyricUI = new List<TextMeshProUGUI>();
    private int currentIndex = -1;

    public void LoadLyrics(string rawLyrics)
    {
        ClearLyrics();

        var parsed = LyricsParser.Parse(rawLyrics);

        foreach (var item in parsed)
        {
            GameObject prefab = item.type == LyricType.Header
                ? headerPrefab
                : linePrefab;

            GameObject obj = Instantiate(prefab, content);
            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = item.text;

            if (item.type == LyricType.Line)
                tmp.color = new Color(1,1,1,0.4f);

            lyricUI.Add(tmp);
        }
    }

    void Update()
    {
        if (audioSource == null || lyricUI.Count == 0)
            return;

        AutoScroll();
    }

    void AutoScroll()
    {
        float normalizedTime = audioSource.time / audioSource.clip.length;
        int targetIndex = Mathf.FloorToInt(normalizedTime * lyricUI.Count);

        if (targetIndex != currentIndex && targetIndex >= 0 && targetIndex < lyricUI.Count)
        {
            currentIndex = targetIndex;
            HighlightLine(targetIndex);
            CenterLine(targetIndex);
        }
    }

    void HighlightLine(int index)
    {
        for (int i = 0; i < lyricUI.Count; i++)
        {
            if (i == index)
            {
                lyricUI[i].color = Color.white;
                lyricUI[i].fontSize = 42;
            }
            else
            {
                lyricUI[i].color = new Color(1,1,1,0.35f);
                lyricUI[i].fontSize = 36;
            }
        }
    }

    void CenterLine(int index)
    {
        StopAllCoroutines();
        StartCoroutine(SmoothScroll(index));
    }

    IEnumerator SmoothScroll(int index)
    {
        Canvas.ForceUpdateCanvases();

        RectTransform target = lyricUI[index].rectTransform;

        float contentHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        float targetY = Mathf.Abs(target.anchoredPosition.y);
        float normalizedPos = 1 - (targetY / (contentHeight - viewportHeight));
        normalizedPos = Mathf.Clamp01(normalizedPos);

        float duration = 0.35f;
        float time = 0f;
        float start = scrollRect.verticalNormalizedPosition;

        while (time < duration)
        {
            time += Time.deltaTime;
            scrollRect.verticalNormalizedPosition =
                Mathf.Lerp(start, normalizedPos, time / duration);
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = normalizedPos;
    }

    void ClearLyrics()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        lyricUI.Clear();
    }
}