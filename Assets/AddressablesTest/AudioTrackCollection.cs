using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AudioClipCollection", menuName = "ScriptableObjects/AudioClipCollection", order = 1)]
public class AudioClipCollection : ScriptableObject
{
    [SerializeField]
    string _clipCollectionName;

    [SerializeField]
    List<AudioClip> _clips;
}
