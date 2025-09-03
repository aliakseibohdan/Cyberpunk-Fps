using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterCall
{
    public string CallerName;
    public string ReceiverName;
    public DialogueLine[] DialogueLines;
    public Sprite CallerPortrait;
    public Sprite ReceiverPortrait;
}

public class CharacterCallManager : MonoBehaviour
{
    [SerializeField] private TerminalDialogueManager displayManager;
    [SerializeField] private CharacterCall[] availableCalls;

    private readonly Dictionary<string, CharacterCall> callDictionary = new();
    private DialogueDisplay? currentCallDisplay;

    private void Start()
    {
        // Populate dictionary for easy access
        foreach (var call in availableCalls)
        {
            string key = $"{call.CallerName}_{call.ReceiverName}";
            callDictionary[key] = call;
        }

        if (displayManager == null)
        {
            displayManager = FindAnyObjectByType<TerminalDialogueManager>();
        }
    }

    public void InitiateCall(string callerName, string receiverName, Vector3 callPosition)
    {
        string key = $"{callerName}_{receiverName}";

        if (callDictionary.TryGetValue(key, out CharacterCall call))
        {
            var dialogueLines = new List<DialogueLine>();

            foreach (var line in call.DialogueLines)
            {
                var newLine = line;

                if (line.SpeakerName == call.CallerName)
                {
                    newLine.SpeakerPortrait = call.CallerPortrait;
                }
                else if (line.SpeakerName == call.ReceiverName)
                {
                    newLine.SpeakerPortrait = call.ReceiverPortrait;
                }

                dialogueLines.Add(newLine);
            }

            currentCallDisplay = displayManager.CreateDialogueDisplay(
                dialogueLines,
                callPosition
            );
        }
        else
        {
            Debug.LogWarning($"No call defined between {callerName} and {receiverName}");
        }
    }

    public void EndCurrentCall()
    {
        if (currentCallDisplay != null)
        {
            displayManager.RemoveDisplay(currentCallDisplay);
            currentCallDisplay = null;
        }
    }
}