﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.IO;
using System;

public class Station : MonoBehaviour
{

    public static Station instance;


    
    public double bpm = 140.0F;

    //Update this sample pack BPM
    //when the sample pack is Loaded! This is for the correct tempo calculation!
    public float samplePackBPM = 120f;

    public double oldBpm;
    
    public int signatureHi = 4;
    
    public int signatureLo = 4;
    
    
    private bool running = false;
    
    public double startTick;
    public double nextDownBeatTime;
    
    public double nextUpBeatTime;
    public double currentDownBeatTime;
    public double currentTime;
    public double previousDownBeatTime;
    
    public double currentUpBeatTime;
    
    public double previousUpBeatTime;

    public AudioSource metronomeLow;
    public AudioSource metronomeHigh;
    public Image signature34Image;
    public Image signature44Image;
    public TextMeshProUGUI countInButtonText;
    public TextMeshProUGUI countOutButtonText;
    public GameObject looperPrefab;
    public Transform backgroundPanel;


    public bool bRecordingALoop = false;
    

    public double timeBetweenDownBeats;

    public int recordingCountIn = 1;
    public int recordingCountOut = 1;


    public TMP_InputField bpmInputField;
    
    public bool bUseMetronome = true;

    bool bScheduledNextDownBeatTime = false;
    bool bScheduledNextUpBeatTime = false;

    public List<Looper> loopers = new List<Looper>();

    public int[] looperState;

    public GameObject loadSessionMenu;
    public RectTransform savedFolderContent;
    public GameObject savedSessionPrefab;
    public TextMeshProUGUI sessionTitle;
    public TextMeshProUGUI errorMessage;

    Coroutine errorMessageRoutineObject;

    public SamplePackManager samplePackManager;

