using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Threading.Tasks;

public class SamplePackManager : MonoBehaviour
{


    public List<AudioClip> _audioSamples;

    public string samplePacksRootFolder;

    public string samplePackFolder;

    public bool autoLoad;

    public string[] samplePacksArray;
    public string[] audioSamplesArray;
    List<string> samplePacksList;
    List<string> audioSamplesList;

    public string[] samplePacksSearchResultArray;
    public string[] audioSamplesSearchResultArray;

    public string presentSelectedSamplePath;
    public AudioClip presentSelectedSample;
    
    [Space(10)]
    [Header("UI Elements for Menus")]
    public GameObject samplePackMenu;
    public GameObject samplePackMenuContainer;
    public GameObject samplePackMenuItemTemplate;
    [Space(10)]
    public GameObject samplePackClipsMenu;
    public GameObject samplePackClipsMenuContainer;
    public GameObject samplePackClipsMenuItemTemplate;
    public AudioSource previewSampleAudioSource;

    public UnityEvent OnSampleClipsLoaded;



    //UI For presetPack Loading
    public TMP_Dropdown foldersDropdown;

    // Start is called before the first frame update
    void Start()
    {

#if UNITY_ANDROID

        //RequestPermissions();

        ////Test Local Android Copy
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "MuseverseSamplePacks");
        string targetPath = Path.Combine(Application.persistentDataPath, "MuseverseSamplePacks");

        ////DirectoryCopier.CopyDirectory(sourcePath, targetPath);

        samplePacksRootFolder = targetPath;

        ListFoldersInRoot(samplePacksRootFolder);
        BuildSamplePackMenu(samplePacksArray, autoLoad);

#else

