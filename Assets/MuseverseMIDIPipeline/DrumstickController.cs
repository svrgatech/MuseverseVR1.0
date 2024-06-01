using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumstickController : MonoBehaviour
{
    public float detectionDistance = 0.5f;

    void Update()
    {
        RaycastHit hit;
        // Cast a ray downward from the drumstick
        if (Physics.Raycast(transform.position, -transform.up, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("VirtualMidiKey"))
            {
                Debug.Log("Hit drum from above!");
                // Add logic to play sound, haptic feedback, etc.
                hit.collider.GetComponent<VirtualMIDIKey>().TriggerKey();
            }
        }
    }
}

