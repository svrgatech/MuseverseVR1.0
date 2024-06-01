using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingManager : MonoBehaviour
{
    public static TimingManager Instance { get; private set; }
    public float bpm = 120; // Default BPM
    public double nextDownBeatTime;
    public double timeBetweenDownBeats;

    private double lastUpdateTime = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        UpdateTempo(bpm); // Initialize timing data
    }

    void Update()
    {
        UpdateNextDownBeatTime();
    }

    public void UpdateTempo(float newBpm)
    {
        bpm = newBpm;
        timeBetweenDownBeats = 60 / bpm; // Seconds per beat
        CalculateNextDownBeat();
    }

    // Calculate and update the next downbeat time continuously
    private void UpdateNextDownBeatTime()
    {
        double currentTime = AudioSettings.dspTime;
        if (currentTime >= nextDownBeatTime)
        {
            // As soon as current time surpasses or meets the next downbeat time,
            // calculate the next downbeat time again.
            nextDownBeatTime += timeBetweenDownBeats;

            // Optional: Trigger any events or updates that need to happen on the downbeat
            OnDownBeat(); // This method would need to be implemented to handle downbeat events
        }
    }

    // Recalculate the next downbeat time based on the current time
    public void CalculateNextDownBeat()
    {
        double currentTime = AudioSettings.dspTime;
        // Align the next downbeat to the nearest future beat based on current time
        double timeSinceLastBeat = (currentTime - lastUpdateTime) % timeBetweenDownBeats;
        nextDownBeatTime = currentTime + (timeBetweenDownBeats - timeSinceLastBeat);

        lastUpdateTime = currentTime; // Update the last update time
    }

    private void OnDownBeat()
    {
        // Handle any logic that needs to occur exactly on the downbeat
        // For example, notify other systems that rely on the downbeat timing
    }
}