        ListFoldersInRoot(samplePacksRootFolder);
        BuildSamplePackMenu(samplePacksArray, autoLoad);
#endif

 
        //Testing
        //LoadAudioClipsFromFolder(@"E:\MuseverseSamplePacks\DrumsPack1");
        
    }

    void RequestPermissions()
    {
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
        }
    }

    public void ListFoldersInRoot(string samplePacksRootFolder)
    {
        if (string.IsNullOrEmpty(samplePacksRootFolder))
        {
            Debug.LogError("Sample packs root folder path is not set.");
            return;
        }

        try
        {
            // Get all directories (folders) in the root folder
            string[] folders = Directory.GetDirectories(samplePacksRootFolder);

            if (folders.Length > 0)
            {

                samplePacksArray = new string[folders.Length];
                Debug.Log("Folders in the sample pack root folder:" + folders.Length);

                for (int i = 0; i < folders.Length; i++)
                {
                    // Display the folder name
                    samplePacksArray[i] = Path.GetFileName(folders[i]);
                }

                samplePacksList = samplePacksArray.ToList();
                
                //To be removed later. this was made only for the Futuristic Drums UI
                //Can be used for later also.
                if (foldersDropdown != null)
                {
                    foldersDropdown.AddOptions(samplePacksArray.ToList());
                }
            }
            else
            {
                Debug.Log("No folders found in the sample pack root folder.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error listing folders: {e.Message}");
        }
    }

    public void BuildSamplePackMenu(string[] samplePacksArray, bool autoLoad = false)
    {
        samplePackClipsMenu.SetActive(false);
        samplePackMenu.SetActive(true);

        RectTransform[] rectTransforms = samplePackMenuContainer.GetComponentsInChildren<RectTransform>();
        //skip the first child since it is the template and delete the rest to clear the menu
        for(int i = 1; i<rectTransforms.Length; i++)
        {
            Destroy(rectTransforms[i].gameObject);
        }

        if (samplePackMenu!=null && samplePacksArray!=null)
        {
            for (int i=0; i<samplePacksArray.Length; i++)
            {
               GameObject menuItem = Instantiate(samplePackMenuItemTemplate, samplePackMenuContainer.transform);
               menuItem.GetComponentInChildren<TextMeshProUGUI>().text = samplePacksArray[i];
               menuItem.name = samplePacksArray[i];

               string thumbNailPath = FindThumbnailPNG(samplePacksArray[i]);
                if(!string.IsNullOrEmpty(thumbNailPath))
                {
                    menuItem.GetComponent<Image>().sprite = LoadSprite(thumbNailPath);
                }

                //If it is autoload mode, we dont open the sample browser, but directly set the Sample Pack to
                //load up on the loopers when it is clicked. else, load the sample browser so we can add them individually
                menuItem.GetComponent<Button>().onClick.AddListener(() => LoadAudioClipsFromFolder(menuItem.name, autoLoad));
              
               menuItem.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Error loading Sample Packs. check samplePackMenu GO or samplePacksArray");
        }
    }

    public void BuildSamplePackClipsMenu(string[] audioFiles)
    {

        samplePackClipsMenu.SetActive(true);

        RectTransform[] rectTransforms = samplePackClipsMenuContainer.GetComponentsInChildren<RectTransform>();
        //skip the first child since it is the template and delete the rest
        for (int i = 1; i < rectTransforms.Length; i++)
        {
            Destroy(rectTransforms[i].gameObject);
        }

        for (int i = 0; i < audioFiles.Length; i++)
        {
            GameObject menuItem = Instantiate(samplePackClipsMenuItemTemplate, samplePackClipsMenuContainer.transform);
            menuItem.GetComponentInChildren<TextMeshProUGUI>().text = Path.GetFileName(audioFiles[i]);
            string path = audioFiles[i];
            menuItem.GetComponent<AudioSampleMetadata>().path = path;
            menuItem.GetComponent<Button>().onClick.AddListener(() => PreviewAudioSample(path));
            menuItem.name = Path.GetFileName(path);
            menuItem.SetActive(true);
        }
    }

    public void OnSamplePackSearchQuery(string Query)
    {
        if(string.IsNullOrEmpty(Query))
        {
            //revert to original samplePacksArray
            BuildSamplePackMenu(samplePacksArray, autoLoad);
        }
        else
        {
            samplePacksSearchResultArray = samplePacksList.Where(x => x.ToLower().Contains(Query.ToLower())).ToArray();
            BuildSamplePackMenu(samplePacksSearchResultArray, autoLoad);
        }
    }

    public void OnAudioSamplesSearchQuery(string Query)
    {
        if (string.IsNullOrEmpty(Query))
        {
            //revert to original audioSamplesArray
            BuildSamplePackClipsMenu(audioSamplesArray);
        }
        else
        {
            audioSamplesSearchResultArray = audioSamplesList.Where(x => x.ToLower().Contains(Query.ToLower())).ToArray();
            BuildSamplePackClipsMenu(audioSamplesSearchResultArray);
        }
    }



    // Function to list the number of audio clips in a folder and populate them in the _audioSamples list
    public async void LoadAudioClipsFromFolder(string folderName, bool autoLoad = false)
    {
        string folderPath = Path.Combine(samplePacksRootFolder, folderName);
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("Folder path is not provided.");
            return;
        }

        try
        {
            // Get all audio files in the specified folder
            string[] audioFiles = Directory.GetFiles(folderPath, "*.mp3"); // Adjust the file extension as needed

            if (audioFiles.Length > 0)
            {
                Debug.Log($"Audio clips in folder '{folderPath}': {audioFiles.Length}");

                audioSamplesArray = new string[audioSamplesArray.Length];
                audioSamplesArray = audioFiles;

                audioSamplesList = audioFiles.ToList();

                _audioSamples.Clear();

                for (int i = 0; i < audioFiles.Length; i++)
                {
                    AudioClip clip = await LoadAudioClipFromFileAsync(audioFiles[i]);
                    //StartCoroutine(LoadAudioClipFromFile(audioFiles[i]));
                }

                if (autoLoad)
                {
                 
                    samplePackMenu.GetComponentInParent<UIWindow>().CloseWindow();

                }
                //else just build the UI Menu for the audiosamples so we may assign them manually from the UI
                else
                {
                    BuildSamplePackClipsMenu(audioFiles);
                }
            }
            else
            {
                Debug.Log($"No audio clips found in folder '{folderPath}'.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading audio clips: {e.Message}");
        }
    }


  

    // Coroutine to load an audio clip from a file using UnityWebRequest
    public IEnumerator LoadAudioClipFromFile(string filePath, bool playPreview = true)
    {
        // Create a UnityWebRequest to load the audio clip
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            // Send the request and wait for it to complete
            yield return www.SendWebRequest();

            // Check for errors
            if (www.result == UnityWebRequest.Result.Success)
            {
                // Get the downloaded audio clip
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

                if (audioClip != null)
                {

                    presentSelectedSample = audioClip;

                   if(previewSampleAudioSource && playPreview)
                    {
                        if (previewSampleAudioSource.isPlaying) previewSampleAudioSource.Stop();

                        previewSampleAudioSource.PlayOneShot(audioClip);
                    }
                }
            }
            else
            {
                Debug.LogError($"Failed to load audio clip from file: {filePath}, Error: {www.error}");
            }
        }
    }

    public async Task<AudioClip> LoadAudioClipFromFileAsync(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            www.SendWebRequest().completed += _ => tcs.TrySetResult(true);

            await tcs.Task;

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                if (audioClip != null)
                {
                    audioClip.name = Path.GetFileName(filePath);
                    _audioSamples.Add(audioClip);

                    if (_audioSamples.Count == audioSamplesList.Count)
                        OnSampleClipsLoaded.Invoke();
                }
            }
            else
            {
                Debug.LogError($"Failed to load audio clip from file: {filePath}, Error: {www.error}");
            }
        }

        return null;
    }



