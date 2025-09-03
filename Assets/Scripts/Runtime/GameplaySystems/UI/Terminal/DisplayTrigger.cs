using System;
using System.Linq;
using UnityEngine;

public class DisplayTrigger : MonoBehaviour
{
    [SerializeField] private DisplayType triggerType;
    [SerializeField] private TerminalLine[] terminalLines = Array.Empty<TerminalLine>();
    [SerializeField] private DialogueLine[] dialogueLines = Array.Empty<DialogueLine>();
    [SerializeField] private SpriteAnimation spriteAnimation;

    [Header("Scriptable Objects")]
    [SerializeField] private TerminalScript terminalScript;

    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnEnter = true;
    [SerializeField] private bool triggerOnStay = false;
    [SerializeField] private bool triggerOnExit = false;
    [SerializeField] private float stayTriggerInterval = 2.0f;

    [Header("Display Settings")]
    [SerializeField] private Vector3 displayOffset = new(0, 0.5f, 0); // Relative to camera
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private Color textColor = Color.green;

    private TerminalDialogueManager manager;
    private DisplayBase currentDisplay;
    private float stayTimer = 0f;

    private void Start()
    {
        manager = FindAnyObjectByType<TerminalDialogueManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggerOnEnter || !other.CompareTag("Player")) return;

        TriggerDisplay();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!triggerOnStay || !other.CompareTag("Player")) return;

        stayTimer += Time.deltaTime;
        if (stayTimer >= stayTriggerInterval)
        {
            stayTimer = 0f;
            TriggerDisplay();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!triggerOnExit || !other.CompareTag("Player")) return;

        TriggerDisplay();
    }

    private void TriggerDisplay()
    {
        if (manager == null) return;

        if (currentDisplay != null)
        {
            manager.RemoveDisplay(currentDisplay);
        }

        switch (triggerType)
        {
            case DisplayType.TerminalText when terminalScript != null:
                currentDisplay = manager.CreateTerminalDisplayFromScript(
                    terminalScript,
                    displayOffset,
                    typingSpeed
                );
                break;

            case DisplayType.TerminalText:
                currentDisplay = manager.CreateTerminalDisplay(
                    terminalLines.ToList(),
                    transform.position + displayOffset,
                    typingSpeed,
                    textColor
                );
                break;

            case DisplayType.Dialogue:
                currentDisplay = manager.CreateDialogueDisplay(
                    dialogueLines.ToList(),
                    displayOffset,
                    typingSpeed
                );
                break;

            case DisplayType.AnimatedSprite:
                currentDisplay = manager.CreateAnimatedSpriteDisplay(
                    spriteAnimation,
                    displayOffset
                );
                break;
        }
    }
}