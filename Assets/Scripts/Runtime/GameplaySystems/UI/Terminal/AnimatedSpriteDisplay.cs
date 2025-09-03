using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedSpriteDisplay : DisplayBase
{
    [SerializeField] private Image spriteDisplay;
    [SerializeField] private AudioSource audioSource;

    private SpriteAnimation animationData;
    private Coroutine animationCoroutine;

    public void Configure(SpriteAnimation animation)
    {
        animationData = animation;
    }

    public override void Show()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(PlayAnimationRoutine());
    }

    public override void Hide()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        gameObject.SetActive(false);
    }

    private IEnumerator PlayAnimationRoutine()
    {
        for (int i = 0; i < animationData.Frames.Length; i++)
        {
            spriteDisplay.sprite = animationData.Frames[i];

            if (animationData.FrameSounds != null &&
                i < animationData.FrameSounds.Length &&
                animationData.FrameSounds[i] != null)
            {
                audioSource.PlayOneShot(animationData.FrameSounds[i]);
            }

            yield return new WaitForSeconds(1 / animationData.FrameRate);
        }

        yield return new WaitForSeconds(animationData.LifetimeAfterAnimation);
        Hide();
    }
}