public enum LyricType
{
    Header,
    Line
}

[System.Serializable]
public class ParsedLyricLine
{
    public LyricType type;
    public string text;
}