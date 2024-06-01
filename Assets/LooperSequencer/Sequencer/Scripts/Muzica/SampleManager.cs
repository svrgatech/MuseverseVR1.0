using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class SampleManager : MonoBehaviour {

   #region Static Properties
   public static SampleManager Instance { get; set; }
   #endregion

   public enum OriginType {
      Default,
      Mix,             
      Mic,
      None
   }

   // TODO: change this to relate to intent of mixload
   // MixLoadIntent.Restore
   // MixLoadIntent.New
   public static bool inAsyncRestore = false;

   public int maxChannels = 16;
   public int numChannels = 8;
   public AudioClip[] soundClips;
   public OriginType[] soundOrigins;
   public string lastMixToken = null;

   private AudioSource[] soundSources;
   private GameObject[] buttonGOs;

   public SamplePackManager samplePackManager;

    private string[] defaultSoundAssetnames = new string[8] {
      "drums/1001",
      "drums/1222",
      "drums/1012",
      "drums/1003",
      "drums/1307",
      "drums/1308",
      "drums/1038",
      "drums/1078"
   };
   private Dictionary<string, object> localState = new Dictionary<string, object>();
   private string stateKey = "soundState";
   
   void Awake() {
      if (SampleManager.Instance == null)
         SampleManager.Instance = this;
   }

   // Use this for initialization
   void Start() {
      soundSources = new AudioSource[numChannels];
      soundOrigins = new OriginType[numChannels];
      buttonGOs = new GameObject[numChannels];

        if(soundClips!=null && soundClips.Length>0)
        {
            InitializeSequencerWithClips(soundClips);
        }
        else
        {
            Debug.Log("SoundClips are empty!");
        }

        // restore prev state of samples
        //RestoreState();


    }

    public void LoadSamplesFromSamplePackManager()
    {
        Debug.Log("Loading samples from selected sample pack");
        if(samplePackManager)
        {
           if(samplePackManager._audioSamples!=null)
            {
                AudioClip[] clipsAray = samplePackManager._audioSamples.ToArray();
                InitializeSequencerWithClips(clipsAray);
            }
        }
    }




    public void InitializeSequencerWithClips(AudioClip[] clips)
    {
        if (soundSources != null)
        {
            for (int i = 0; i < soundSources.Length; i++)
            {
                    Destroy(soundSources[i]);
            }

        }

        DisplayManagerMix.Instance.ClearSampleInfoItems();
      

        // initialize default audio sources
        for (int i = 0; i < numChannels; i++)
        {
            GameObject child = new GameObject("Sound" + i);
            child.transform.parent = gameObject.transform;
            soundSources[i] = child.AddComponent<AudioSource>();
            soundOrigins[i] = OriginType.None;

            //if the channel number is less than the number of clips available in the clips Array
            if (i<clips.Length)
            {
                soundSources[i].clip = clips[i];
            }
            //Add the sample Info Item on the left panel
            DisplayManagerMix.Instance.AddSampleInfoItem(((i<clips.Length) ? clips[i].name : "No Clip"), i);

        }
    }

    // initialize one default audio source
    private void SetSlotClip(int i, AudioClip clip)
    {
        if (defaultSoundAssetnames.Length >= (i + 1))
        {
            soundSources[i].clip = clip;
            soundOrigins[i] = OriginType.Default;
            SaveSlot(i, defaultSoundAssetnames[i], "default", defaultSoundAssetnames[i]);
        }
        else
        {
            // blank out the clip
            soundSources[i].clip = null;
            soundOrigins[i] = OriginType.None;
        }
    }


    // public method to revert to default sample state
    public void ResetSamples() {
      SetStateFromDefault();
   }

   // initialize default audio sources for all
   private void SetStateFromDefault() {
      for (int i = 0; i < numChannels; i++) {
         SetSlotFromDefault(i);
      }
   }

   // initialize one default audio source
   private void SetSlotFromDefault(int i) {
      if (defaultSoundAssetnames.Length >= (i + 1)) {
         soundSources[i].clip = Resources.Load(defaultSoundAssetnames[i], typeof(AudioClip)) as AudioClip;
         soundOrigins[i] = OriginType.Default;
         SaveSlot(i, defaultSoundAssetnames[i], "default", defaultSoundAssetnames[i]);
      }
      else {
         // blank out the clip
         soundSources[i].clip = null;
         soundOrigins[i] = OriginType.None;
      }
   }

   private void RestoreState() {
      if (String.IsNullOrEmpty(PlayerPrefs.GetString(stateKey))) {
         SetStateFromDefault();
         return;
      }

      RestoreStateSync();
   }

   // restore audio clips from available sources
   private void RestoreStateSync() {
      var stateInfo = MiniJSON.Json.Deserialize(PlayerPrefs.GetString(stateKey)) as Dictionary<string, object>;
      for (int i = 0; i < numChannels; i++) {
         if (stateInfo.ContainsKey(i.ToString()) == false) {
            SetSlotFromDefault(i);
            continue;
         }
         
         var tmpSlot = stateInfo[i.ToString()] as Dictionary<string, object>;
         var tmpType = tmpSlot["type"] as string;
         var tmpSrc = tmpSlot["src"] as string;
         
         localState[i.ToString()] = tmpSlot;
         
         if (tmpType == "default") {
            SetSlotFromDefault(i);
         }
      }
      inAsyncRestore = false;
   }

   private void SaveSlot(int i, string name, string type, string src) {
      var slot = new Dictionary<string, object>();
      slot.Add("name", name);
      slot.Add("type", type);
      slot.Add("src", src);
      localState[i.ToString()] = slot;
      SaveState(localState);
   }
   
   private void SaveState(Dictionary<string, object> stateObj) {
      PlayerPrefs.SetString(stateKey, MiniJSON.Json.Serialize(stateObj));
   }

   public Dictionary<string, object> GetState() {
      var stateInfo = MiniJSON.Json.Deserialize(PlayerPrefs.GetString(stateKey)) as Dictionary<string, object>;
      return stateInfo;
   }

  
   // wrapper for putting url/file ref into a channel
   public void PlaySample(int channel, double playTime) {
      soundSources[channel].PlayScheduled(playTime);
   }

   public void PlayDelayed(int channel, float delay) {
      soundSources[channel].PlayDelayed(delay);
   }

   public void StopSample(int channel) {
      soundSources[channel].Stop();
   }

   public void StopAll() {
      for (int i = 0; i < numChannels; i++) {
         soundSources[i].Stop();
      }
   }
}
