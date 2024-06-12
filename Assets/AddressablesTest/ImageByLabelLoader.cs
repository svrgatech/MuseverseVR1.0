using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI; // Include if you want to display the image in a UI Image component
using TMPro;

public class ImageByLabelLoader : MonoBehaviour
{
    public string label = "yourImageLabel"; // Set this to the label of the image you want to load

    [SerializeField]
    RawImage displayImage; // Assign a UI Image component in the Inspector to display the loaded image
    [SerializeField]
    TextMeshProUGUI _labelText;

    public void LoadImageByLabel(string label)
    {
        this.label = label;
        _labelText.text = label;

        Addressables.LoadAssetAsync<Texture>(label).Completed += OnImageLoaded;
    }

    private void OnImageLoaded(AsyncOperationHandle<Texture> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Texture image = handle.Result;
            if (displayImage != null)
            {
                Debug.Log($"Texture found for {label}");
                displayImage.texture = image; // Display the image in a UI component
            }
        }
        else
        {
            Debug.LogError("Failed to load the image for label: " + label);
        }
        // Always remember to release the handle when you're done
        Addressables.Release(handle);
    }
}