    /// <summary>
    /// Initialize singleton
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

        }
        else if (instance != this)
        {
            Destroy(this);
        }
        
    }

    void Start()
    {
        nextDownBeatTime = AudioSettings.dspTime + 2;
        nextUpBeatTime = nextDownBeatTime;
        timeBetweenDownBeats = (60 * signatureHi) / bpm;

        running = true;
        loadSessionMenu.SetActive(false);

        ShowErrorMessage("Save folder located at: " + Application.persistentDataPath + "/SavedLoops");


        looperState = new int[loopers.Count];
        
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

    /// <summary>
    /// Checks if the user entered a valid bpm then updates the bpm
    /// </summary>
    public void ChangeBPM()
    {

        string currentBPM = ((int)bpm).ToString();

       

        foreach (Looper looper in loopers)
        {
            if (looper.bIsPlaying)
            {
                ShowErrorMessage("Cannot change bpm when a looper is playing.");
                bpmInputField.text = currentBPM;
                return;
            }
        }
        int tempBPM;

        int.TryParse(bpmInputField.text,System.Globalization.NumberStyles.Integer,null, out tempBPM);
        if (tempBPM > 0)
        {
            //bpm must be between 40 and 150
            if (tempBPM >= 40 && tempBPM <= 150)
            {
                bpm = tempBPM;
                timeBetweenDownBeats = (60 * signatureHi) / bpm;
                nextDownBeatTime = AudioSettings.dspTime + 2.0f;
                nextUpBeatTime = nextDownBeatTime;

                foreach (Looper looper in loopers)
                {
                    looper.UpdatedBPM();
                }
            }
            else
            {
                ShowErrorMessage("Invalid BPM input. (must be between 40 and 150)");
                bpmInputField.text = currentBPM;
            }
        }
        else
        {
            ShowErrorMessage("Invalid BPM input. (must be between 40 and 150)");
            bpmInputField.text = currentBPM;
        }

    }

    //Update the tempo on all the loopers when it is changed
    //from the tempo module
    public void UpdateLooperTempos(string tempoInput)
    {
        if(loopers!=null && loopers.Count>0)
        {
            foreach(Looper looper in loopers)
            {
                looper.AdjustTempoToBPM(float.Parse(tempoInput), samplePackBPM);
            }
        }
    }

    /// <summary>
    /// Changes the bpm to the loaded session's bpm
    /// </summary>
    /// <param name="newBPM">the new bpm to use</param>
    public void ChangeBPM(double newBPM)
    {
        if (newBPM != bpm)
        {
            bpm = newBPM;
            timeBetweenDownBeats = (60 * signatureHi) / bpm;
            nextDownBeatTime = AudioSettings.dspTime + 2.0f;
            nextUpBeatTime = nextDownBeatTime;

            foreach (Looper looper in loopers)
            {
                looper.UpdatedBPM();
            }
            bpmInputField.text = ((int)bpm).ToString();
        }
    }

    /// <summary>
    /// Changes the time signature
    /// </summary>
    /// <param name="newHi">high signature</param>
    public void ChangeTimeSignature(int newHi)
    {
        if (signatureHi != newHi)
        {
            foreach (Looper looper in loopers)
            {
                if (looper.bIsPlaying)
                {
                    ShowErrorMessage("Cannot change time signature when a looper is playing.");
                    return;
                }
            }
            if (newHi == 3)
            {
                signature34Image.color = new Color(.6f, .6f, .6f, 1f);
                signature44Image.color = new Color(1, 1, 1, 1);

            }
            else
            {
                signature44Image.color = new Color(.6f, .6f, .6f, 1f);
                signature34Image.color = new Color(1, 1, 1, 1);
            }
            signatureHi = newHi;
            timeBetweenDownBeats = (60 * signatureHi) / bpm;
            nextDownBeatTime = AudioSettings.dspTime + 2.0f;
            nextUpBeatTime = nextDownBeatTime;
            foreach (Looper looper in loopers)
            {
                looper.UpdatedBPM();
            }
        }
    }

    /// <summary>
    /// enables or disables the metronome sound
    /// </summary>
    public void SetMetronome()
    {
        bUseMetronome = !bUseMetronome;
    }

    /// <summary>
    /// Schedules the next down beat
    /// </summary>
    public void ScheduleNextDownBeatTime()
    {
        bScheduledNextDownBeatTime = true;
        if (bUseMetronome)
        {
            metronomeHigh.PlayScheduled(nextDownBeatTime);
        }

        Invoke("ResetDownBeatTimeBool",(float)(nextDownBeatTime + .1 - AudioSettings.dspTime));
    }

    /// <summary>
    /// Schedules the next up beat
    /// </summary>
    public void ScheduleNextUpBeatTime()
    {
        
        bScheduledNextUpBeatTime = true;
        if (bUseMetronome)
        {
            metronomeLow.PlayScheduled(nextUpBeatTime);
        }
              
        Invoke("ResetUpBeatTimeBool", (float)(nextUpBeatTime + .1 - AudioSettings.dspTime));
    }


    /// <summary>
    /// Updates the time of the next down beat
    /// </summary>
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

    /// <summary>
    /// Updates the countIn measure count
    /// </summary>
    public void PressedCountInButton()
    {
        if (bRecordingALoop) { return; }
        if (recordingCountIn == 3)
        {
            recordingCountIn = 1;
        }
        else
        {
            recordingCountIn++;
        }
        countInButtonText.text = recordingCountIn.ToString();
    }

    /// <summary>
    /// updates the countOut measure count
    /// </summary>
    public void PressedCountOutButton()
    {
        if (bRecordingALoop) { return; }
        if (recordingCountOut == 3)
        {
            recordingCountOut = 1;
        }
        else
        {
            recordingCountOut++;
        }
        countOutButtonText.text = recordingCountOut.ToString();
    }

    /// <summary>
    /// Adds a looper to the session
    /// </summary>
    public void AddLooper()
    {
        if (loopers.Count < 16)
        {
            Looper newLooper = Instantiate(looperPrefab, backgroundPanel).GetComponent<Looper>();
            newLooper.looperName = "Looper0" + (loopers.Count + 1).ToString();
            newLooper.SetLooperName();
            loopers.Add(newLooper);
        }
        else
        {
            ShowErrorMessage("Maximum number of loopers has been reached.");
        }
        
    }


    /// <summary>
    /// Saves the current session
    /// </summary>
    public void SaveData()
    {

        //check if we can save
        if (loopers.Count == 0 || bRecordingALoop) { return; }
        foreach (Looper looper in loopers)
        {
            if (looper.bIsPlaying) {
                ShowErrorMessage("Cannot save session when a looper is playing.");
                return; 
            }
        }
        //create save file name
        string date;
        if (!string.IsNullOrWhiteSpace(sessionTitle.text))
        {
            date = $"{sessionTitle.text}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}";
        }
        else
        {
            date = $"session_{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}";
        }
        
        string savedLoopsFolderPath = Application.persistentDataPath + "/SavedLoops";
        //create the saved loops folder if it does not exist
        if (!File.Exists(savedLoopsFolderPath))
        {
            Directory.CreateDirectory(savedLoopsFolderPath);
        }
        string saveFolderPath = $"{savedLoopsFolderPath}/{date}";
        Directory.CreateDirectory(saveFolderPath);




        //write bpm info
        string infoFilePath = saveFolderPath + "/info.txt";
        if (!File.Exists(infoFilePath))
        {
            File.WriteAllText(infoFilePath, "");
        }
        File.AppendAllText(infoFilePath, ((int)bpm).ToString() + "\n");
        File.AppendAllText(infoFilePath, signatureHi.ToString() + "\n");
        for (int i = 0; i < loopers.Count; i++)
        {
            if (loopers[i].bHasClip)
            {
                //write looper info
                string looperInfo = loopers[i].looperName + "::"
                    + ((int)loopers[i].recordedBPM).ToString() + ";;"
                    + ((int)loopers[i].numberOfMeasures).ToString() + ",,"
                    + loopers[i].recordedSignatureHi.ToString() + "\n";
                File.AppendAllText(infoFilePath, looperInfo);
            }
        }

        //convert each looper's clip to a WAV file in case user wants to export loop
        for (int i = 0; i < loopers.Count; i++)
        {
            if (loopers[i].bHasClip)
            {
                AudioClip clip = loopers[i].GetComponent<AudioSource>().clip;
                string path = saveFolderPath + "/" + loopers[i].looperName + ".wav";
                SavWav.Save(path, clip);
            }
            
        }
        ShowErrorMessage("Saved Session as: " + date);
        
        

    }

    /// <summary>
    /// Loads a saved session
    /// </summary>
    public void LoadData()
    {
        DisableLoadSessionMenu();
        //check if we can load a seesion
        foreach (Looper looper in loopers)
        {
            if (looper.bIsPlaying) {
                ShowErrorMessage("Cannot load session when a looper is playing.");
                return; 
            }
        }
        //return if there are no saved files
        string savedLoopsFolderPath = Application.persistentDataPath + "/SavedLoops";      
        if (!Directory.Exists(savedLoopsFolderPath)) {
            ShowErrorMessage("There is not a saved loops folder to access.");
            return; 
        }
        RefreshSavedFolderContent();
        loadSessionMenu.SetActive(true);

    }

    /// <summary>
    /// Updates the list of possible saved sessions to load
    /// </summary>
    public void RefreshSavedFolderContent()
    {
        //first, delete the currently displayed list of saved sessions
        string savedLoopsFolderPath = Application.persistentDataPath + "/SavedLoops";
        foreach (Transform child in savedFolderContent)
        {
            Destroy(child.gameObject);
        }
        loadSessionMenu.SetActive(true);
        try
        {
            //add option for user to back out of loading a saved session
            GameObject backOption = Instantiate(savedSessionPrefab, savedFolderContent);
            backOption.GetComponentInChildren<TextMeshProUGUI>().text = "Back";
            backOption.GetComponent<Button>().onClick.AddListener(DisableLoadSessionMenu);

            //find and display each saved session
            string[] dir = Directory.GetDirectories(savedLoopsFolderPath);
            for (int i = 0; i < dir.Length; i++)
            {
                GameObject savedSession = Instantiate(savedSessionPrefab, savedFolderContent);
                savedSession.GetComponentInChildren<TextMeshProUGUI>().text = dir[i].Substring(dir[i].LastIndexOf("\\") + 1);
                int value = i;
                savedSession.GetComponent<Button>().onClick.AddListener(delegate { LoadPreviousSession(value); });
            }
            //add option for user to refresh the list of saved sessions
            GameObject refreshOption = Instantiate(savedSessionPrefab, savedFolderContent);
            refreshOption.GetComponentInChildren<TextMeshProUGUI>().text = "Refresh";
            refreshOption.GetComponent<Button>().onClick.AddListener(RefreshSavedFolderContent);

        }
        catch (Exception e)
        {
            ShowErrorMessage("Exception: " + e.Message);
        }
    }

    /// <summary>
    /// Loads a previous session given the session's index in the list of saved sessions
    /// </summary>
    /// <param name="sessionIndex">index of session in the list of saved sessions</param>
    public void LoadPreviousSession(int sessionIndex)
    {
        string savedLoopsFolderPath = Application.persistentDataPath + "/SavedLoops";
        if (!Directory.Exists(savedLoopsFolderPath)) {
            DisableLoadSessionMenu();
            ShowErrorMessage("No saved loops available");
            return; 
        }
        try
        {
            string[] dir = Directory.GetDirectories(savedLoopsFolderPath);
            if (sessionIndex < dir.Length)
            {
                if (!Directory.Exists(dir[sessionIndex]))
                {
                    return;
                }
                //delete all loopers in the current session
                foreach (Looper looper in loopers)
                {
                    looper.ClearLoop();
                    Destroy(looper.gameObject);
                }
                loopers.Clear();
                //read the info about the session to load
                StreamReader sr = new StreamReader(dir[sessionIndex] + "/info.txt");
                List<string> sessionInfo = new List<string>();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sessionInfo.Add(line);
                }
                double newBPM = Convert.ToDouble(sessionInfo[0]);
                int newSigHigh = Convert.ToInt32(sessionInfo[1]);
                for (int i = 2; i < sessionInfo.Count;i++)
                {
                    //create looper

                    string loopName = sessionInfo[i].Substring(0, sessionInfo[i].IndexOf("::"));
                    double loopBPM = Convert.ToDouble(sessionInfo[i].Substring(sessionInfo[i].IndexOf("::") + 2,sessionInfo[i].IndexOf(";;") - (sessionInfo[i].IndexOf("::") + 2)));
                    double loopMeasures = Convert.ToDouble(sessionInfo[i].Substring(sessionInfo[i].IndexOf(";;") + 2, sessionInfo[i].IndexOf(",,") - (sessionInfo[i].IndexOf(";;") + 2)));
                    int loopSigHi = Convert.ToInt32(sessionInfo[i].Substring(sessionInfo[i].IndexOf(",,") + 2));
                    Looper newLooper = Instantiate(looperPrefab, backgroundPanel).GetComponent<Looper>();
                    newLooper.looperName = loopName;
                    newLooper.recordedBPM = loopBPM;
                    newLooper.numberOfMeasures = loopMeasures;
                    newLooper.recordedSignatureHi = loopSigHi;
                    newLooper.SetLooperName();
                    loopers.Add(newLooper);
                }
                //for each saved clip, convert the WAV file back to an audio clip and apply it to a looper
                string[] files = Directory.GetFiles(dir[sessionIndex],"*wav");
                for (int i = 0; i < files.Length;i++)
                {
                    StartCoroutine(LoadAudio(files[i], i));

                }
                ChangeTimeSignature(newSigHigh);
                ChangeBPM(newBPM);

            }
            DisableLoadSessionMenu();

        }
        catch (Exception e)
        {
            ShowErrorMessage("Exception: " + e.Message);
            
        }
    }




    /// <summary>
    /// Converts WAV file to audio clip and applies the clip to a looper
    /// </summary>
    /// <param name="url">the path to the WAV file</param>
    /// <param name="looperIndex">the index of the looper to apply the clip to</param>
    /// <returns></returns>
    private IEnumerator LoadAudio(string url, int looperIndex)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return www.SendWebRequest();


            AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
            if (myClip == null)
            {
                ShowErrorMessage("Failed to load clip");               
            }
            else
            {
                myClip.name = loopers[looperIndex].looperName;
                loopers[looperIndex].SetAudioClip(myClip);
            }         
        }
    }


    /// <summary>
    /// Hides the saved sessions menu
    /// </summary>
    public void DisableLoadSessionMenu()
    {
        loadSessionMenu.SetActive(false);
    }

    /// <summary>
    /// Displays a message at the bottom of the screen
    /// </summary>
    /// <param name="message"></param>
    public void ShowErrorMessage(string message)
    {
        
        errorMessage.text = message;
        errorMessage.gameObject.SetActive(true);
        if (errorMessageRoutineObject != null)
        {
            StopCoroutine(errorMessageRoutineObject);
        }
        errorMessageRoutineObject = StartCoroutine(ErrorMessageRoutine());
        
    }
    /// <summary>
    /// Hides error message after 4 seconds
    /// </summary>
    /// <returns></returns>
    IEnumerator ErrorMessageRoutine()
    {
        yield return new WaitForSeconds(4);
        errorMessage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Plays all paused loopers at the next down beat
    /// </summary>
    public void PlayAllLoopers()
    {
        foreach (Looper looper in loopers)
        {
            if (looper.bHasClip && looper.bPaused)
            {
                looper.Playback();
            }
        }
    }


    public void PlayLoopersFromPlayState()
    {
        if(looperState!=null && looperState.Length>0)
        {
            for(int i=0; i<looperState.Length; i++)
            {
                if(looperState[i] == 1)
                {
                    loopers[i].Playback();
                }
            }

            Debug.Log("Looper State Restored!");
        }

        
    }

    /// <summary>
    /// Stops all playing loopers at the next down beat
    /// </summary>
    public void StopAllLoopers()
    {

        //SaveLooperPlayState();

        foreach (Looper looper in loopers)
        {
            if (looper.bHasClip && !looper.bPaused)
            {
                //we first can save the
                //state of the loopers playing, and then stop, so that we may play them back again from the state array
                //using the PlayLoopersFromPlayState function!
                looperState[loopers.IndexOf(looper)] = 1;


                looper.Playback();
            }
        }
    }

    public void InitialiseLooperWithClips(List<AudioClip> audioClips)
    {
        InitialiseLooperWithClips(audioClips.ToArray());
    }


    public void InitialiseLooperWithClips(AudioClip[] audioClips)
    {
        if(samplePackManager)
        {
            if(loopers!=null)
            {
               for(int i=0; i<loopers.Count; i++)
                {

                    if (i < audioClips.Length)
                        loopers[i].SetAudioClip(audioClips[i]);
                    else
                        Debug.Log("Looper " + i + " has no clip assigned (No more clips to assign)");
                }
            }
        }
    }

    public void LoadSamplesFromSamplePackManager()
    {
        Debug.Log("Loading samples from selected sample pack");
        if (samplePackManager)
        {
            if (samplePackManager._audioSamples != null)
            {
                AudioClip[] clipsAray = samplePackManager._audioSamples.ToArray();
                InitialiseLooperWithClips(clipsAray);
            }
        }
    }


    //When we call StopAllLoopers(), we first can save the 
    //state of the loopers playing, and then stop, so that we may play them back again from the state array
    //using the PlayLoopersFromPlayState function!
    public void SaveLooperPlayState()
    {
        if(loopers!=null && loopers.Count>0)
        {
            for(int i = 0; i<loopers.Count; i++)
            {

                looperState[i] = 0;

                if (loopers[i].bIsPlaying)
                    looperState[i] = 1;
            }

            Debug.Log("Looper State Saved!");
        }
    }
}
