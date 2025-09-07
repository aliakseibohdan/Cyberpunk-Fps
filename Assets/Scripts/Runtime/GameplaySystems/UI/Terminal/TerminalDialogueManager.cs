using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerminalDialogueManager : MonoBehaviour
{
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Transform canvasCamera;
    [SerializeField] private GameObject terminalDisplayPrefab;
    [SerializeField] private GameObject dialogueDisplayPrefab;
    [SerializeField] private GameObject animatedSpritePrefab;

    [Header("Default Settings")]
    [SerializeField] private float defaultTypingSpeed = 0.05f;
    [SerializeField] private Color defaultTerminalColor = Color.green;

    private readonly List<DisplayBase> activeDisplays = new();

    private void Start()
    {
        if (worldCanvas == null)
        {
            worldCanvas = FindAnyObjectByType<Canvas>();
            if (worldCanvas == null)
            {
                Debug.LogError("No Canvas found in scene!");
            }
        }
    }

    public TerminalDisplay CreateTerminalDisplay(
        List<TerminalLine> lines,
        Vector3 worldOffset,
        float? typingSpeed = null,
        Color? defaultColor = null)
    {
        if (canvasCamera == null || worldCanvas == null)
            throw new InvalidOperationException("Manager not properly initialized");

        Vector3 startPosition = canvasCamera.position + canvasCamera.forward;
        var displayObj = Instantiate(terminalDisplayPrefab, startPosition, Quaternion.identity, worldCanvas.transform);
        var terminalDisplay = displayObj.GetComponent<TerminalDisplay>();

        terminalDisplay.Configure(
            lines,
            typingSpeed ?? defaultTypingSpeed,
            defaultColor ?? defaultTerminalColor
        );

        terminalDisplay.Initialize(canvasCamera, worldOffset);
        activeDisplays.Add(terminalDisplay);
        terminalDisplay.Show();

        return terminalDisplay;
    }

    public TerminalDisplay CreateTerminalDisplayFromScript(
        TerminalScript script,
        Vector3 worldOffset,
        float? typingSpeed = null)
    {
        var parsedLines = TerminalParser.ParseTerminalScript(
            script.scriptText,
            script.defaultDelay,
            script.defaultColor
        );

        var terminalLines = new List<TerminalLine>();
        foreach (var parsedLine in parsedLines)
        {
            terminalLines.Add(new TerminalLine
            {
                Text = parsedLine.Text,
                PreLineDelay = parsedLine.Delay,
                Color = parsedLine.Color,
                LineSpecificSound = parsedLine.Sound
            });
        }

        return CreateTerminalDisplay(terminalLines, worldOffset, typingSpeed, script.defaultColor);
    }

    public DialogueDisplay CreateDialogueDisplay(
        List<DialogueLine> lines,
        Vector3 worldOffset,
        float? typingSpeed = null)
    {
        if (canvasCamera == null || worldCanvas == null)
            throw new InvalidOperationException("Manager not properly initialized");

        Vector3 startPosition = canvasCamera.position + canvasCamera.forward;
        var displayObj = Instantiate(dialogueDisplayPrefab, startPosition + worldOffset, Quaternion.identity, worldCanvas.transform);
        var dialogueDisplay = displayObj.GetComponent<DialogueDisplay>();

        dialogueDisplay.Configure(lines, typingSpeed ?? defaultTypingSpeed);
        dialogueDisplay.Initialize(canvasCamera, worldOffset);
        activeDisplays.Add(dialogueDisplay);
        dialogueDisplay.Show();

        return dialogueDisplay;
    }

    public AnimatedSpriteDisplay CreateAnimatedSpriteDisplay(
        SpriteAnimation animation,
        Vector3 worldPosition)
    {
        if (canvasCamera == null || worldCanvas == null)
            throw new InvalidOperationException("Manager not properly initialized");

        var displayObj = Instantiate(animatedSpritePrefab, worldPosition, Quaternion.identity, worldCanvas.transform);
        var spriteDisplay = displayObj.GetComponent<AnimatedSpriteDisplay>();

        spriteDisplay.Configure(animation);
        spriteDisplay.Initialize(canvasCamera, worldPosition);
        activeDisplays.Add(spriteDisplay);
        spriteDisplay.Show();

        return spriteDisplay;
    }

    public void RemoveDisplay(DisplayBase display)
    {
        if (activeDisplays.Contains(display))
        {
            activeDisplays.Remove(display);
            display.Hide();
            Destroy(display.gameObject);
        }
    }

    public void ClearAllDisplays()
    {
        foreach (var display in activeDisplays.ToList())
        {
            RemoveDisplay(display);
        }
    }
}