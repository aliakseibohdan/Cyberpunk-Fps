using UnityEngine;

public class StopParticleEmissionAfterTime : MonoBehaviour
{
    [Tooltip("Time in seconds after which the particle system stops emitting new particles")]
    public float stopAfterSeconds = 60f;

    private new ParticleSystem particleSystem;
    private float timer;
    private bool hasStopped;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();

        if (particleSystem == null)
        {
            Debug.LogError("No ParticleSystem found on this GameObject!", this);
            enabled = false;
            return;
        }

        timer = 0f;
        hasStopped = false;
    }

    void Update()
    {
        if (hasStopped) return;

        timer += Time.deltaTime;

        if (timer >= stopAfterSeconds)
        {
            var emission = particleSystem.emission;
            emission.enabled = false;

            hasStopped = true;
            enabled = false;
        }
    }
}