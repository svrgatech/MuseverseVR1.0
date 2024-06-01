using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;


public class DisplayManagerMix : MonoBehaviour
{

    #region Static Properties
    public static DisplayManagerMix Instance { get; set; }
    #endregion

    public enum PanelType
    {
        Sample,
        Mix,
        Share,
        Settings
    }

    public PanelType _panelType = PanelType.Sample;
    public GameObject _mixPanel;
    public GameObject _mixTempoField;
    public TMP_InputField _mixTempoInput;
    public GameObject _imageMixPlay;
    public GameObject _imageMixPause;
    public GameObject _ftueMixField;
    public InputField _ftueMixInput;

    public GameObject sampleInfoItemTemplate;
    public GameObject sampleInfoPanel;

    private string _ftueMixText = "Tap squares where you want sounds to play";

    private string _ftueResetText = "All sounds have been reset";

    void Awake()
    {
        if (DisplayManagerMix.Instance == null)
            DisplayManagerMix.Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        // main panels
        //_mixPanel = GameObject.Find("MixPanel");

        //// text fields to manipulate
        //_mixTempoField = GameObject.Find("MixTempoField");
        //if (_mixTempoField)
        //    _mixTempoInput = _mixTempoField.GetComponent<TMP_InputField>();

        //_ftueMixField = GameObject.Find("FtueMixField");
        //if (_ftueMixField)
        //    _ftueMixInput = _ftueMixField.GetComponent<InputField>();

        //_imageMixPlay = GameObject.Find("ImageMixPlay");
        //_imageMixPause = GameObject.Find("ImageMixPause");

        // now set the current
        SetPanels(_panelType);

        // TODO: bind any event listeners here
    }


    public void SetTempo(int tempo)
    {
        string tempoString = tempo.ToString();
        if(_mixTempoInput)
            _mixTempoInput.text = tempoString;
    }

    public void ClearSampleInfoItems()
    {
        if(sampleInfoPanel)
        {
            //Destroy all except the first child (That would be the ItemTemplate)
            for (int i = sampleInfoPanel.transform.childCount - 1; i >= 1; i--)
            {
                
                    Destroy(sampleInfoPanel.transform.GetChild(i).gameObject);
                
               
            }
        }
    }

    public void AddSampleInfoItem(string sampleName, int channel)
    {
        if(sampleInfoPanel && sampleInfoItemTemplate)
        {
            GameObject sampleInfoItem = Instantiate(sampleInfoItemTemplate, sampleInfoPanel.transform);
            sampleInfoItem.SetActive(true);
            sampleInfoItem.GetComponent<SampleInfo>().SetSampleInfo(sampleName, channel);
            
        }
    }

    public void TogglePanel(GameObject panel, bool state)
    {
        if (state == true)
        {
            panel.GetComponent<CanvasGroup>().alpha = 1f;
            panel.GetComponent<CanvasGroup>().interactable = true;
            panel.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        else
        {
            panel.GetComponent<CanvasGroup>().alpha = 0f;
            panel.GetComponent<CanvasGroup>().interactable = false;
            panel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void ToggleMixMode(string stateOn)
    {
        if (stateOn == "mix")
        {
            _ftueMixInput.text = _ftueMixText;
        }
    }

    public void ConveyReset()
    {
        if (_mixPanel)
            _ftueMixInput.text = _ftueResetText;
    }

    public void TogglePlayPause(string stateOn)
    {
        if (stateOn == "play")
        {
            _imageMixPlay.GetComponent<CanvasGroup>().alpha = 1f;
            _imageMixPlay.GetComponent<CanvasGroup>().interactable = true;
            _imageMixPlay.GetComponent<CanvasGroup>().blocksRaycasts = true;
            _imageMixPause.GetComponent<CanvasGroup>().alpha = 0f;
            _imageMixPause.GetComponent<CanvasGroup>().interactable = false;
            _imageMixPause.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        else
        {
            _imageMixPause.GetComponent<CanvasGroup>().alpha = 1f;
            _imageMixPause.GetComponent<CanvasGroup>().interactable = true;
            _imageMixPause.GetComponent<CanvasGroup>().blocksRaycasts = true;
            _imageMixPlay.GetComponent<CanvasGroup>().alpha = 0f;
            _imageMixPlay.GetComponent<CanvasGroup>().interactable = false;
            _imageMixPlay.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void SetPanels(PanelType panelType)
    {
        // blank all panels
        if (_mixPanel)
            TogglePanel(_mixPanel, false);

        // turn on the current one
        switch (panelType)
        {
            case PanelType.Mix:
                if (_mixPanel)
                {
                    TogglePanel(_mixPanel, true);
                    ToggleMixMode("mix");
                    // HACK: flaky if done from TileManager Start()
                    TileManager.Instance.RestoreState();
                }
                break;

            default:
                Debug.Log("Found unknown panel type");
                break;
        }

        // record current
        _panelType = panelType;
    }
}
