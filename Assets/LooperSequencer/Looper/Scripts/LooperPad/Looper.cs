using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Looper : MonoBehaviour
{
    public string looperName = "testname";

    public Slider progressBar;
    public Image looperBackgroundImage;
    public GameObject bpmWarningObject;
    public TMP_InputField looperNameInputField;
    public Button playButton;
    public TextMeshProUGUI recordButtonText;
    public TextMeshProUGUI looperKey;
    public bool loopToggle;


    public AudioSource audioSource0;
    public AudioSource audioSource1;

    int frequency = 44100;

    //the delay to account for when starting playback
    double playbackBuffer = .15f;

    double recordingStartTime = 100f;
    double recordingFirstBeatTime = 200f;

    double firstClipStartTime = 0f;
    double secondClipStartTime = 0f;

    public bool bIsRecording = false;
    public bool bHasClip = false;
    public bool bPaused = true;
    public bool bIsPlaying = false;
    private bool bIsPlayingCoroutine = false;
    public bool isSamplePlayer;

    //the index (0 or 1) of the audiosource that is playing
    int clipIndex = 0;

    //whether we've scheduled a clip to play (the first clip should play first)
    bool bScheduledFirstClip = true;
    bool bScheduledSecondClip = false;
    
    //number of measures recorded
    public double numberOfMeasures = 0;
    public double recordedBPM = 120;
    public int recordedSignatureHi = 4;


    //audio clip if sample player
    public AudioClip looperClip;
    public WaveformPreviewCreator waveformPreview;
    
    //coroutines used to switch between audiosources
    Coroutine switchCoroutine0;
    Coroutine switchCoroutine1;
    Coroutine isPlayingCoroutine;


    

    // Start is called before the first frame update
    void Start()
    {
        
        
        if (Microphone.devices.Length <= 0)
        {
            Station.instance.ShowErrorMessage("No microphone detected");
        }

        playButton.enabled = false;
        bpmWarningObject.SetActive(false);

        if (isSamplePlayer)
            playButton.enabled = true;

       

    }

    /// <summary>
    /// changes the looper background to red after a certain amount of time
    /// </summary>
    /// <param name="wait">the time to wait</param>
    /// <returns></returns>
    IEnumerator ChangeLooperBackgroundRoutine(double wait)
    {
        yield return new WaitForSeconds((float)wait);
        looperBackgroundImage.color = new Color(1f, 0f, 0f, 222f / 255f);
    }

    /// <summary>
    /// Plays or pauses the loop
    /// </summary>
    public void Playback()
    {


        if(looperClip && isSamplePlayer)
        {
            SetAudioClip(looperClip);
        }

        if (bHasClip)
        {
            //To play one shot of the audio
            if (!loopToggle)
            {
                if (audioSource0.isPlaying)
                {
                    audioSource0.Stop();
                }
                else
                {
                    audioSource0.PlayScheduled(AudioSettings.dspTime);
                }

                return;
            }

            if (bPaused)
            {
                if (bIsPlayingCoroutine)
                {
                    //return since we are about to be paused
                    return;
                }
                //make sure that we have at least 0.4 seconds to prepare to play
                if (Station.instance.nextDownBeatTime - AudioSettings.dspTime >= .4)
                {
                    bIsPlaying = true;
                    bPaused = false;
                    clipIndex = 0;
                    //stop coroutines from last time the clip was played
                    if (switchCoroutine0 != null)
                    {
                        StopCoroutine(switchCoroutine0);
                    }
                    if (switchCoroutine1 != null)
                    {
                        StopCoroutine(switchCoroutine1);
                    }
                    //schedule and start the playback loop
                    audioSource0.PlayScheduled(Station.instance.nextDownBeatTime - playbackBuffer);
                    firstClipStartTime = Station.instance.nextDownBeatTime - playbackBuffer;
                    StartCoroutine(PlaybackLoop());
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void ToggleMute()
    {
        audioSource0.mute = !audioSource0.mute;
        audioSource1.mute = !audioSource1.mute;
    }


    /// <summary>
    /// Pauses (stops) the loop.
    /// </summary>
    void Pause()
    {
        bPaused = true;
        if (switchCoroutine0 != null)
        {
            StopCoroutine(switchCoroutine0);
        }
        if (switchCoroutine1 != null)
        {
            StopCoroutine(switchCoroutine1);
        }


        //end the currently playing clip 
        if (clipIndex == 0)
        {
            audioSource0.SetScheduledEndTime(Station.instance.nextDownBeatTime);
            audioSource1.Stop();
        }
        else
        {
            audioSource1.SetScheduledEndTime(Station.instance.nextDownBeatTime);
            audioSource0.Stop();
        }
        isPlayingCoroutine = StartCoroutine(SetIsPlayingRoutine((float)(Station.instance.nextDownBeatTime - AudioSettings.dspTime)));
        bScheduledFirstClip = false;
        bScheduledSecondClip = false;
        
    }
    
    /// <summary>
    /// Continuously plays the recorded clip
    /// </summary>
    /// <returns></returns>
    IEnumerator PlaybackLoop()
    {
        while (bHasClip && !bPaused)
        {
            if (clipIndex == 0)
            {
                //if the first clip has started playing
                if (AudioSettings.dspTime - firstClipStartTime >= 0.1)
                {

                    if (bScheduledSecondClip == false)
                    {
                        audioSource0.SetScheduledEndTime((Station.instance.nextDownBeatTime + Station.instance.timeBetweenDownBeats * numberOfMeasures) + playbackBuffer);
                        audioSource1.PlayScheduled(Station.instance.nextDownBeatTime + (Station.instance.timeBetweenDownBeats * numberOfMeasures) - playbackBuffer);
                            
                        secondClipStartTime = Station.instance.nextDownBeatTime + (Station.instance.timeBetweenDownBeats * numberOfMeasures) - playbackBuffer;
                        bScheduledSecondClip = true;
                        bScheduledFirstClip = false;
                        switchCoroutine1 = StartCoroutine(SwitchClipIndex((float)(secondClipStartTime - AudioSettings.dspTime)));

                    }
                    
                }
            }
            else
            {
                //if the second clip has started playing
                if (AudioSettings.dspTime - secondClipStartTime >= 0.1)
                {

                    if (bScheduledFirstClip == false)
                    {
                        audioSource1.SetScheduledEndTime((Station.instance.nextDownBeatTime + Station.instance.timeBetweenDownBeats * numberOfMeasures) + playbackBuffer);
                        audioSource0.PlayScheduled(Station.instance.nextDownBeatTime + (Station.instance.timeBetweenDownBeats * numberOfMeasures) - playbackBuffer);
                            
                        firstClipStartTime = Station.instance.nextDownBeatTime + (Station.instance.timeBetweenDownBeats * numberOfMeasures) - playbackBuffer;
                        bScheduledFirstClip = true;
                        bScheduledSecondClip = false;
                        switchCoroutine0 = StartCoroutine(SwitchClipIndex((float)(firstClipStartTime - AudioSettings.dspTime)));
                          
                    }
                    
                }
            }
            yield return null;
        }
    }


    IEnumerator SetIsPlayingRoutine(float timeToWait)
    {
        bIsPlayingCoroutine = true;
        yield return new WaitForSeconds(timeToWait);
        bIsPlaying = false;
        bIsPlayingCoroutine = false;
    }

    public void AdjustTempoToBPM(float targetBPM, float originalBPM)
    {
        if (audioSource0 != null && audioSource1!=null)
        {
            float pitchAdjustment = targetBPM / originalBPM;
            audioSource0.pitch = pitchAdjustment;
            audioSource1.pitch = pitchAdjustment;
        }
    }



    // Update is called once per frame
    void Update()
    {
        //update the looper's progress bar
            if (clipIndex == 0 && bHasClip)
            {
                int timeSamples = audioSource0.timeSamples;
                int clipLength = audioSource0.clip.samples;
                float percent = (float)timeSamples / clipLength;
                progressBar.value = percent;
            }
            if (clipIndex == 1 && bHasClip)
            {
                int timeSamples = audioSource1.timeSamples;
                int clipLength = audioSource1.clip.samples;
                float percent = (float)timeSamples / clipLength;
                progressBar.value = percent;
            }
        
        

    }

    /// <summary>
    /// Switches the clipIndex to 0 or 1 after a given amount of time
    /// </summary>
    /// <param name="timeToWait">the time to wait before switching the clipIndex</param>
    /// <returns></returns>
    public IEnumerator SwitchClipIndex(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);
        if (!bPaused)
        {
            clipIndex = 1 - clipIndex;
        }
    }


    /// <summary>
    /// Clears (deletes) the loop stored in the looper
    /// </summary>
    public void ClearLoop()
    {
        bHasClip = false;
        bIsRecording = false;
        playButton.enabled = false;
        audioSource0.Stop();
        audioSource1.Stop();
        if (switchCoroutine0 != null)
        {
            StopCoroutine(switchCoroutine0);
        }
        if (switchCoroutine1 != null)
        {
            StopCoroutine(switchCoroutine1);
        }
        looperBackgroundImage.color = new Color(173f / 255f, 173f / 255f, 173f / 255f, 180f / 255f);
        bScheduledFirstClip = false;
        bScheduledSecondClip = false;
        audioSource0.clip = null;
        audioSource1.clip = null;
        bpmWarningObject.SetActive(false);
        bIsPlaying = false;
    }

    /// <summary>
    /// Checks if looper bpm matches the station bpm. Also checks
    /// if looper high signature matches the station high signature.
    /// If not, display a warning message.
    /// </summary>
    public void UpdatedBPM()
    {
        if ((Station.instance.bpm != recordedBPM && bHasClip) || (Station.instance.signatureHi != recordedSignatureHi && bHasClip))
        {
            bpmWarningObject.SetActive(true);           
        }
        else
        {
            bpmWarningObject.SetActive(false);
        }
    }

    /// <summary>
    /// Deletes this looper
    /// </summary>
    public void DeleteLooper()
    {
        ClearLoop();
        Station.instance.loopers.Remove(this);
        Destroy(gameObject);
    }

    /// <summary>
    /// Sets the looper's name to be what is in the looperNameInputField.
    /// </summary>
    public void SetLooperName()
    {
        if (!string.IsNullOrWhiteSpace(looperNameInputField.text))
        {
            looperName = looperNameInputField.text;
        }
        else
        {
            
            looperNameInputField.text = looperName;
        }
    }

    /// <summary>
    /// Sets the looper's audio clip to the provided clip.
    /// This is called when a previous looping session has been loaded.
    /// </summary>
    /// <param name="clip">the clip to use</param>
    public void SetAudioClip(AudioClip clip)
    {
        audioSource0.clip = clip;
        audioSource1.clip = clip;
        bHasClip = true;
        playButton.enabled = true;
        looperBackgroundImage.color = new Color(173f / 255f, 173f / 255f, 173f / 255f, 222f / 255f);

        if(waveformPreview)
        {
            waveformPreview.CreateWaveformPreview(clip);
        }

    }

    public void OnLoopToggle(bool value)
    {
        Pause();
        loopToggle = value;
       
    }

    public void SetNumberOfMeasures(string valueFromInputField)
    {
        numberOfMeasures = int.Parse(valueFromInputField);
    }
}
