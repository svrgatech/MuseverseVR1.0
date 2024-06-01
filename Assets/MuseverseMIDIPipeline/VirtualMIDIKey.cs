using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualMIDIKey : MonoBehaviour
{
    public VirtualMIDIInstrument virtualMIDIInstrument;
    //Corresponds to the Midi Number (0-127)
    [SerializeField]
    public int midiNumber;

    //Visuals for the Key. Implementing a simple material switch for now
    MeshRenderer meshRenderer;
    [SerializeField]
    Material defaultMat;
    [SerializeField]
    Material triggeredMat;


    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if(meshRenderer)
        {
            meshRenderer.sharedMaterial = defaultMat;
        }
    }

    public void TriggerKey()
    {
        if (virtualMIDIInstrument)
        {
            virtualMIDIInstrument.RaiseMIDIEvent(midiNumber);
        }

        if (meshRenderer)
        {
            meshRenderer.sharedMaterial = triggeredMat;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "VirtualInstrumentTrig")
        {
            TriggerKey();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("VirtualInstrumentTrig"))
        {
            if (virtualMIDIInstrument)
            {
                if (meshRenderer)
                {
                    meshRenderer.sharedMaterial = defaultMat;
                }
            }
        }
    }

}
