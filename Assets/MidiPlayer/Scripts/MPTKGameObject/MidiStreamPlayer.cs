﻿//#define DEBUGPERF
using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{
    /// <summary>
    /// Build and Play Real Time Music in relation with user actions or algorithms. This class must be used with the prefab MidiStreamPlayer.\n
    /// 
    /// @attention 
    ///     - MidiStreamPlayer inherits of class MidiSynth. For clarity, only MidiStreamPlayer attibutes are provided here. 
    ///       Look at the class MidiSynth to discover all attributes available.
    ///       
    /// No MIDI file is necessary, the notes are generated by your scripts from your own algorithms by using the methods and properties of this class.\n
    /// The main function MPTK_PlayEvent() and the class MPTKEvent are able to create all kind of MIDI events as note-on.\n
    /// All the values must be set in MPTKEvent, command, note value, duration ... for more details look at the class MPTKEvent.\n\n
    /// A note-on must also be stopped, : if duration = -1 the note is infinite, it's the goal of MPTK_StopEvent() to stop the note with a note-off.\n
    /// On top of that, the Pro version adds playing chords with MPTK_PlayChordFromRange() and MPTK_PlayChordFromLib().\n
    /// For playing scales, have a look to the class MPTKRangeLib\n
    /// For more information see here https://paxstellar.fr/midi-file-player-detailed-view-2-2/ \n
    /// and look at the demo TestMidiStream with the source code TestMidiStream.cs.\n\n
    /// This class inherits from MidiSynth so all properties, events, methods from MidiSynth are available in this class.\n\n
    /// @note
    ///     - A full example bellow. 
    ///     - You can't directly copy paste this extract. But the full source code is available for free with Maestro MPTK free.
    ///     - Some features are available only with the Pro version.
    /// @snippet TestMidiStream.cs ExampleFullMidiStream
    /// </summary>
    //[RequireComponent(typeof(MidiPlayerGlobal))] -- not possible. It's a singleton object, all gameobjects will be merge at run time
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(AudioReverbFilter))]
    [RequireComponent(typeof(AudioChorusFilter))]
    [HelpURL("https://paxstellar.fr/midi-file-player-detailed-view-2-2/")]
    public partial class MidiStreamPlayer : MidiSynth
    {
        /// <summary>
        /// Description and list of MIDI Channels associated to the MIDI synth.\n
        /// Each MIDI synth has 16 channels that carry all the relevant MIDI information.
        ///     - Current instrument / bank
        ///     - Volume
        ///     - Mute / Unmute (see Enable)
        ///     - Pitch bend ...
        ///     
        /// They serve to distinguish between instruments and provide independent control over each one. \n
        /// By transmitting MIDI messages on their respective channels, you can alter the instrument, volume, pitch, and other parameters. \n
        /// Within the Maestro Midi Player Toolkit, MIDI channels are designated numerically from 0 to 15. Notably, channel 9 is set aside specifically for drum sounds.
        /// 
        /// @snippet TestMidiFilePlayerScripting.cs ExampleUsingChannelAPI_Full
        /// </summary>
        public MPTKChannels MPTK_Channels
        {
            get { return Channels; }
        }

        new void Awake()
        {
            base.Awake();
        }

        new void Start()
        {
            MPTK_StartMidiStream();
        }

        // removed V2.89.0
        //private bool startIsDone = false;

        /// <summary>@brief
        /// Force starting of the MidiStream. May be useful from another gamecomponent which want send midi command at the start of the application.
        /// @code
        /// 
        /// // Find a MidiStreamPlayer Prefab from the scene
        /// MidiStreamPlayer MidiStream = FindObjectOfType<MidiStreamPlayer>();
        /// MidiStream.MPTK_StartMidiStream();
        ///
        /// @endcode        
        /// </summary>
        public void MPTK_StartMidiStream()
        {
            //Debug.Log($"*** MPTK_StartMidiStream IdSynth:{IdSynth} ***");
            //if (!startIsDone)
            {
                //startIsDone = true;
                try
                {
                    MPTK_InitSynth();
                    // With v2.10.1 the choice is available in the inspector. True by default.
                    // MPTK_EnablePresetDrum = true;
                    base.Start();
                    // V2.88.2 forced EnablePresetDrum to true removed -  Always enabled for midi stream or need to be change by script
                    //MPTK_EnablePresetDrum = true;
                    ThreadDestroyAllVoice();
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        /// <summary>@brief
        /// Play one midi event from an instance of the class MPTKEvent.\n
        /// Run in a thread so the call return immediately.\n
        /// See also the method #MPTK_StopEvent to stop a note from an instance of MPTKEvent.
        /// @snippet MusicView.cs Example PlayNote
        /// </summary>
        /// <param name="mptkEvent">instance of the class MPTKEvent</param>
        public void MPTK_PlayEvent(MPTKEvent mptkEvent)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    if (!MPTK_CorePlayer)
                        Routine.RunCoroutine(TheadPlay(mptkEvent), Segment.RealtimeUpdate);
                    else
                    {
                        lock (this) // V2.83
                        {
                            QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StartEvent, MidiEvent = mptkEvent });
                        }
                    }
                }
                else
                    Debug.LogWarningFormat("SoundFont not yet loaded, MIDI Event cannot be processed Code:{0} Channel:{1}", mptkEvent.Command, mptkEvent.Channel);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>@brief
        /// Play a list MIDI event from an instance of the class MPTKEvent.\n
        /// Run in a thread so the call return immediately.\n
        /// See also the method #MPTK_StopEvent to stop a note from an instance of MPTKEvent.
        /// @snippet TestMidiStream.cs ExampleMPTK_PlayEvent
        /// </summary>
        /// <param name="mptkEvents">List of instance of the class MPTKEvent</param>
        public void MPTK_PlayEvent(List<MPTKEvent> mptkEvents)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    if (!MPTK_CorePlayer)
                        Routine.RunCoroutine(TheadPlay(mptkEvents), Segment.RealtimeUpdate);
                    else
                    {
                        lock (this) // V2.83
                        {
                            foreach (MPTKEvent evnt in mptkEvents)
                                QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StartEvent, MidiEvent = evnt });
                        }
                    }
                }
                else
                    Debug.LogWarningFormat("SoundFont not yet loaded, MIDI Events cannot be processed");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private IEnumerator<float> TheadPlay(MPTKEvent evnt)
        {
            if (evnt != null)
            {
                try
                {
                    //TBR if (!MPTK_PauseOnDistance || MidiPlayerGlobal.MPTK_DistanceToListener(this.transform) <= VoiceTemplate.Audiosource.maxDistance)
                    {
#if DEBUGPERF
                        DebugPerf("-----> Init perf:", 0);
#endif
                        MPTK_PlayDirectEvent(evnt);
#if DEBUGPERF
                        DebugPerf("<---- ClosePerf perf:", 2);
#endif
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            yield return 0;
        }

        private IEnumerator<float> TheadPlay(List<MPTKEvent> events)
        {
            if (events != null && events.Count > 0)
            {
                try
                {
                    try
                    {
                        //TBR if (!MPTK_PauseOnDistance || MidiPlayerGlobal.MPTK_DistanceToListener(this.transform) <= VoiceTemplate.Audiosource.maxDistance)
                        {
                            PlayEvents(events, true);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MidiPlayerGlobal.ErrorDetail(ex);
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            yield return 0;

        }

        /// <summary>@brief
        /// Stop playing the note. All samples associated to the note are stopped by sending a noteoff.\n
        /// Take into parameter an instance of the class MPTKEvent which is used to produced a note from #MPTK_PlayEvent.
        /// @code
        /// void Update()
        /// {
        ///     if (Input.GetKeyDown(KeyCode.Space))
        ///     {
        ///        // Assign our "Hello, World!"-equivalent note (using MPTKEvent's defaults plus Value = 60 for C5.
        ///        // HelperNoteLabel class could be your friend)
        ///        **mptkEvent** = new MPTKEvent() { Value = 60 };
        ///        // Start playing our "Hello, World!"-equivalent note
        ///        midiStreamPlayer.MPTK_PlayEvent(mptkEvent);
        ///    }
        ///    else if (Input.GetKeyUp(KeyCode.Space))
        ///    {
        ///        // Stop playing our "Hello, World!"-equivalent note
        ///        midiStreamPlayer.** MPTK_StopEvent** (mptkEvent);
        ///    }
        /// }
        /// @endcode
        /// </summary>
        /// <param name="mptkEvent">an instance of the class MPTKEvent which is used to produced a note from MPTK_PlayEvent</param>
        public void MPTK_StopEvent(MPTKEvent mptkEvent)
        {
            if (!MPTK_CorePlayer)
                StopEvent(mptkEvent);
            else
            {
                lock (this) // V2.83
                {
                    QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StopEvent, MidiEvent = mptkEvent });
                }
            }
        }

        // current max distance for this player
        float localMaxDistance;

        public void Update()
        {
            if (MPTK_Spatialize && localMaxDistance != MPTK_MaxDistance)
            {
                localMaxDistance = MPTK_MaxDistance;
                SetSpatialization();

                distanceToListener = MidiPlayerGlobal.MPTK_DistanceToListener(this.transform);
                if (distanceToListener > MPTK_MaxDistance)
                {
                    // Nothing to stop with the midistreamplayer like for the MIDI player
                }
            }
        }
    }
}

