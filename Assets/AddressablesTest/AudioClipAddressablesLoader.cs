using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AudioClipAddressablesLoader : MonoBehaviour
{
    [SerializeField]
    string _groupName;

    List<string> _trackAddresses = new List<string>();
    Dictionary<string, AudioClip> _audioClipsDictionary = new Dictionary<string, AudioClip>();

    public List<AudioClip> _audioClipsList = new List<AudioClip>();

    bool loadedAllClips = false;

    public UnityEvent<List<AudioClip>> OnAudioClipsLoaded;

    //// Start is called before the first frame update
    //void Start()
    //{
    //    StartCoroutine(InitializeAddressables(_groupName));
    //}


    public IEnumerator InitializeAddressables(string _groupName)
    {
        Debug.Log("initializing Addressables from " + _groupName);
        var groupHandle = Addressables.LoadResourceLocationsAsync(_groupName);

        while(!groupHandle.IsDone)
        {
            yield return null;
        }

        GetTrackAddresses(groupHandle);
        InitializeAudioClips();

        while(!loadedAllClips)
        {
            yield return null;
        }

    }

    void GetTrackAddresses(AsyncOperationHandle groupHandle)
    {
        Debug.Log("Getting track addresses for " + _groupName);

        if (groupHandle.Result is not IEnumerable locations) return;
        _trackAddresses.Clear();
        foreach(var location in locations)
        {
            _trackAddresses.Add(location.ToString());
            Debug.Log("Found Track at " + location.ToString());
        }
    }

    void InitializeAudioClips()
    {
        Debug.Log($"Initializing audio clips for {_groupName}");
        _audioClipsList.Clear();
        _audioClipsDictionary.Clear();

        int trackCount = 0;

        List<string> audioPathList = _trackAddresses.Where(x => x.Contains(".mp3")).ToList();

        foreach(var trackAddress in audioPathList)
        {
                Addressables.LoadAssetAsync<AudioClip>(trackAddress).Completed += operation =>
                {
                    if (operation.Status != AsyncOperationStatus.Succeeded) return;
                    var clip = operation.Result;
                    _audioClipsDictionary.Add(trackAddress, clip);
                    _audioClipsList.Add(clip);
                    trackCount += 1;
                    Debug.Log($"Loaded {_groupName} {clip.name} for trackAddress {trackAddress}");
                    Debug.Log("Track Count: " + trackCount + " " + audioPathList.Count);

                    if(trackCount == audioPathList.Count)
                    {
                        loadedAllClips = true;
                        OnAudioClipsLoaded.Invoke(_audioClipsList);
                        Debug.Log($"{_audioClipsList.Count} tracks are in the _audioClipsList");
                    }
                };
        }
    }
}
