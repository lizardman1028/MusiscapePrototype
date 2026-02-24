using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq; // we’ll install this next

public class GeniusAPI : MonoBehaviour
{
    public string accessToken = "YOUR_GENIUS_ACCESS_TOKEN";
    public SpotifyLyricsController lyricsController;

    public void FetchLyrics(string songQuery)
    {
        StartCoroutine(SearchSong(songQuery));
    }

    IEnumerator SearchSong(string query)
    {
        string url = "https://api.genius.com/search?q=" + UnityWebRequest.EscapeURL(query);

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        JObject json = JObject.Parse(request.downloadHandler.text);

        string songUrl = json["response"]["hits"][0]["result"]["url"].ToString();

        StartCoroutine(GetLyricsFromPage(songUrl));
    }

    IEnumerator GetLyricsFromPage(string songUrl)
    {
        UnityWebRequest request = UnityWebRequest.Get(songUrl);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
            yield break;
        }

        string html = request.downloadHandler.text;

        string lyrics = ExtractLyricsFromHtml(html);

        lyricsController.LoadLyrics(lyrics);
    }

    string ExtractLyricsFromHtml(string html)
    {
        // Basic extraction method
        int start = html.IndexOf("Lyrics__Container");
        if (start < 0) return "Lyrics not found.";

        string[] split = html.Split(new string[] { "<br/>" }, System.StringSplitOptions.None);

        string combined = "";

        foreach (string line in split)
        {
            if (line.Contains(">") && line.Contains("<"))
            {
                int s = line.IndexOf(">") + 1;
                int e = line.IndexOf("<", s);
                if (e > s)
                {
                    combined += line.Substring(s, e - s) + "\n";
                }
            }
        }

        return combined;
    }
}