using System.Collections;
using System.Collections.Generic;
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

        foreach(var trackAddress in _trackAddresses)
        {

            int otherFileCount = 0;
            //A HACK TO MAKE SURE THE PNG IMAGE THUMBNAIL IS NOT LOADED AS AN AUDIOCLIP
            //MAKE BETTER!
            if(!trackAddress.Contains(".png"))
            {
                Addressables.LoadAssetAsync<AudioClip>(trackAddress).Completed += operation =>
                {
                    if (operation.Status != AsyncOperationStatus.Succeeded) return;
                    var clip = operation.Result;
                    _audioClipsDictionary.Add(trackAddress, clip);
                    _audioClipsList.Add(clip);
                    trackCount += 1;
                    Debug.Log($"Loaded {_groupName} {clip.name} for trackAddress {trackAddress}");
                    Debug.Log("Track Count: " + trackCount + " " + (_trackAddresses.Count - otherFileCount));

                    if(trackCount == (_trackAddresses.Count - otherFileCount))
                    {
                        loadedAllClips = true;
                        OnAudioClipsLoaded.Invoke(_audioClipsList);
                    }

                };
            }
            else
            {
                _trackAddresses.Remove(trackAddress);
            }
           
        }

       
    }
}
