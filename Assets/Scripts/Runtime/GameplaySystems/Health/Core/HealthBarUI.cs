using System.Collections;
using UnityEngine;

public class HealthBarUI : MonoBehaviour, IHealthView
{
    [Header("Health Bar References")]
    [SerializeField] private RectTransform healthBarContainer;
    [SerializeField] private UnityEngine.UI.Image instantHealthBar;
    [SerializeField] private UnityEngine.UI.Image smoothHealthBar;

    [Header("Colors")]
    [SerializeField] private Color healthyColor = Color.cyan;
    [SerializeField] private Color damagedColor = Color.red;
    [SerializeField] private Color healColor = Color.green;

    [Header("Animation Settings")]
    [SerializeField] private float smoothAnimationTime = 0.8f;
    [SerializeField] private float healAnimationTime = 0.5f;

    private float currentSmoothFill;
    private float targetSmoothFill;
    private Coroutine smoothCoroutine;
    private bool isAnimating;

    private void Start()
    {
        currentSmoothFill = 1f;
        targetSmoothFill = 1f;
        instantHealthBar.fillAmount = 1f;
        smoothHealthBar.fillAmount = 1f;
    }

    public void UpdateHealth(float currentHealth, float maxHealth, bool isDamage)
    {
        float targetFill = currentHealth / maxHealth;

        if (isDamage)
        {
            // Damage: instant bar snaps to new value, smooth bar animates with red color
            instantHealthBar.fillAmount = targetFill;
            smoothHealthBar.color = damagedColor;
            AnimateSmoothBar(targetFill, smoothAnimationTime);
        }
        else
        {
            // Healing: smooth bar snaps to new value, instant bar animates with green color
            smoothHealthBar.fillAmount = targetFill;
            smoothHealthBar.color = healColor;
            AnimateInstantBar(targetFill, healAnimationTime);
        }
    }

    public void UpdateArmor(float currentArmor)
    {
        // Optional: Add armor visualization
    }

    private void AnimateSmoothBar(float targetFill, float duration)
    {
        if (smoothCoroutine != null)
            StopCoroutine(smoothCoroutine);

        smoothCoroutine = StartCoroutine(AnimateBarCoroutine(smoothHealthBar, targetFill, duration));
    }

    private void AnimateInstantBar(float targetFill, float duration)
    {
        if (smoothCoroutine != null)
            StopCoroutine(smoothCoroutine);

        smoothCoroutine = StartCoroutine(AnimateBarCoroutine(instantHealthBar, targetFill, duration));
    }

    private IEnumerator AnimateBarCoroutine(UnityEngine.UI.Image bar, float targetFill, float duration)
    {
        float startFill = bar.fillAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            bar.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }

        bar.fillAmount = targetFill;
        smoothCoroutine = null;
    }

    private void ResetColors()
    {
        instantHealthBar.color = healthyColor;
        smoothHealthBar.color = healthyColor;
    }
}