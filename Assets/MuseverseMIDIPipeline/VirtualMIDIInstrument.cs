using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MPTK;
using MidiPlayerTK;

public class VirtualMIDIInstrument : MonoBehaviour
{

    public MidiStreamPlayer midiStreamPlayer;

    public int midiStartNumber;

    public int CurrentPreset = 0;
    public int StreamChannel;

    //the Virtual Midi Keys that are a part of this Instrument
    [SerializeField]
    private VirtualMIDIKey[] virtualMIDIKeys;

    // Start is called before the first frame update
    void Awake()
    {

        //Assign virtual midi key's instrument reference to this instance of VirtualMidiInstrument
        if(virtualMIDIKeys!=null && virtualMIDIKeys.Length>0)
        {
            for(int i=0; i<virtualMIDIKeys.Length; i++)
            {
                virtualMIDIKeys[i].virtualMIDIInstrument = this;

                //Assign the Midi number to each virtual key, based on the Midi Start Number
                //The keys must be populated in the Inspector IN ORDER of their semitones
                virtualMIDIKeys[i].midiNumber = i + midiStartNumber;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            TestMidiEvent();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (Input.GetKeyDown(KeyCode.DownArrow)) CurrentPreset--;
            if (Input.GetKeyDown(KeyCode.UpArrow)) CurrentPreset++;
            CurrentPreset = Mathf.Clamp(CurrentPreset, 0, 127);

            // Send the patch (other name for preset) change MIDI event to the MIDI synth
            midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent()
            {
                Command = MPTKCommand.PatchChange,
                Value = CurrentPreset,   // From 0 to 127
                Channel = StreamChannel, // From 0 to 15
            });
        }
    }

    public void RaiseMIDIEvent(int midiNumber, int velocity = 100, long duration = 1000)
    {
        var NotePlaying = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOn, // midi command
            Value = midiNumber, // from 0 to 127, 48 for C4, 60 for C5, ...
            Channel = 0, // from 0 to 15, 9 reserved for drum
            Duration = duration, // note duration in millisecond, -1 to play undefinitely, MPTK_StopChord to stop
            Velocity = velocity, // from 0 to 127, sound can vary depending on the velocity
            Delay = 0, // delay in millisecond before playing the note
        };
        midiStreamPlayer.MPTK_PlayEvent(NotePlaying);
    }


    public void TestMidiEvent()
    {
        var NotePlaying = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOn, // midi command
            Value = 48, // from 0 to 127, 48 for C4, 60 for C5, ...
            Channel = 2, // from 0 to 15, 9 reserved for drum
            Duration = 1000, // note duration in millisecond, -1 to play undefinitely, MPTK_StopChord to stop
            Velocity = 100, // from 0 to 127, sound can vary depending on the velocity
            Delay = 0, // delay in millisecond before playing the note
        };
        midiStreamPlayer.MPTK_PlayEvent(NotePlaying);
    }

    //public void AssignMIDINumbersToKeys(int start)
    //{
    //    if(virtualMIDIKeys!=null && virtualMIDIKeys.Length>0)
    //    {
    //        for(int i = 0; i<virtualMIDIKeys.Length; i++)
    //        {
    //            virtualMIDIKeys[i].midiNumber = start + i;
    //        }
    //    }
    //}

}
