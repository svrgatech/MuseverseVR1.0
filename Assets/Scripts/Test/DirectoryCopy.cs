using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DirectoryCopy : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CopyStreamingAssetsToPersistentData());
    }

    IEnumerator CopyStreamingAssetsToPersistentData()
    {
        string sourceDirectory = Path.Combine(Application.streamingAssetsPath, "MuseverseSamplePacks");
        string targetDirectory = Path.Combine(Application.persistentDataPath, "MuseverseSamplePacks");

        // Ensure the target directory exists
        Directory.CreateDirectory(targetDirectory);

        // Get all files from the source directory
        string uri = sourceDirectory + "/"; // Adding a slash to indicate it's a directory
        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(uri))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error loading directory: " + www.error);
            }
            else
            {
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                if (bundle != null)
                {
                    var paths = bundle.GetAllAssetNames();

                    foreach (var path in paths)
                    {
                        StartCoroutine(CopyFile(path, Path.Combine(targetDirectory, Path.GetFileName(path))));
                    }
                    bundle.Unload(false);
                }
            }
        }
    }

    IEnumerator CopyFile(string sourceFilePath, string targetFilePath)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(sourceFilePath))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Create a new file and write the contents of the downloaded file
                File.WriteAllBytes(targetFilePath, www.downloadHandler.data);
            }
            else
            {
                Debug.LogError("Failed to download file: " + www.error);
            }
        }
    }
}
