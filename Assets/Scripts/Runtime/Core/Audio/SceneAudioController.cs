using UnityEngine;

public class SceneAudioController : MonoBehaviour
{
    [SerializeField] private string ambientSound;
    [SerializeField] private string musicTrack;
    [SerializeField] private float musicFadeDuration = 2f;
    [SerializeField] private float ambienceFadeDuration = 3f;

    private string currentAmbient;
    private string currentMusic;

    private void Start()
    {
        currentAmbient = ambientSound;
        currentMusic = musicTrack;

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
        // Only fade to silence if we were actually playing something
        if (!string.IsNullOrEmpty(currentMusic) && currentMusic != "Silence")
        {
            AudioManager.Instance.PlayMusic("Silence", 1f);
        }

        if (!string.IsNullOrEmpty(currentAmbient) && currentAmbient != "Silence")
        {
            AudioManager.Instance.PlayAmbience("Silence", 1f);
        }
    }
}