#region Unused LoadClip Functions
    ////Load AudioClip to Looper
    //public IEnumerator LoadAudioClipFromFile(string filePath, Looper looper)
    //{
    //    // Create a UnityWebRequest to load the audio clip
    //    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
    //    {
    //        // Send the request and wait for it to complete
    //        yield return www.SendWebRequest();

    //        // Check for errors
    //        if (www.result == UnityWebRequest.Result.Success)
    //        {
    //            // Get the downloaded audio clip
    //            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

    //            if (audioClip != null)
    //            {

    //                looper.SetAudioClip(audioClip);

    //            }
    //        }
    //        else
    //        {
    //            Debug.LogError($"Failed to load audio clip from file: {filePath}, Error: {www.error}");
    //        }
    //    }
    //}

    //public IEnumerator LoadAudioClipFromFile(string filePath)
    //{
    //    // Create a UnityWebRequest to load the audio clip
    //    using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
    //    {
    //        // Send the request and wait for it to complete
    //        yield return www.SendWebRequest();

    //        // Check for errors
    //        if (www.result == UnityWebRequest.Result.Success)
    //        {
    //            // Get the downloaded audio clip
    //            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);

    //            if (audioClip != null)
    //            {
    //                audioClip.name = Path.GetFileName(filePath);
    //                _audioSamples.Add(audioClip);

    //                if (_audioSamples.Count == audioSamplesList.Count)
    //                    OnSampleClipsLoaded.Invoke();

    //            }
    //        }
    //        else
    //        {
    //            Debug.LogError($"Failed to load audio clip from file: {filePath}, Error: {www.error}");
    //        }
    //    }
    //}
#endregion

    public void PreviewAudioSample(string samplePath)
    {
        //StartCoroutine(LoadAudioClipFromFile(samplePath));
    }

    public string FindThumbnailPNG(string samplePackName)
    {
        DirectoryInfo dir = new DirectoryInfo(Path.Combine(samplePacksRootFolder, samplePackName));
        FileInfo[] files = dir.GetFiles("*.png");
        if (files.Length > 0)
        {
            string firstPNGFilePath = files[0].FullName;
            return firstPNGFilePath;
        }
        else
        {
            Debug.LogWarning("No PNG files found in the specified folder: " + Path.Combine(samplePacksRootFolder, samplePackName));
            return null;
        }
    }

    private Sprite LoadSprite(string path)
    {
        //Debug.Log(path);
        if (string.IsNullOrEmpty(path)) return null;
        if (File.Exists(path))
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(900, 900, TextureFormat.RGB24, false);
            texture.filterMode = FilterMode.Trilinear;
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.0f), 100.0f);
            return sprite;
        }
        return null;
    }




    public void OnDropdownValueChanged(int value)
    {
        string folderName = samplePacksArray[value];
        LoadAudioClipsFromFolder(folderName);
    }

    public void SwitchNextPresetInDropdown()
    {
        if (foldersDropdown)
        {
            foldersDropdown.value += 1;
        }
    }

    public void SwitchPreviousPresetInDropdown()
    {
        if (foldersDropdown && foldersDropdown.value > 0)
        {
            foldersDropdown.value -= 1;
        }
    }
}
