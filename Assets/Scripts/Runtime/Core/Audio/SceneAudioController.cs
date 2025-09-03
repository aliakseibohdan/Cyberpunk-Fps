using UnityEngine;

public class SceneAudioController : MonoBehaviour
{
    [SerializeField] private string ambientSound;
    [SerializeField] private string musicTrack;
    [SerializeField] private float musicFadeDuration = 2f;
    [SerializeField] private float ambienceFadeDuration = 3f;

    private void Start()
    {
        if (!string.IsNullOrEmpty(ambientSound))
        {
            AudioManager.Instance.PlayAmbience(ambientSound, ambienceFadeDuration);
        }

        if (!string.IsNullOrEmpty(musicTrack))
        {
            AudioManager.Instance.PlayMusic(musicTrack, musicFadeDuration);
        }
    }

    private void OnDestroy()
    {
        AudioManager.Instance.PlayMusic("Silence", 1f);
        AudioManager.Instance.PlayAmbience("Silence", 1f);
    }
}