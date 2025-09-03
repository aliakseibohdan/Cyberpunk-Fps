using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class TerminalParser
{
    private static readonly Regex TagRegex = new(@"^<(\w+)=([^>]+)>$", RegexOptions.Compiled);

    private static readonly Dictionary<string, Color> ColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        {"black", Color.black},
        {"white", Color.white},
        {"red", Color.red},
        {"green", Color.green},
        {"blue", Color.blue},
        {"yellow", Color.yellow},
        {"cyan", Color.cyan},
        {"magenta", Color.magenta},
        {"gray", Color.gray}
    };

    public static List<ParsedTerminalLine> ParseTerminalScript(string scriptText, float defaultDelay, Color defaultColor)
    {
        var parsedLines = new List<ParsedTerminalLine>();
        var lines = scriptText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        float currentDelay = defaultDelay;
        Color currentColor = defaultColor;
        AudioClip currentSound = null;
        bool skipNextLine = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrEmpty(trimmedLine))
                continue;

            var match = TagRegex.Match(trimmedLine);
            if (match.Success)
            {
                string tagName = match.Groups[1].Value.ToLower();
                string tagValue = match.Groups[2].Value;

                switch (tagName)
                {
                    case "delay":
                        if (float.TryParse(tagValue, out float delayValue))
                            currentDelay = delayValue;
                        break;

                    case "color":
                        if (ColorMap.TryGetValue(tagValue, out Color mappedColor))
                        {
                            currentColor = mappedColor;
                        }
                        else if (ColorUtility.TryParseHtmlString(tagValue, out Color parsedColor))
                        {
                            currentColor = parsedColor;
                        }
                        break;

                    case "sound":
                        currentSound = Resources.Load<AudioClip>($"Sounds/{tagValue}");
                        break;

                    case "skip":
                        if (int.TryParse(tagValue, out int skipCount))
                        {
                            skipNextLine = skipCount > 0;
                        }
                        break;

                    case "clear":
                        parsedLines.Clear();
                        break;

                    case "reset":
                        currentDelay = defaultDelay;
                        currentColor = defaultColor;
                        currentSound = null;
                        break;
                }

                continue;
            }

            if (skipNextLine)
            {
                skipNextLine = false;
                continue;
            }

            var parsedLine = new ParsedTerminalLine
            {
                Text = trimmedLine,
                Delay = currentDelay,
                Color = currentColor,
                Sound = currentSound
            };

            parsedLines.Add(parsedLine);

            currentDelay = defaultDelay;
            currentColor = defaultColor;
            currentSound = null;
        }

        return parsedLines;
    }
}

public class ParsedTerminalLine
{
    public string Text;
    public float Delay;
    public Color Color;
    public AudioClip Sound;
}