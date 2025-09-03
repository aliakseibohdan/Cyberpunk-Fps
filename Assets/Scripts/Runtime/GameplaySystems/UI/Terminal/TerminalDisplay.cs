using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerminalDisplay : DisplayBase
{
    [SerializeField] private TMP_Text textDisplay;
    [SerializeField] private Image progressBar;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxLines = 10;

    private List<TerminalLine> lines = new();
    private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine;
    private Color defaultTextColor = Color.white;
    private readonly List<string> currentLines = new();

    public void Configure(List<TerminalLine> lines, float typingSpeed, Color defaultColor)
    {
        this.lines = lines;
        this.typingSpeed = typingSpeed;
        defaultTextColor = defaultColor;
    }

    public override void Show()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        currentLines.Clear();
        textDisplay.text = string.Empty;
        typingCoroutine = StartCoroutine(TypeTextRoutine());
    }

    public override void Hide()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        gameObject.SetActive(false);
    }

    private IEnumerator TypeTextRoutine()
    {
        foreach (var line in lines)
        {
            yield return new WaitForSeconds(line.PreLineDelay);

            if (line.LineSpecificSound != null)
            {
                audioSource.PlayOneShot(line.LineSpecificSound);
            }

            string currentText = string.Empty;
            for (int i = 0; i <= line.Text.Length; i++)
            {
                currentText = line.Text[..i];

                UpdateTextDisplay(currentText);

                audioSource.PlayOneShot(Resources.Load<AudioClip>("TypeSound"));

                if (progressBar != null)
                {
                    progressBar.fillAmount = (float)i / line.Text.Length;
                }

                yield return new WaitForSeconds(typingSpeed);
            }

            currentLines.Add($"<color=#{ColorUtility.ToHtmlStringRGB(line.Color != default ? line.Color : defaultTextColor)}>{line.Text}</color>");

            while (currentLines.Count > maxLines)
            {
                currentLines.RemoveAt(0);
            }

            UpdateTextDisplay("");

            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(displayLifetime);
        Hide();
    }

    private void UpdateTextDisplay(string currentLine)
    {
        string displayText = string.Join("\n", currentLines);

        if (!string.IsNullOrEmpty(currentLine))
        {
            if (currentLines.Count > 0) displayText += "\n";
            displayText += $"<color=#{ColorUtility.ToHtmlStringRGB(defaultTextColor)}>{currentLine}</color>";
        }

        textDisplay.text = displayText;

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}