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
    Image displaySprite; // Assign a UI Image component in the Inspector to display the loaded sprite


    [SerializeField]
    TextMeshProUGUI _labelText;

    private Texture2D loadedTexture;
    public Sprite imageSprite;

    public void LoadImageByLabel(string label)
    {
        this.label = label;
        _labelText.text = label;

        Addressables.LoadAssetAsync<Texture2D>(label).Completed += OnImageLoaded;

        //Addressables.LoadAssetAsync<Sprite>(label).Completed += OnImageLoaded;
    }

    private void OnImageLoaded(AsyncOperationHandle<Texture2D> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedTexture = TextureConverter.CopyTexture2D(handle.Result);
            if (displayImage != null)
            {
                Debug.Log($"Texture found for {label}");
                displayImage.texture = loadedTexture; // Display the image in a UI component

                //imageSprite = TextureConverter.ConvertToSprite(displayImage.texture as Texture2D);
            }
        }
        else
        {
            Debug.LogError("Failed to load the image for label: " + label);
        }
        // Always remember to release the handle when you're done
        Addressables.Release(handle);
    }

    private void OnImageLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite sprite = handle.Result;
            if (displayImage != null)
            {
                Debug.Log($"Sprite found for {label}");
                displaySprite.sprite = sprite; // Display the image in a UI component
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
