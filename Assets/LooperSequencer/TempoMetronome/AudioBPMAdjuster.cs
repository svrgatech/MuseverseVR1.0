using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioBPMAdjuster : MonoBehaviour
{
    public AudioSource audioSource;
    public float originalBPM = 120.0f;  // Set the original BPM of the clip
    public float targetBPM = 120.0f;    // Set your target BPM here

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        AdjustTempoToBPM();
    }

    void AdjustTempoToBPM()
    {
        if (audioSource != null)
        {
            float pitchAdjustment = targetBPM / originalBPM;
            audioSource.pitch = pitchAdjustment;
        }
    }

    // Optionally, update BPM dynamically during runtime
    void Update()
    {
        // For example, changing the targetBPM based on some game logic
        //AdjustTempoToBPM();
    }
}
