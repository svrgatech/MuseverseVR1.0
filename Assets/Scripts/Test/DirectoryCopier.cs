using System.IO;
using UnityEngine;

public class DirectoryCopier : MonoBehaviour
{
    //void Start()
    //{
    //    string sourceDirectory = Path.Combine(Application.streamingAssetsPath, "MuseverseSamplePacks");
    //    string targetDirectory = Path.Combine(Application.persistentDataPath, "MuseverseSamplePacks");

    //    // Start the copying process
    //    CopyDirectory(sourceDirectory, targetDirectory);
    //}

    public static void CopyDirectory(string sourceDir, string destinationDir)
    {
        // Create the destination directory if it doesn't exist
        Directory.CreateDirectory(destinationDir);

        Debug.Log("Destination Directory: " + destinationDir);

        // Copy each file into the new directory
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, true); // overwrite if file already exists
            Debug.Log("Copying file: " + file + " to " + destFile);
        }

        // Copy each subdirectory using recursion
        foreach (string directory in Directory.GetDirectories(sourceDir))
        {
            string destDirectory = Path.Combine(destinationDir, Path.GetFileName(directory));
            CopyDirectory(directory, destDirectory);
        }
    }
}
