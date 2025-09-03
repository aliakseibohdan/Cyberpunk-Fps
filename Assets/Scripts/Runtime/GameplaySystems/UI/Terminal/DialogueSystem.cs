using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueDisplay : DisplayBase
{
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private AudioSource audioSource;

    private List<DialogueLine> dialogueLines = new();
    private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine;

    public void Configure(List<DialogueLine> lines, float typingSpeed)
    {
        dialogueLines = lines;
        this.typingSpeed = typingSpeed;
    }

    public override void Show()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeDialogueRoutine());
    }

    public override void Hide()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        gameObject.SetActive(false);
    }

    private IEnumerator TypeDialogueRoutine()
    {
        dialogueText.text = string.Empty;

        foreach (var line in dialogueLines)
        {
            speakerNameText.text = line.SpeakerName;

            if (line.SpeakerPortrait != null)
            {
                speakerPortrait.sprite = line.SpeakerPortrait;
                speakerPortrait.gameObject.SetActive(true);
            }
            else
            {
                speakerPortrait.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(line.PreLineDelay);

            if (line.VoiceLine != null)
            {
                audioSource.PlayOneShot(line.VoiceLine);
            }

            for (int i = 0; i <= line.Text.Length; i++)
            {
                dialogueText.text = line.Text[..i];

                audioSource.PlayOneShot(Resources.Load<AudioClip>("TypeSound"));

                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(displayLifetime);
        Hide();
    }
}