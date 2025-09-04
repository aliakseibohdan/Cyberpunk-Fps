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
    [SerializeField] private RectTransform containerRect;
    [SerializeField] private float scaleAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private List<DialogueLine> dialogueLines = new();
    private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine;
    private Coroutine scaleCoroutine;
    private Vector3 originalScale;

    public void Configure(List<DialogueLine> lines, float typingSpeed)
    {
        dialogueLines = lines;
        this.typingSpeed = typingSpeed;

        if (containerRect != null)
        {
            originalScale = containerRect.localScale;
        }
    }

    public override void Show()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        if (containerRect != null)
        {
            containerRect.pivot = new Vector2(0.5f, 0.5f);

            containerRect.localScale = new Vector3(originalScale.x, 0, originalScale.z);
        }

        gameObject.SetActive(true);

        if (dialogueLines.Count > 0)
        {
            var firstLine = dialogueLines[0];
            speakerNameText.text = firstLine.SpeakerName;

            if (firstLine.SpeakerPortrait != null)
            {
                speakerPortrait.sprite = firstLine.SpeakerPortrait;
                speakerPortrait.color = Color.white;
                speakerPortrait.gameObject.SetActive(true);
            }
            else
            {
                speakerPortrait.gameObject.SetActive(false);
            }

            dialogueText.text = string.Empty;
        }

        scaleCoroutine = StartCoroutine(ScaleAnimation(true, () => {
            typingCoroutine = StartCoroutine(TypeDialogueRoutine());
        }));
    }

    public override void Hide()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleAnimation(false, () => {
            gameObject.SetActive(false);
        }));
    }

    private IEnumerator ScaleAnimation(bool scalingUp, System.Action onComplete = null)
    {
        if (containerRect == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float elapsedTime = 0f;
        Vector3 startScale = containerRect.localScale;
        Vector3 targetScale = scalingUp ? originalScale : new Vector3(originalScale.x, 0, originalScale.z);

        Debug.Log($"Scaling {(scalingUp ? "up" : "down")} from {startScale} to {targetScale}");

        while (elapsedTime < scaleAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = scaleAnimationCurve.Evaluate(elapsedTime / scaleAnimationDuration);

            containerRect.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        containerRect.localScale = targetScale;

        onComplete?.Invoke();
    }

    private IEnumerator TypeDialogueRoutine()
    {
        int startIndex = dialogueLines.Count > 0 ? 1 : 0;

        if (startIndex == 0)
        {
            yield break;
        }

        for (int lineIndex = startIndex; lineIndex < dialogueLines.Count; lineIndex++)
        {
            var line = dialogueLines[lineIndex];

            speakerNameText.text = line.SpeakerName;

            if (line.SpeakerPortrait != null)
            {
                speakerPortrait.sprite = line.SpeakerPortrait;
                speakerPortrait.color = Color.white;
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

            string currentText = "";
            for (int i = 0; i <= line.Text.Length; i++)
            {
                currentText = line.Text[..i];
                dialogueText.text = currentText;

                audioSource.PlayOneShot(Resources.Load<AudioClip>("TypeSound"));

                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(displayLifetime);
        Hide();
    }
}