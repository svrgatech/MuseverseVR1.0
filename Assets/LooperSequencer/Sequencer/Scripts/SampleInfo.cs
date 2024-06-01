using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SampleInfo : MonoBehaviour
{

    public string sampleName;
    public int channelNum;

    //UI
    public TextMeshProUGUI sampleNameText;
    
    public void SetSampleInfo(string name, int channel)
    {
        sampleName = name;
        channelNum = channel;

        if(sampleNameText)
        {
            sampleNameText.text = name;
        }

    }

}
