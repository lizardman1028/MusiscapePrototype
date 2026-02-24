using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class LyricsParser
{
    public static List<ParsedLyricLine> Parse(string rawLyrics)
    {
        List<ParsedLyricLine> parsed = new List<ParsedLyricLine>();

        string[] lines = rawLyrics.Split('\n');

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (Regex.IsMatch(line, @"^\[.*\]$"))
            {
                parsed.Add(new ParsedLyricLine
                {
                    type = LyricType.Header,
                    text = line.ToUpper()
                });
            }
            else
            {
                parsed.Add(new ParsedLyricLine
                {
                    type = LyricType.Line,
                    text = line
                });
            }
        }

        return parsed;
    }
}