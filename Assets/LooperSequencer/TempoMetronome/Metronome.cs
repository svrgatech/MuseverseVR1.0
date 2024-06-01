using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Metronome : MonoBehaviour
{
    public int bpm = 140;
    private bool running = false;
    bool bScheduledNextDownBeatTime = false;
    bool bScheduledNextUpBeatTime = false;
    public bool bUseMetronome = true;
    public AudioSource metronomeLow;
    public AudioSource metronomeHigh;
    public double nextDownBeatTime;
    public double nextUpBeatTime;
    public double previousDownBeatTime;
    public double previousUpBeatTime;
    public int signatureHi = 4;
    public int signatureLo = 4;
    public double timeBetweenDownBeats;
    public TMP_InputField tempoInput;

    private void Start()
    {
        //nextDownBeatTime = AudioSettings.dspTime + 2;
        //nextUpBeatTime = nextDownBeatTime;
        //timeBetweenDownBeats = (60 * signatureHi) / bpm;

        //running = true;
    }

    private void Update()
    {
        if (!running)
        {
            return;
        }

        if (!bScheduledNextDownBeatTime)
        {
            ScheduleNextDownBeatTime();
        }

        if (!bScheduledNextUpBeatTime)
        {
            ScheduleNextUpBeatTime();
        }

    }

    public void ScheduleNextDownBeatTime()
    {
        bScheduledNextDownBeatTime = true;
        if (bUseMetronome)
        {
            metronomeHigh.PlayScheduled(nextDownBeatTime);
        }

        Invoke("ResetDownBeatTimeBool", (float)(nextDownBeatTime + .1 - AudioSettings.dspTime));
    }

    public void ScheduleNextUpBeatTime()
    {

        bScheduledNextUpBeatTime = true;
        if (bUseMetronome)
        {
            metronomeLow.PlayScheduled(nextUpBeatTime);
        }

        Invoke("ResetUpBeatTimeBool", (float)(nextUpBeatTime + .1 - AudioSettings.dspTime));
    }

    public void ResetDownBeatTimeBool()
    {
        previousDownBeatTime = nextDownBeatTime;
        nextDownBeatTime += 60.0f / bpm * signatureHi;
        bScheduledNextDownBeatTime = false;
    }

    /// <summary>
    /// Update the time of the next up beat
    /// </summary>
    public void ResetUpBeatTimeBool()
    {

        previousUpBeatTime = nextUpBeatTime;
        nextUpBeatTime += 60.0f / (bpm * signatureHi) * signatureHi;
        bScheduledNextUpBeatTime = false;
    }

    public void SetMetronome()
    {
        if(!running)
        {
            nextDownBeatTime = AudioSettings.dspTime + 2;
            nextUpBeatTime = nextDownBeatTime;
            timeBetweenDownBeats = (60 * signatureHi) / bpm;

            running = true;
        }
        else
        {
            running = false;
        }
        
    }

    public void ChangeBPM(string input)
    {
        SetMetronome();

        int newBPM = int.Parse(input);
        bpm = newBPM;
        timeBetweenDownBeats = (60 * signatureHi) / bpm;
        nextDownBeatTime = AudioSettings.dspTime + 2.0f;
        nextUpBeatTime = nextDownBeatTime;

        SetMetronome();
    }
}
