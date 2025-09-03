using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioEvent", menuName = "Audio/Audio Event")]
public class AudioEvent : ScriptableObject
{
    public AudioClip[] clips;
    public AudioMixerGroup mixerGroup;
    public Vector2 volumeRange = new(0.9f, 1.1f);
    public Vector2 pitchRange = new(0.95f, 1.05f);
    public bool spatial = false;
    public float maxDistance = 50f;

    public void Play(Vector3 position = default)
    {
        if (clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        float volume = Random.Range(volumeRange.x, volumeRange.y);
        float pitch = Random.Range(pitchRange.x, pitchRange.y);

        if (spatial)
        {
            AudioManager.Instance.PlaySoundAtPoint(clip.name, position, mixerGroup, volume, 1f);
        }
        else
        {
            AudioManager.Instance.PlaySound(clip.name, mixerGroup, volume, pitch);
        }
    }
}