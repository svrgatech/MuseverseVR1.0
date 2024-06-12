using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class AddressablesSamplePackManager : MonoBehaviour
{

    public List<string> _samplePackNames;

    public GameObject menuItemPrefab;

    [SerializeField]
    AudioClipAddressablesLoader audioClipAddressablesLoader;

    [SerializeField]
    Transform menuParent;


    public void Start()
    {
        BuildMenu(_samplePackNames);
    }

    public void BuildMenu(List<string> samplePackNames)
    {
        foreach(string samplePackName in samplePackNames)
        {
            GameObject menuItem = Instantiate(menuItemPrefab, menuParent);
            if(menuItem)
            {
                ImageByLabelLoader imageLoader = menuItem.GetComponent<ImageByLabelLoader>();
                imageLoader.label = samplePackName;
                imageLoader.LoadImageByLabel(samplePackName);
                menuItem.GetComponent<Button>().onClick.AddListener(() => InitializeAddressableAudioClips(samplePackName));
            }
        }
    }

    public void InitializeAddressableAudioClips(string samplePackName)
    {
        if(audioClipAddressablesLoader)
        {
            StartCoroutine(audioClipAddressablesLoader.InitializeAddressables(samplePackName));
        }
    }


   

}
