// Ver 1.0 - Universal logic blox script for music synthesizer.
// Mar 2019 Initial release
// Developed by Mike Hogan (2019) - Granby Games - mhogan@remhouse.com
// Updated May30, 2019 - Added sequencer logic


using System;
using System.Collections;
using UnityEngine;
using MidiJack;


public class UniTones : MonoBehaviour
{
    #region Constants
    // filter parameter limits and magic numbers
    const float volDef = .4f, volMax = .9f, volMin = 0f, volLDialTickDef = .01f;
    const float pitchDef = 1, pitchMin = .1f, pitchMax = 3, pitchDialTick = .001f, globalPitchAdj = 0;

    const float lowCutDef = 300, lowCutMin = 50, lowCutMax = 8000, lowCutDialTick = 190;
    const float lowResDef = 2, lowResMin = 1, lowResMax = 10, lowResDialTick = .1f;

    const float highCutDef = 30f, highCutMin = 100, highCutMax = 1000, highCutDialTick = 12;
    const float highResDef = 1, highResMin = 1, highResMax = 10, highResDialTick = .1f;

    const float distortLevDef = .4f, distortMin = 0, distortMax = 1f, distortLevIncDef = .005f, distortLevIncMin = .001f, distortLevIncMax = 1f;
    const float chorusRateMax = 20, chorusRateMin = 0, chorusDepthMin = 0, chorusDepthMax = 1, chorusRateIncDef = .5f, chorusDepthIncDef = .04f, chorusRateIncMin = .5f, chorusRateIncMax = 4, chorusDepthIncMin = .01f, chorusDepthIncMax = 1f;

    const float decayRateDef = .026f, decayTimerDurDef = 750, decayRateDialTick = .001f, decayLenMin = .002f, decayLenMax = .026f;
    const float decayTailDef = .015f;

    const float sustainLenDef = 0, sustainTimerDurDef = 20, sustainLenDialTickDef = .5f, sustainLenTickDef = .05f, sustainMax = 300, sustainMin = 0;
    const float echoDelayMax = 1000, echoDealyMin = 1, echoDelayDef = 1f, echoDelayIncDef = 10f, echoDelayIncMax = 1, echoDelayIncMin = .0001f;
    const float arpTick = .1f, arpTimerDurDef = 100;

    const int KeyStatLed = 0, MainArpStatLed = 1, OctArpLed = 2, PitchLfoStatLed = 3, LowLfoStatLed = 4, VolLfoStatLed = 5;

    const int footPedalSw = 0, sustainSw = 1, volSw = 2, pitchSw = 3, modSw = 4, arpMainSw = 5;

    // LFO parameter limits and magic numbers
    const float volTimerMin = .01f, volTimerMax = 100, volTimerDurDef = 6, volTimerTick = .0005f;
    const float volRateMax = 5, volRateMin = .01f, volRateTick = .01f;
    const float pitchRateTick = .001f, pitchTimerDurDef = 20, pitchTimerTick = .1f, pitchRateMax = .5f;

    const float lowCutTimerMin = 0.05f, lowCutTimerMax = 1000, lowCutTimerDurDef = 10, lowCutTimerTick = 5f, lowCutRateTick = .1f;
    const float attackDefFreq = 300, attackMinFreq = 2, attackMaxFreq = 8000, attackTimerDurDef = 20, attackTimerMaxDur = 8000, attackTimerDialTickDef = 10, attackRateDialTickDef = 1f;
    const float echoLfoDelayMax = 800, echoLfoDelayMin = .1f, echoTimerDurDef = 60, echoLfoTimerTick = 1, echoLfoDelayTick = 10f;

    const float keyVelAdj = 2.5f;
    const int numNotes = 12;
    const int numOctaves = 4;

    const float pitchQuantTickDef = .001f;

    #endregion

    [HideInInspector]
    string[] waveNameExt;

    public int selectedWave;
    public bool isMidi;
    [HideInInspector]
    public MidiChannel midiChannel = 0;

    public bool isMaster;
    public int octave;
    public float volSet = volDef;

    bool isAnalyseSound;
    bool isAddSinWave, isAddSqrWave, isAddSawWave;

    bool isPlayout;
    bool isPitchSync;

    float pitchSet;
    #region Sound Envelope

    bool isDecay, isSustain, isAttack, isEcho, isVelocity;

    bool isLowCutLfoAttack, isPitchLfoAttack, isVolLfoAttack, isEchoLfoAttack;

    bool isPitchQuant;
    bool isWhiteNoise, isPinkNoise, isBownNoise;
    bool isPitchShift, isPitchReverse;
    float pitchShifterRate, pitchShifterTimer, pitchShifterTimerDur;
    double pitchQuantTick;
    bool isLowCutShift, isLowCutReverse;
    float[] lowCutShifterRate, lowCutShifterTimerDur;
    float lowCutShifterTimer;

    bool isVolShift, isVolReverse;
    float volShifterRate, volShifterTimer, volShifterTimerDur;
  //  float volProcAdj=0;

    bool[] isDecayTrig;
    float[] decayRate, decayTimer;
    float[] decayTail;

    float[] decayProcTimer;

    bool[] isSustainTrig;
    float[] sustainTimer;
    float sustainDur, sustainRate;

    bool[] isAttackTrig;
    float[] attackTimer, attacklowCutRate, keyVelocity;

    float attackTimerDur;
    #endregion


    AudioSource[] audioSource;
    AudioReverbFilter[] audioReverb;
    AudioReverbFilter[] audioProcReverb;
    AudioEchoFilter[] audioEcho;
    AudioEchoFilter[] audioProcEcho;
    AudioDistortionFilter[] audioDistortion;
    AudioDistortionFilter[] audioProcDistortion;
    AudioChorusFilter[] audioChorus;
    AudioChorusFilter[] audioProcChorus;
    AudioLowPassFilter[] audioLowPass;
    AudioLowPassFilter[] audioProcLowPass;
    AudioHighPassFilter[] audioHighPass;

    [HideInInspector]
    public AllClips allClips = new AllClips();
    public Hashtable soundIndex = new Hashtable();
    Hashtable paramIndex = new Hashtable();

    [HideInInspector]
    public GameObject[] procToneObj = new GameObject[numNotes];

    #region keyboard control array
    bool[] isKeyUp = new bool[numNotes];
    bool[] isKeyDown = new bool[numNotes];

    [HideInInspector]
    public bool[] keyPressTrig, keyReleaseTrig;


    [HideInInspector]
    public bool[] isStopNote;

    bool isArpOn, isArpReverse;
    int[] arpNote, arpAdv, arpDec;
    float arpTimer, arpTimerDur;
    int arpCount;

    bool isArpPitchSet, isArpPitchUpTrigger, isArpPitchDownTrigger;

    float[] pitchValue = new float[numNotes];
    [HideInInspector]
    public float[] pitchAdj = new float[numNotes];

    #endregion


    #region Keycodes
    //keyboard as music keys
    const KeyCode kb0 = KeyCode.Alpha1; //C1
    const KeyCode kb1 = KeyCode.F1;  // C1#
    const KeyCode kb2 = KeyCode.Alpha2; // D1
    const KeyCode kb3 = KeyCode.F2; //D1#
    const KeyCode kb4 = KeyCode.Alpha3;  //E1
    const KeyCode kb5 = KeyCode.Alpha4;   //F1
    const KeyCode kb6 = KeyCode.F3; //F1#
    const KeyCode kb7 = KeyCode.Alpha5;   //G1
    const KeyCode kb8 = KeyCode.F4;   //G1#
    const KeyCode kb9 = KeyCode.Alpha6;   // A1
    const KeyCode kb10 = KeyCode.F5; // A1#
    const KeyCode kb11 = KeyCode.Alpha7; //B1
    const KeyCode kb12 = KeyCode.Alpha8; //C2
    const KeyCode kb13 = KeyCode.F6; //C2#
    const KeyCode kb14 = KeyCode.Alpha9;  //D2
    const KeyCode kb15 = KeyCode.F7; //D2#
    const KeyCode kb16 = KeyCode.Alpha0;   //E2
    const KeyCode kb17 = KeyCode.Minus;   //F2
    const KeyCode kb18 = KeyCode.F8;  //F2#
    const KeyCode kb19 = KeyCode.Equals; //G2
    const KeyCode kb20 = KeyCode.F9;   //G2#
    const KeyCode kb21 = KeyCode.Backspace;   //A2
    const KeyCode kb22 = KeyCode.F10;   //A2#
    const KeyCode kb23 = KeyCode.Insert;   //B2
    const KeyCode kb24 = KeyCode.Home;   //C3
    #endregion

    private const int QSamples = 1024;
    private const float RefValue = 0.1f, Threshold = 0.02f;

    public string rmsAll, dbAll, pitchAll;
    public float totalRms;
    float[] _samples, _spectrum;
    float _fSample;



    //  [Header("MIDI Knobs")]
    [HideInInspector]
    public GameObject[] controlObj = new GameObject[5];
    bool sustainPedalTrig;
    bool volKnobTrig;
    public int debugLev;



    void Start()
    {
        createAudioObjects();
        initParams();
        addSounds();

    }

    void Update()
    {

        if (isAnalyseSound)
        {
            AnalyzeSound();
        }

    }



    void FixedUpdate()
    {


        if (isPitchQuant)
            pitchQuant();

        finalEnvelope();


        if (!isMidi)
        {
            getDawKeyboardData();
        }
        else
        {
            getMIDIKeys();
            getMidiKnobs();
        }

        updateLogicTimers();




    }



    #region Detect keyboard and MIDI data

    void getDawKeyboardData()
    {

        if (octave == 1)
        {
            //key up

            if (Input.GetKeyUp(kb0))
                procKeyUp(0);
            if (Input.GetKeyUp(kb1))
                procKeyUp(1);
            if (Input.GetKeyUp(kb2))
                procKeyUp(2);
            if (Input.GetKeyUp(kb3))
                procKeyUp(3);
            if (Input.GetKeyUp(kb4))
                procKeyUp(4);
            if (Input.GetKeyUp(kb5))
                procKeyUp(5);
            if (Input.GetKeyUp(kb6))
                procKeyUp(6);
            if (Input.GetKeyUp(kb7))
                procKeyUp(7);
            if (Input.GetKeyUp(kb8))
                procKeyUp(8);
            if (Input.GetKeyUp(kb9))
                procKeyUp(9);
            if (Input.GetKeyUp(kb10))
                procKeyUp(10);
            if (Input.GetKeyUp(kb11))
                procKeyUp(11);


            // key down
            if (Input.GetKey(kb0))
                procKeyDown(0);
            if (Input.GetKey(kb1))
                procKeyDown(1);
            if (Input.GetKey(kb2))
                procKeyDown(2);
            if (Input.GetKey(kb3))
                procKeyDown(3);
            if (Input.GetKey(kb4))
                procKeyDown(4);
            if (Input.GetKey(kb5))
                procKeyDown(5);
            if (Input.GetKey(kb6))
                procKeyDown(6);
            if (Input.GetKey(kb7))
                procKeyDown(7);
            if (Input.GetKey(kb8))
                procKeyDown(8);
            if (Input.GetKey(kb9))
                procKeyDown(9);
            if (Input.GetKey(kb10))
                procKeyDown(10);
            if (Input.GetKey(kb11))
                procKeyDown(11);
        }

        if (octave == 2)
        {
            // key up
            if (Input.GetKeyUp(kb12))
                procKeyUp(0);
            if (Input.GetKeyUp(kb13))
                procKeyUp(1);
            if (Input.GetKeyUp(kb14))
                procKeyUp(2);
            if (Input.GetKeyUp(kb15))
                procKeyUp(3);
            if (Input.GetKeyUp(kb16))
                procKeyUp(4);
            if (Input.GetKeyUp(kb17))
                procKeyUp(5);
            if (Input.GetKeyUp(kb18))
                procKeyUp(6);
            if (Input.GetKeyUp(kb19))
                procKeyUp(7);
            if (Input.GetKeyUp(kb20))
                procKeyUp(8);
            if (Input.GetKeyUp(kb21))
                procKeyUp(9);
            if (Input.GetKeyUp(kb22))
                procKeyUp(10);
            if (Input.GetKeyUp(kb23))
                procKeyUp(11);


            //key down
            if (Input.GetKey(kb12))
                procKeyDown(0);
            if (Input.GetKey(kb13))
                procKeyDown(1);
            if (Input.GetKey(kb14))
                procKeyDown(2);
            if (Input.GetKey(kb15))
                procKeyDown(3);
            if (Input.GetKey(kb16))
                procKeyDown(4);
            if (Input.GetKey(kb17))
                procKeyDown(5);
            if (Input.GetKey(kb18))
                procKeyDown(6);
            if (Input.GetKey(kb19))
                procKeyDown(7);
            if (Input.GetKey(kb20))
                procKeyDown(8);
            if (Input.GetKey(kb21))
                procKeyDown(9);
            if (Input.GetKey(kb22))
                procKeyDown(10);
            if (Input.GetKey(kb23))
                procKeyDown(11);

        }
    }

    void getMIDIKeys()
    {

        for (int i = 0; i < numNotes; i++)
        {

            if (MidiMaster.GetKeyUp(midiChannel, i + (octave * numNotes)) && !isKeyUp[i])
                procKeyUp(i);

            float keyVel = MidiMaster.GetKey(midiChannel, i + (octave * numNotes));

            if (keyVel > 0 && !keyPressTrig[i])
            {
                if (keyVelocity[i] == 0 && isVelocity)
                {
                    keyVelocity[i] = keyVel;
                    if (debugLev > 0)
                        Debug.Log("Midi key pressed. Key= " + i.ToString() + ", Velocity= " + keyVel.ToString());
                }

                procKeyDown(i);


            }

        }
    }

    void getMidiKnobs()
    {

        if (isMaster && controlObj.Length > 0)
        {
            //check pedal
            var knobData = MidiMaster.GetKnob(64, 0);
            if (knobData == 1 && !sustainPedalTrig)
            {
                if (controlObj.Length >= footPedalSw && controlObj[footPedalSw])
                    if (controlObj.Length >= sustainSw && controlObj[sustainSw].GetComponent<UniLogicChip>().gateState[1] == false)
                        controlObj[sustainSw].GetComponent<UniLogicChip>().gateState[0] = true;

                sustainPedalTrig = true;
                if (debugLev > 0)
                    Debug.Log("Sustain Pedal pedal pressed, Index " + footPedalSw.ToString() + ", State: " + knobData.ToString() + "panelSwitch Idx: " + sustainSw.ToString());

            }
            else
            {
                if (sustainPedalTrig && knobData == 0)
                {
                    if (controlObj.Length >= footPedalSw && controlObj[footPedalSw])
                        if (controlObj.Length >= sustainSw && controlObj[sustainSw].GetComponent<UniLogicChip>().gateState[1] == true)
                            controlObj[sustainSw].GetComponent<UniLogicChip>().gateState[0] = false;

                    sustainPedalTrig = false;
                    if (debugLev > 0)
                        Debug.Log("Sustain Pedal pedal relesased, Index " + footPedalSw.ToString() + ", State: " + knobData.ToString() + "panelSwitch Idx: " + sustainSw.ToString());

                }
            }
        }


        var volKnobData = MidiMaster.GetKnob(7, 0);
        if (volKnobData > 0)
        {
            string volTxt = "";
            volTxt = volSet.ToString("#0.00");
            float volX = float.Parse(volTxt);

            string newVolTxt = "";
            newVolTxt = volKnobData.ToString("#0.00");
            float newVolX = float.Parse(newVolTxt);

            if (newVolX > volMax)
                newVolX = volMax;

            if (newVolX < volMin)
                newVolX = volMin;

            if (newVolX < volX)
            {
                volSet = setIncrements(volMax, volMin, volSet, volLDialTickDef, volLDialTickDef, true);
                StartCoroutine(AudioFX.audioMain(audioSource, volMax, volMin, volSet, true, volLDialTickDef, "VOL", ""));
            }
            else
            if (newVolX > volX)
            {
                volSet = setIncrements(volMax, volMin, volSet, volLDialTickDef, volLDialTickDef, false);
                StartCoroutine(AudioFX.audioMain(audioSource, volMax, volMin, volSet, false, volLDialTickDef, "VOL", ""));
            }

        }


        var pitchKnobData = MidiMaster.GetKnob(1, 0);
        if (pitchKnobData > 0)
        {

            string pitchTxt = "";
            pitchTxt = audioSource[0].pitch.ToString("#0.0");
            float pitchX = float.Parse(pitchTxt);

            string newPitchTxt = "";
            newPitchTxt = pitchKnobData.ToString("#0.0");
            float newPitchX = float.Parse(newPitchTxt);

            if (newPitchX > pitchMax + globalPitchAdj)
                newPitchX = pitchMax + globalPitchAdj;

            if (newPitchX < pitchMin + globalPitchAdj)
                newPitchX = pitchMin + globalPitchAdj;

            if (newPitchX < pitchX)
            {
                StartCoroutine(AudioFX.audioMain(audioSource, pitchMax, pitchMin, .025f, true, .025f, "PITCH", ""));
                pitchSet = newPitchX;
            }
            else
            {
                if (newPitchX > pitchX)
                {

                    StartCoroutine(AudioFX.audioMain(audioSource, pitchMax, pitchMin, .025f, false, .025f, "PITCH", ""));
                    pitchSet = newPitchX;
                }
            }


        }


    }
    #endregion



    public void procKeyDown(int _keyNum)
    {


        //  isArpPitchUpTrigger = true;

        if (isDecay)
        {
            isDecayTrig[_keyNum] = true;

            if (isSustain)
            {
                isSustainTrig[_keyNum] = true;

            }
        }
        else
        {
            isDecayTrig[_keyNum] = false;
        }



        if (!isKeyDown[_keyNum])
        {

            if (isAddSinWave || isAddSqrWave || isAddSawWave && procToneObj[_keyNum].gameObject.transform.GetComponent<UniProcTone>())
                procToneObj[_keyNum].gameObject.transform.GetComponent<UniProcTone>().masterProcVol = 1;

            if (debugLev > 0)
                Debug.Log("Key down: " + _keyNum.ToString());

            isKeyDown[_keyNum] = true;
            isKeyUp[_keyNum] = false;




            if (isAttack)
            {
                isAttackTrig[_keyNum] = true;

                if (isLowCutLfoAttack)
                    StartCoroutine(AudioFX.lowFilterShifter(audioLowPass, lowCutMax, lowCutMin, lowCutDef, true, lowCutDialTick, false, "LOWCUT"));

                if (isPitchLfoAttack)
                    audioSource[_keyNum].pitch = 1;

            }


            if (isArpOn)
            {

                if (arpCount <= numNotes)
                {
                    arpNote[arpCount] = _keyNum;

                    arpCount++;

                    if (arpCount >= numNotes)
                        arpCount = 0;


                }



            }

        }

        keyPressTrig[_keyNum] = true;

    }

    public void procKeyUp(int _keyNum)
    {


        if (isAttack)
        {
            isAttackTrig[_keyNum] = false;
            attackTimer[_keyNum] = attackTimerDur;
        }

        if (isSustain)
            sustainTimer[_keyNum] = sustainDur;

        isKeyDown[_keyNum] = false;
        isKeyUp[_keyNum] = true;
        keyReleaseTrig[_keyNum] = true;
        isSustainTrig[_keyNum] = false;

        if (isLowCutLfoAttack)
        {
            isAttackTrig[_keyNum] = false;
            attackTimer[_keyNum] = attackTimerDur;
            audioLowPass[_keyNum].cutoffFrequency = lowCutDef;
            audioLowPass[_keyNum].lowpassResonanceQ = 1;

        }


        if (isVelocity)
            keyVelocity[_keyNum] = 0;


    }


    #region Process sub function commands from switches and controls


    public void runFunc(GameObject sender, string func, string param, bool _countDown, float value)
    {

        if (func == "")
            return;

        UniLogicChip senderObj = sender.GetComponent<UniLogicChip>();

        string _runFunc = func.ToUpper();
        string _runParam = param.ToUpper();

        if (debugLev > 0)
            Debug.Log("Run Func: " + func + ", isCountingback: " + _countDown.ToString() + ", Value: " + value.ToString());

        switch (_runFunc)
        {

            case "PITCH":
                {

                    if (_runParam == "KEYLEVEL")
                    {
                        StartCoroutine(AudioFX.audioMain(audioSource, pitchMax, pitchMin, value, _countDown, pitchDialTick, _runFunc, "key"));
                        pitchSet = value;

                    }

                    if (_runParam == "LEVEL" || _runParam == "ALLLEVEL")
                    {
                        StartCoroutine(AudioFX.audioMain(audioSource, pitchMax, pitchMin, value, _countDown, pitchDialTick, _runFunc, _runParam));
                        pitchSet = value;

                    }


                    if (_runParam == "PROCLEVEL" || _runParam == "ALLLEVEL")
                    {


                        for (int i = 0; i < numNotes; i++)
                        {
                            if (procToneObj[i])
                            {
                                if (!_countDown)
                                    procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().procFreq = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().procFreq * 1.059463094359;
                                else
                                    procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().procFreq = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().procFreq / 1.059463094359;

                            }
                        }

                    }




                    if (_runParam == "LFOSET")
                    {
                        if (!isPitchShift)
                        {
                            isPitchShift = true;
                        }
                        else
                        {
                            if (isPitchShift)
                            {
                                isPitchShift = false;
                                isPitchReverse = false;

                            }
                        }


                    }

                    if (_runParam == "LFORATE")
                    {
                        pitchShifterRate = setIncrements(pitchRateMax, pitchRateTick, pitchShifterRate, value, pitchRateTick, _countDown);
                    }

                    if (_runParam == "LFODEPTH")
                    {
                        pitchShifterTimerDur = setIncrements(pitchTimerDurDef, pitchTimerTick, pitchShifterTimerDur, value, pitchTimerTick, _countDown);
                    }


                    if (_runParam == "RESET" || _runParam == "RESETALL")
                    {
                        for (int i = 0; i < audioSource.Length; i++)
                            audioSource[i].pitch = pitchDef + globalPitchAdj;


                    }



                    if (_runParam == "PROCRESET" || _runParam == "RESETALL")
                    {
                        for (int i = 0; i < numNotes; i++)
                        {
                            procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().procFreq = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().baseProcFreq;
                        }

                    }


                    if (_runParam == "LFORESET")
                    {

                        pitchShifterRate = pitchRateTick;
                        pitchShifterTimerDur = pitchTimerDurDef;
                        pitchSet = pitchDef + globalPitchAdj;

                    }


                    if (_runParam == "QUANT")
                    {
                        if (!isPitchQuant)
                        {
                            isPitchQuant = true;
                        }
                        else
                        {
                            if (isPitchQuant)
                            {
                                isPitchQuant = false;
                                for (int i = 0; i < numNotes; i++)
                                {
                                    audioSource[i].pitch = 1;

                                }
                            }
                        }


                    }


                    if (_runParam == "QUANTRATE")
                    {
                        pitchQuantTick = setIncrements(.001f, .0009f, (float)pitchQuantTick, value, pitchQuantTickDef, _countDown);

                    }


                    if (_runParam == "ARPPITCH")
                    {
                        if (!isArpPitchSet)
                        {
                            isArpPitchSet = true;
                        }
                        else
                        {
                            if (isArpPitchSet)
                            {
                                isArpPitchSet = false;

                            }
                        }


                    }

                    break;

                }

            case "ECHO":
                {

                    if (_runParam == "DELAY")
                    {
                        for (int i = 0; i < audioEcho.Length; i++)
                            audioEcho[i].delay = setIncrements(echoDelayMax, echoDealyMin, audioEcho[i].delay, value, echoDelayIncDef, _countDown);

                    }


                    if (_runParam == "DELAYPROC")
                    {
                        for (int i = 0; i < audioEcho.Length; i++)
                            audioProcEcho[i].delay = setIncrements(echoDelayMax, echoDealyMin, audioProcEcho[i].delay, value, echoDelayIncDef, _countDown);

                    }


                    if (_runParam == "RATIO")
                    {
                        for (int i = 0; i < audioEcho.Length; i++)
                            audioEcho[i].decayRatio = setIncrements(1, 0, audioEcho[i].decayRatio, value, .033f, _countDown);

                    }


                    if (_runParam == "RATIOPROC")
                    {
                        for (int i = 0; i < audioEcho.Length; i++)
                            audioProcEcho[i].decayRatio = setIncrements(1, 0, audioProcEcho[i].decayRatio, value, .033f, _countDown);

                    }

                    if (_runParam == "WETMIX")
                    {
                        for (int i = 0; i < audioEcho.Length; i++)
                            audioEcho[i].wetMix = setIncrements(1, 0, audioEcho[i].wetMix, value, .033f, _countDown);

                    }

                    if (_runParam == "DRYMIX")
                    {
                        for (int i = 0; i < audioEcho.Length; i++)
                            audioEcho[i].dryMix = setIncrements(1, 0, audioEcho[i].dryMix, value, .033f, _countDown);

                    }


                    if (_runParam == "SET")
                    {
                        for (int i = 0; i < audioEcho.Length; i++)
                        {
                            if (!audioEcho[i].isActiveAndEnabled)
                                audioEcho[i].enabled = true;
                            else
                                 if (audioEcho[i].isActiveAndEnabled)
                                audioEcho[i].enabled = false;

                        }


                    }


                    if (_runParam == "SETPROC")
                    {
                        for (int i = 0; i < audioProcEcho.Length; i++)
                        {


                            if (!audioProcEcho[i].isActiveAndEnabled)
                                audioProcEcho[i].enabled = true;
                            else
                                   if (audioProcEcho[i].isActiveAndEnabled)
                                audioProcEcho[i].enabled = false;

                        }

                    }


                    if (_runParam == "RESET")
                    {
                        for (int i = 0; i < audioEcho.Length; i++)
                        {
                            audioEcho[i].delay = echoDelayDef;
                            audioEcho[i].enabled = false;
                        }


                    }

                    break;
                }

            case "DISTORT":
                {
                    if (_runParam == "LEVEL")
                    {
                        for (int i = 0; i < audioDistortion.Length; i++)
                            audioDistortion[i].distortionLevel = setIncrements(distortMax, distortMin, audioDistortion[i].distortionLevel, value, distortLevIncDef, _countDown);
                    }

                    if (_runParam == "LEVELPROC")
                    {
                        for (int i = 0; i < audioProcDistortion.Length; i++)
                            audioProcDistortion[i].distortionLevel = setIncrements(distortMax, distortMin, audioProcDistortion[i].distortionLevel, value, distortLevIncDef, _countDown);
                    }


                    if (_runParam == "SET")
                    {
                        for (int i = 0; i < audioDistortion.Length; i++)
                        {
                            if (!audioDistortion[i].isActiveAndEnabled)
                                audioDistortion[i].enabled = true;
                            else
                            {
                                audioDistortion[i].enabled = false;

                            }
                        }

                    }


                    if (_runParam == "SETPROC")
                    {
                        for (int i = 0; i < audioProcDistortion.Length; i++)
                        {
                            if (!audioProcDistortion[i].isActiveAndEnabled)
                                audioProcDistortion[i].enabled = true;
                            else
                            {
                                audioProcDistortion[i].enabled = false;

                            }
                        }

                    }

                    if (_runParam == "RESET")
                    {
                        for (int i = 0; i < audioDistortion.Length; i++)
                            audioDistortion[i].distortionLevel = distortLevDef;

                    }

                    break;
                }

            case "CHORUS":
                {
                    if (_runParam == "RATE")
                    {
                        for (int i = 0; i < audioChorus.Length; i++)
                            audioChorus[i].rate = setIncrements(chorusRateMax, chorusRateMin, audioChorus[i].rate, value, chorusRateIncDef, _countDown);
                    }

                    if (_runParam == "RATEPROC")
                    {
                        for (int i = 0; i < audioProcChorus.Length; i++)
                            audioProcChorus[i].rate = setIncrements(chorusRateMax, chorusRateMin, audioProcChorus[i].rate, value, chorusRateIncDef, _countDown);
                    }


                    if (_runParam == "DEPTH")
                    {
                        for (int i = 0; i < audioChorus.Length; i++)
                            audioChorus[i].depth = setIncrements(chorusDepthMax, chorusDepthMin, audioChorus[i].depth, value, chorusDepthIncDef, _countDown);

                    }

                    if (_runParam == "DEPTHPROC")
                    {
                        for (int i = 0; i < audioProcChorus.Length; i++)
                            audioProcChorus[i].depth = setIncrements(chorusDepthMax, chorusDepthMin, audioProcChorus[i].depth, value, chorusDepthIncDef, _countDown);

                    }

                    if (_runParam == "WETMIX1")
                    {
                        for (int i = 0; i < audioChorus.Length; i++)
                            audioChorus[i].wetMix1 = setIncrements(1, 0, audioChorus[i].wetMix1, value, .1f, _countDown);


                    }

                    if (_runParam == "WETMIX2")
                    {
                        for (int i = 0; i < audioChorus.Length; i++)
                            audioChorus[i].wetMix2 = setIncrements(1, 0, audioChorus[i].wetMix2, value, .1f, _countDown);


                    }

                    if (_runParam == "WETMIX3")
                    {
                        for (int i = 0; i < audioChorus.Length; i++)
                            audioChorus[i].wetMix3 = setIncrements(1, 0, audioChorus[i].wetMix3, value, .1f, _countDown);


                    }

                    if (_runParam == "DELAY")
                    {
                        for (int i = 0; i < audioChorus.Length; i++)
                            audioChorus[i].delay = setIncrements(100, 0, audioChorus[i].delay, value, .1f, _countDown);


                    }


                    if (_runParam == "SET")
                    {
                        for (int i = 0; i < audioChorus.Length; i++)
                        {
                            if (!audioChorus[i].isActiveAndEnabled)
                            {
                                audioChorus[i].enabled = true;

                            }
                            else
                            {
                                audioChorus[i].enabled = false;

                            }
                        }
                        break;
                    }


                    if (_runParam == "SETPROC")
                    {
                        for (int i = 0; i < audioProcChorus.Length; i++)
                        {
                            if (!audioProcChorus[i].isActiveAndEnabled)
                            {
                                audioProcChorus[i].enabled = true;

                            }
                            else
                            {
                                audioProcChorus[i].enabled = false;

                            }
                        }
                        break;
                    }

                    if (_runParam == "RESET")
                    {
                        for (int i = 0; i < audioChorus.Length; i++)
                            audioChorus[i].rate = 0;

                    }


                    break;
                }


            case "ATTACK":
                {
                    if (_runParam == "SET")
                    {
                        if (!isAttack)
                            isAttack = true;
                        else
                        {
                            isAttack = false;
                            break;
                        }
                    }
                    if (_runParam == "SETLOWCUTLFO")
                    {
                        if (!isLowCutLfoAttack)
                            isLowCutLfoAttack = true;
                        else
                        {
                            if (isLowCutLfoAttack)
                                isLowCutLfoAttack = false;
                            break;
                        }
                    }


                    if (_runParam == "SETPITCHLFO")
                    {
                        if (!isPitchLfoAttack)
                            isPitchLfoAttack = true;
                        else
                        {
                            if (isPitchLfoAttack)
                                isPitchLfoAttack = false;
                            break;
                        }
                    }

                    if (_runParam == "SETVOLLFO")
                    {
                        if (!isVolLfoAttack)
                            isVolLfoAttack = true;
                        else
                            if (isVolLfoAttack)
                            isVolLfoAttack = false;

                        break;

                    }




                    if (_runParam == "LENGTH")
                    {
                        for (int i = 0; i < audioLowPass.Length; i++)
                        {
                            attackTimer[i] = setIncrements(attackTimerMaxDur, attackTimerDur, attackTimerDur, value, attackTimerDialTickDef, _countDown);

                        }
                        //   StartCoroutine(AudioFX.lowFilterShifter(audioLowPass, lowCutMax, lowCutMin, lowCutShifterRate, isLowCutReverse, lowCutLfoRateDialTickDef, false, "LOWCUT"));


                    }
                    if (_runParam == "RATE")
                    {
                        for (int i = 0; i < audioLowPass.Length; i++)
                        {
                            attacklowCutRate[i] = setIncrements(attackMaxFreq, 1, attacklowCutRate[i], value, attackRateDialTickDef, _countDown);

                        }
                        //  StartCoroutine(AudioFX.lowFilterShifter(audioLowPass, lowCutMax, lowCutMin, lowCutShifterRate, isLowCutReverse, lowCutLfoRateDialTickDef, false, "LOWCUT"));


                    }

                    break;

                }

            case "DECAY":
                {
                    if (_runParam == "SET")
                    {
                        if (!isDecay)
                            isDecay = true;
                        else
                        {
                            isDecay = false;
                            break;
                        }
                    }

                    if (_runParam == "LENGTH")
                    {
                        for (int i = 0; i < numNotes; i++)
                            decayRate[i] = setIncrements(decayLenMax, decayLenMin, decayRate[i], value, decayRateDialTick, !_countDown);
                    }


                    break;

                }


            case "SUSTAIN":
                {
                    if (_runParam == "RATE")
                    {

                        sustainRate = setIncrements(sustainMax, sustainMin, sustainRate, value, sustainLenDialTickDef, _countDown);
                        break;
                    }

                    if (_runParam == "SET")
                        if (!isSustain)
                        {
                            isSustain = true;
                        }
                        else
                        {
                            isSustain = false;
                            for (int i = 0; i < numNotes; i++)
                            {
                                isSustainTrig[i] = false;

                            }


                        }

                    break;
                }


            case "DAMPEN":
                {
                    bool prevDecay = isDecay;

                    if (isDecay)
                        isDecay = false;

                    for (int i = 0; i < numNotes; i++)
                        isStopNote[i] = true;

                    finalEnvelope();

                    if (prevDecay)
                        isDecay = true;

                    break;

                }

            case "VOL":
                {
                    if (_runParam == "LEVEL" || _runParam == "ALLLEVEL")
                    {
                        volSet = setIncrements(volMax, volMin, volSet, volLDialTickDef, value, _countDown);
                        StartCoroutine(AudioFX.audioMain(audioSource, volMax, volMin, value, _countDown, volLDialTickDef, _runFunc, _runParam));

                    }


                    if (_runParam == "PROCLEVEL" || _runParam == "ALLLEVEL")
                    {
                        float procVolAdj = 0;
                        for (int i = 0; i < numNotes; i++)
                        {

                            if (!_countDown)
                                procVolAdj = volRateTick;
                            else
                                procVolAdj = -volRateTick;

                            if (procToneObj[i])
                            {
                                procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().sinWaveIntensity = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().sinWaveIntensity + procVolAdj;
                                procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().sqrWaveIntensity = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().sqrWaveIntensity + procVolAdj;
                                procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().sawWaveIntensity = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().sawWaveIntensity + procVolAdj;
                            }
                        }
                    }



                    if (_runParam == "LFOSET")
                    {
                        if (!isVolShift)
                        {
                            volShifterTimerDur = volTimerDurDef;
                            isVolShift = true;
                        }

                        else
                        {
                            if (isVolShift)
                            {
                                volShifterTimerDur = volTimerDurDef;
                                isVolShift = false;
                                isVolReverse = false;
                            }
                        }

                    }


                    if (_runParam == "LFOSETAMPPROC")
                    {

                        for (int i = 0; i < numNotes; i++)
                        {
                            if (!procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().isProcAmpMod)
                            {
                                procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().isProcAmpMod = true;
                            }

                            else
                            {
                                if (procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().isProcAmpMod)
                                {
                                    procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().isProcAmpMod = false;

                                }
                            }
                        }

                    }

                    if (_runParam == "LFOSETFREQPROC")
                    {

                        for (int i = 0; i < numNotes; i++)
                        {
                            if (!procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().isProcFreqMod)
                            {
                                procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().isProcFreqMod = true;
                            }

                            else
                            {
                                if (procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().isProcFreqMod)
                                {
                                    procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().isProcFreqMod = false;

                                }
                            }
                        }

                    }


                    if (_runParam == "LFORATE")
                    {
                        volShifterRate = setIncrements(volRateMax, volRateMin, volShifterRate, value, volLDialTickDef, _countDown);

                    }


                    if (_runParam == "LFOAMPPROC")
                    {
                        for (int i = 0; i < numNotes; i++)
                        {
                            procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().ampModOscFreq = setIncrements(70, volRateMin, procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().ampModOscFreq, value, volLDialTickDef, _countDown);
                        }

                    }


                    if (_runParam == "LFOFREQPROC")
                    {
                        for (int i = 0; i < numNotes; i++)
                        {
                            procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().freqModOscFreq = setIncrements(volRateMax, volRateMin, procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().freqModOscFreq, value, volLDialTickDef, _countDown);
                        }

                    }


                    if (_runParam == "LFODEPTH")
                    {
                        volShifterTimerDur = setIncrements(volTimerMax, volTimerMin, volShifterTimerDur, value, volTimerTick, _countDown);

                    }

                    if (_runParam == "RESET")
                    {
                        for (int i = 0; i < audioSource.Length; i++)
                            audioSource[i].volume = volDef;
                    }





                    if (_runParam == "LFORESET")
                    {

                        volShifterRate = volLDialTickDef;
                        volShifterTimerDur = volTimerDurDef;

                    }


                    break;
                }

            case "SETOSC":
                {

                    if (senderObj.chipType.ToUpper() == "DIAL")
                    {
                        selectedWave = (int)senderObj.selCurPos;
                    }

                    if (senderObj.chipType.ToUpper() == "SWITCH")
                    {
                        selectedWave = (int)value;
                    }


                    if (selectedWave == 6)
                        isPlayout = true;
                    else
                        isPlayout = false;

                    break;

                }

            case "REVERB":
                {
                    if (_runParam == "PRESET")
                    {

                        if (senderObj.sendVal > -1 && senderObj.sendVal < 27)
                        {
                            for (int i = 0; i < audioReverb.Length; i++)
                            {
                                if (audioReverb[i])
                                {
                                    if (audioReverb[i].reverbPreset != (AudioReverbPreset)value)
                                    {
                                        audioReverb[i].reverbPreset = (AudioReverbPreset)value;
                                        if (!audioReverb[i].isActiveAndEnabled)
                                            audioReverb[i].enabled = true;
                                    }
                                    else
                                    {
                                        audioReverb[i].reverbPreset = 0;

                                    }


                                }


                            }


                            for (int i = 0; i < audioReverb.Length; i++)
                                if (audioReverb[i].reverbPreset == 0)
                                    audioReverb[i].enabled = false;

                        }

                    }

                    if (_runParam == "PRESETPROC")
                    {

                        if (senderObj.sendVal > -1 && senderObj.sendVal < 27)
                        {
                            for (int i = 0; i < audioProcReverb.Length; i++)
                            {
                                if (audioProcReverb[i])
                                {
                                    if (audioProcReverb[i].reverbPreset != (AudioReverbPreset)value)
                                    {
                                        audioProcReverb[i].reverbPreset = (AudioReverbPreset)value;
                                        if (!audioProcReverb[i].isActiveAndEnabled)
                                            audioProcReverb[i].enabled = true;
                                    }
                                    else
                                    {
                                        audioProcReverb[i].reverbPreset = 0;

                                    }

                                }


                            }

                            for (int i = 0; i < audioProcReverb.Length; i++)
                                if (audioProcReverb[i].reverbPreset == 0)
                                    audioProcReverb[i].enabled = false;
                        }


                    }


                    if (_runParam == "DISABLE")
                    {
                        for (int i = 0; i < audioReverb.Length; i++)
                        {
                            if (audioReverb[i].isActiveAndEnabled)
                            {
                                audioReverb[i].enabled = false;
                            }
                        }

                    }

                    if (_runParam == "DISABLEPROC")
                    {
                        for (int i = 0; i < audioProcReverb.Length; i++)
                        {
                            if (audioProcReverb[i].isActiveAndEnabled)
                            {
                                audioProcReverb[i].enabled = false;
                            }
                        }

                    }

                    if (_runParam == "SET")
                    {
                        for (int i = 0; i < audioReverb.Length; i++)
                        {
                            if (!audioReverb[i].isActiveAndEnabled)
                            {
                                audioReverb[i].enabled = true;
                            }
                            else
                            {
                                audioReverb[i].enabled = false;
                            }
                        }

                    }




                    break;
                }

            case "LOWFILTER":
                {

                    if (_runParam == "CUT")
                    {
                        StartCoroutine(AudioFX.lowFilterShifter(audioLowPass, lowCutMax, lowCutMin, value, _countDown, lowCutDialTick, false, "LOWCUT"));

                    }

                    if (_runParam == "CUTPROC")
                    {
                        StartCoroutine(AudioFX.lowFilterShifter(audioProcLowPass, lowCutMax, lowCutMin, value, _countDown, lowCutDialTick, false, "LOWCUT"));

                    }

                    if (_runParam == "RES")
                    {
                        StartCoroutine(AudioFX.lowFilterShifter(audioLowPass, lowResMax, lowResMin, value, _countDown, lowResDialTick, false, "LOWRES"));

                    }



                    if (_runParam == "SETLFO")
                    {


                        if (!isLowCutShift)
                        {
                            isLowCutShift = true;
                        }
                        else
                        {
                            if (isLowCutShift)
                            {
                                isLowCutShift = false;
                          //      if (isMaster)
                          //          GetComponent<UniLogicChip>().setStatusLed(false);

                            }
                        }
                    }

                    if (_runParam == "LFOCUT")
                    {
                        for (int i = 0; i < numNotes; i++)
                            lowCutShifterRate[i] = setIncrements(lowCutMax, lowCutMin, lowCutShifterRate[i], value, lowCutRateTick, _countDown);

                    }


                    if (_runParam == "LFODEPTH")
                    {
                        for (int i = 0; i < numNotes; i++)
                            lowCutShifterTimerDur[i] = setIncrements(lowCutTimerMax, lowCutTimerMin, lowCutShifterTimerDur[i], value, lowCutTimerTick, _countDown);
                    }


                    if (_runParam == "SET")
                    {
                        for (int i = 0; i < audioLowPass.Length; i++)
                            if (!audioLowPass[i].isActiveAndEnabled)
                                audioLowPass[i].enabled = true;
                            else
                            {
                                if (audioLowPass[i].isActiveAndEnabled)
                                {
                                    audioLowPass[i].enabled = false;

                                }
                            }
                    }

                    if (_runParam == "SETPROC")
                    {
                        if (audioProcLowPass.Length > 0)
                        {

                            for (int i = 0; i < audioProcLowPass.Length; i++)
                                if (!audioProcLowPass[i].isActiveAndEnabled)
                                {
                                    audioProcLowPass[i].enabled = true;
                                }
                                else
                                {
                                    if (audioProcLowPass[i].isActiveAndEnabled)
                                    {
                                        audioProcLowPass[i].enabled = false;

                                    }
                                }

                        }

                    }




                    if (_runParam == "RESET")
                    {
                        for (int i = 0; i < audioSource.Length; i++)
                        {
                            audioLowPass[i].cutoffFrequency = lowCutDef;
                            audioLowPass[i].lowpassResonanceQ = 1;

                            lowCutShifterTimer = lowCutTimerDurDef;
                            lowCutShifterRate[i] = lowCutRateTick;
                            lowCutShifterTimerDur[i] = lowCutTimerDurDef;

                        }
                    }

                    break;
                }


            case "HIGHFILTER":
                {
                    if (_runParam == "CUT")
                    {
                        StartCoroutine(AudioFX.highFilterShifter(audioHighPass, highCutMax, highCutMin, value, _countDown, highCutDialTick, false, "HIGHCUT"));
                        break;
                    }

                    if (_runParam == "RES")
                    {
                        StartCoroutine(AudioFX.highFilterShifter(audioHighPass, highResMax, highResMin, value, _countDown, highResDialTick, false, "HIGHRES"));
                        break;
                    }


                    if (_runParam == "SET")
                    {
                        for (int i = 0; i < audioHighPass.Length; i++)
                        {
                            if (!audioHighPass[i].isActiveAndEnabled)
                                audioHighPass[i].enabled = true;
                            else
                                audioHighPass[i].enabled = false;
                        }

                    }



                    if (_runParam == "RESET")
                    {
                        for (int i = 0; i < audioSource.Length; i++)
                        {
                            audioHighPass[i].cutoffFrequency = highCutDef;
                            audioHighPass[i].highpassResonanceQ = 1;
                        }

                    }


                    break;

                }


            case "ARP":
                {
                    if (_runParam == "SET")
                    {

                        if (!isArpOn)
                        {
                            isArpOn = true;
                       //     GetComponent<UniLogicChip>().setStatusLed(true);
                       //     GetComponent<UniLogicChip>().setStatusLed(true);

                        }
                        else
                        {
                            for (int i = 0; i < numNotes; i++)
                            {
                                arpNote[i] = -1;
                                isStopNote[i] = true;
                            }

                            isArpOn = false;
                            arpTimer = arpTimerDur;
                            arpCount = 0;
                            arpAdv[octave] = 0;

                        //    GetComponent<UniLogicChip>().setStatusLed( false);
                        //    GetComponent<UniLogicChip>().setStatusLed(false);

                        }


                    }

                    if (_runParam == "RESET")
                    {
                        if (isArpOn)
                        {
                            isArpOn = false;
                            // arppegTimerTrig = true;
                            GetComponent<UniLogicChip>().setStatusLed(false);

                          //  isArpCord = false;
                            arpTimer = arpTimerDur;
                            arpCount = 0;

                            arpAdv[octave] = 0;
                            arpDec[octave] = 0;

                            for (int i = 0; i < numNotes; i++)
                            {
                                arpNote[i] = -1;
                                isStopNote[i] = true;
                            }

                        }

                    }

                    if (_runParam == "RATE")
                    {
                        if (value == 0)
                            value = arpTick;

                        float _rateChange = arpTimerDur;

                        if (_countDown)
                        {
                            _rateChange += value;
                            if (_rateChange >= arpTimerDurDef)
                                _rateChange = arpTimerDurDef;


                        }

                        else
                        {
                            _rateChange -= value;
                            if (_rateChange <= 0f)
                                _rateChange = 0f;


                        }

                        arpTimerDur = _rateChange;
                    }

                    if (_runParam == "PAUSE")
                    {
                        if (isArpOn)
                        {
                            isArpOn = false;

                            for (int i = 0; i < numNotes; i++)
                            {
                                isStopNote[i] = true;
                            }
                        }
                        else
                        {
                            if (!isArpOn)
                            {
                                isArpOn = true;
                            }
                        }

                    }

                    if (_runParam == "DIR")
                    {

                        if (!isArpReverse)
                        {
                            isArpReverse = true;
                            arpCount = numNotes;
                        }
                        else
                        {
                            arpCount = 0;
                            isArpReverse = false;
                        }
                    }

                    break;
                }

            case "SETMIDI":
                {
                    if (!isMidi)
                        isMidi = true;
                    else
                        isMidi = false;
                    break;

                }



            case "VELOCITY":
                {
                    if (!isVelocity)
                        isVelocity = true;
                    else
                        isVelocity = false;
                    break;

                }


            case "AUDIO":
                {
                    if (_runParam == "ANALYSE")
                    {
                        if (!isAnalyseSound)
                            isAnalyseSound = true;
                        else
                            isAnalyseSound = false;


                        if (isAnalyseSound)
                        {
                            foreach (var debugObj in FindObjectsOfType(typeof(UniLightShow)) as UniLightShow[])
                            {
                                if (debugObj.circuitGroup.ToUpper() == senderObj.circuitGroup.ToUpper())
                                {
                                    debugObj.isRunShow = true;

                                }
                            }
                        }
                        else
                        {
                            foreach (var debugObj in FindObjectsOfType(typeof(UniLightShow)) as UniLightShow[])
                            {
                                if (debugObj.circuitGroup.ToUpper() == senderObj.circuitGroup.ToUpper())
                                {
                                    debugObj.isRunShow = false;
                                    senderObj.pinState[0] = false;

                                }
                            }
                        }


                    }


                    if (_runParam == "SETSCAN")
                    {
                        if (!isAnalyseSound)
                            isAnalyseSound = true;
                        else
                            isAnalyseSound = false;


                        if (isAnalyseSound)
                        {
                            foreach (var debugObj in FindObjectsOfType(typeof(UniLightShow)) as UniLightShow[])
                            {
                                if (debugObj.circuitGroup.ToUpper() == senderObj.circuitGroup.ToUpper())
                                {
                                    debugObj.isRunShow = true;

                                }
                            }
                        }
                        else
                        {
                            foreach (var debugObj in FindObjectsOfType(typeof(UniLightShow)) as UniLightShow[])
                            {
                                if (debugObj.circuitGroup.ToUpper() == senderObj.circuitGroup.ToUpper())
                                {
                                    debugObj.isRunShow = false;
                                    senderObj.pinState[0] = false;

                                }
                            }
                        }


                    }


                    if (_runParam == "DECAY")
                        foreach (var debugObj in FindObjectsOfType(typeof(UniLightShow)) as UniLightShow[])
                            if (debugObj.circuitGroup.ToUpper() == senderObj.circuitGroup.ToUpper())
                                debugObj.decayLenVal = setIncrements(100, 1, debugObj.decayLenVal, value, 1, _countDown);


                    if (_runParam == "FADE")
                        foreach (var debugObj in FindObjectsOfType(typeof(UniLightShow)) as UniLightShow[])
                            if (debugObj.circuitGroup.ToUpper() == senderObj.circuitGroup.ToUpper())
                                debugObj.fadeTick = setIncrements(1, .0001f, debugObj.fadeTick, value, .0001f, _countDown);

                    if (_runParam == "AREA")
                        foreach (var debugObj in FindObjectsOfType(typeof(UniLightShow)) as UniLightShow[])
                            if (debugObj.circuitGroup.ToUpper() == senderObj.circuitGroup.ToUpper())
                                debugObj.areaTick = setIncrements(50, .01f, debugObj.areaTick, value, .01f, _countDown);


                    if (_runParam == "RMS")
                        foreach (var debugObj in FindObjectsOfType(typeof(UniLightShow)) as UniLightShow[])
                            if (debugObj.circuitGroup.ToUpper() == senderObj.circuitGroup.ToUpper())
                                debugObj.rmsHighTrigLev = setIncrements(4, .001f, debugObj.rmsHighTrigLev, value, .01f, _countDown);


                    break;

                }


            case "SETKEYLIGHTS":
                {
                    foreach (var debugObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
                    {


                        if (debugObj.GetComponent<UniLogicChip>().chipType.ToUpper() == "KEYBOARD")
                        {
                            if (debugObj.GetComponent<UniLogicChip>().circuitGroup == sender.GetComponent<UniLogicChip>().circuitGroup)
                            {
                                if (debugObj.GetComponent<UniLogicChip>().isIllumSwitch == false)
                                    debugObj.GetComponent<UniLogicChip>().isIllumSwitch = true;
                                else
                                    debugObj.GetComponent<UniLogicChip>().isIllumSwitch = false;
                            }




                        }
                    }
                    break;
                }



            case "SETPROCTONE":
                {

                    bool _endNote = false;

                    if (_runParam == "SIN")
                    {

                        if (!isAddSinWave)
                        {
                            isAddSinWave = true;
                            for (int i = 0; i < numNotes; i++)
                                procToneObj[i].SetActive(true);

                            return;
                        }
                        else
                        {
                            if (isAddSinWave)
                            {
                                isAddSinWave = false;
                                _endNote = true;


                            }
                        }

                    }

                    if (_runParam == "SQR")
                    {
                        if (!isAddSqrWave)
                        {
                            isAddSqrWave = true;
                            for (int i = 0; i < numNotes; i++)
                                procToneObj[i].SetActive(true);

                            return;
                        }
                        else
                        {
                            isAddSqrWave = false;
                            _endNote = true;
                        }


                    }

                    if (_runParam == "SAW")
                    {
                        if (!isAddSawWave)
                        {
                            isAddSawWave = true;
                            for (int i = 0; i < numNotes; i++)
                                procToneObj[i].SetActive(true);

                            return;
                        }
                        else
                        {
                            isAddSawWave = false;
                            _endNote = true;
                        }


                    }



                    if (_endNote)
                    {
                        for (int i = 0; i < numNotes; i++)
                        {
                            StartCoroutine(AudioFX.endProcSound(procToneObj[i]));


                        }
                    }



                    if (!isAddSawWave && !isAddSinWave && !isAddSqrWave)
                    {

                        for (int i = 0; i < numNotes; i++)
                        {
                            procToneObj[i].SetActive(false);
                        }
                    }



                    break;



                }



        }


    }

    float setIncrements(float _upper, float _lower, float _curVal, float _rate, float _defInc, bool _isReverse)
    {


        if (_rate == 0)
            _rate = _defInc;

        if (_isReverse)
            _rate = -_rate;

        if (_curVal == 0)
            _curVal = _lower;

        float _valueChange = _curVal;
        _valueChange += _rate;

        if (_valueChange > _upper)
            _valueChange = _upper;

        if (_valueChange < _lower)
            _valueChange = _lower;

        _curVal = _valueChange;

        return _curVal;
    }

    #endregion

    #region Play amd process sounds

    public void playAudio(string _sound, int _octave, int _soundIdx)
    {

        if (!audioSource[_soundIdx] || octave != _octave)
            return;

        if (isPlayout)
            StartCoroutine(AudioFX.endSound(audioSource[_soundIdx]));


        bool _isLoop = false;
        decayTimer[_soundIdx] = decayTimerDurDef;
        isDecayTrig[_soundIdx] = false;

        string sound = _sound + waveNameExt[selectedWave];
        AudioClip soundClip = (AudioClip)soundIndex[sound];


        if (waveNameExt[selectedWave].ToLower() == "_sin" || waveNameExt[selectedWave].ToLower() == "_sq" || waveNameExt[selectedWave].ToLower() == "_saw")
        {
            _isLoop = true;
            isPlayout = false;

        }
        else
        {

            if (waveNameExt[selectedWave].ToLower() == "_usr1")
                _isLoop = true;

            if (waveNameExt[selectedWave].ToLower() == "_kit")
                isPlayout = true;

        }


        if (procToneObj.Length > 0 && procToneObj[_soundIdx])
        {
            if (isAddSinWave)
            {
                procToneObj[_soundIdx].gameObject.transform.GetComponent<UniProcTone>().masterProcVol = 1;
                procToneObj[_soundIdx].gameObject.transform.GetComponent<UniProcTone>().isPlaySinWave = true;

            }

            if (isAddSqrWave)
            {
                procToneObj[_soundIdx].gameObject.transform.GetComponent<UniProcTone>().masterProcVol = 1;
                procToneObj[_soundIdx].gameObject.transform.GetComponent<UniProcTone>().isPlaySqrWave = true;

            }

            if (isAddSawWave)
            {
                procToneObj[_soundIdx].gameObject.transform.GetComponent<UniProcTone>().masterProcVol = 1;
                procToneObj[_soundIdx].gameObject.transform.GetComponent<UniProcTone>().isPlaySawWave = true;

            }
        }

        //   if (isAnalyseSound)
        //       pitchQuantX(_soundIdx);

        if (soundClip)
        {
            StartCoroutine(AudioFX.TriggerSound(audioSource[_soundIdx], soundClip, volSet, _isLoop, keyVelocity[_soundIdx]));
        }


        if (debugLev > 0)
            Debug.Log("Sound Clip: " + sound + octave.ToString() + ", Octave: " + octave.ToString() + ", Freq:" + soundClip.frequency + ", len: " + soundClip.samples + ", pitch " + audioSource[_soundIdx].pitch + ", Velocity: " + keyVelocity[_soundIdx].ToString() + ", isloop: " + _isLoop.ToString());



    }



    void finalEnvelope()
    {


        for (int i = 0; i < numNotes; i++)
        {
            if (isStopNote[i])
            {

                isStopNote[i] = false;
                keyPressTrig[i] = false;
                keyReleaseTrig[i] = false;
                isDecayTrig[i] = false;
                decayTimer[i] = decayTimerDurDef;

                if (!isDecay)
                {

                    if (!isPlayout)
                    {

                        if (procToneObj.Length > 0 && procToneObj[i] != null && procToneObj[i].gameObject.transform.GetComponent<UniProcTone>())
                            StartCoroutine(AudioFX.endProcSound(procToneObj[i]));

                        StartCoroutine(AudioFX.endSound(audioSource[i]));

                    }


                }
                else
                {

                    //  Debug.Log("final release");
                    decayTimer[i] = decayTimerDurDef;
                    isDecayTrig[i] = true;

                    if (waveNameExt[selectedWave].ToLower() == "_sin" || waveNameExt[selectedWave].ToLower() == "_sq" || waveNameExt[selectedWave].ToLower() == "_saw")
                    {
                        audioSource[i].loop = true;
                        isPlayout = false;

                    }


                }

            }

        }

    }

    void runAppeg()
    {

       
        if (isArpOn)
        {

            if (arpCount > 0)
                procKeyUp(arpCount);

            if (arpNote[arpAdv[octave]] > -1)
            {
                procKeyDown(arpNote[arpAdv[octave]]);
            }

            arpAdv[octave]++;

            if (arpAdv[octave] >= arpCount)
                arpAdv[octave] = 0;



        }


    }

    void updateLogicTimers()
    {



        if (isArpPitchUpTrigger)
        {

            for (int i = 0; i < numNotes; i++)
            {
                audioSource[i].pitch = audioSource[i].pitch * 1.5f;

                if (audioSource[i].pitch > 3)
                    audioSource[i].pitch = 3;

            }

            isArpPitchUpTrigger = false;

        }

        if (isArpPitchDownTrigger)
        {
            for (int i = 0; i < numNotes; i++)
            {
                audioSource[i].pitch = audioSource[i].pitch / 1.5f;

                if (audioSource[i].pitch < 0)
                    audioSource[i].pitch = 0;
            }


            isArpPitchDownTrigger = false;

        }


        // sound envelope

        #region Released keys envelope shape

        for (int i = 0; i < numNotes; i++)
        {
            // lpoattack
            if (isAttack && isAttackTrig[i])
            {
                if (isLowCutLfoAttack)
                    StartCoroutine(AudioFX.lowFilterShifter(audioLowPass, attackMaxFreq, attackMinFreq, i, false, attacklowCutRate[i], true, "LOWCUT"));

                if (isPitchLfoAttack)
                    StartCoroutine(AudioFX.audioMain(audioSource, pitchMax, pitchMin, pitchShifterRate, isPitchReverse, pitchRateTick, "PITCH", ""));

                if (isVolLfoAttack)
                    StartCoroutine(AudioFX.audioMain(audioSource, volMax, volMin, volShifterRate, isVolReverse, volRateTick, "VOL", ""));

                attackTimer[i]--;

                if (attackTimer[i] <= 0f)
                {
                    isAttackTrig[i] = false;
                    attackTimer[i] = attackTimerDur;

                }

            }


            if (isSustain)
            {
                if (isSustainTrig[i])
                {
                    sustainTimer[i]--;
                    if (sustainTimer[i] <= 0f)
                    {
                        sustainTimer[i] = sustainRate;
                        isSustainTrig[i] = false;
                    }

                }
            }

            if (isDecayTrig[i])
            {

                float decayAdj = 0;

                if (waveNameExt[selectedWave].ToLower() != "_drum" && waveNameExt[selectedWave].ToLower() != "_string" && waveNameExt[selectedWave].ToLower() != "_kit")
                    audioSource[i].loop = true;

                decayTimer[i]--;

                float setVol = audioSource[i].volume;
                float newVol = setVol;

                float setProcVol = 0;

                if (isAddSinWave || isAddSqrWave || isAddSawWave && procToneObj[i] && procToneObj[i].gameObject.transform.GetComponent<UniProcTone>())
                    setProcVol = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().masterProcVol;

                float newProcVol = setProcVol;

                if (decayTimer[i] < decayTimerDurDef - 10)
                {
                    decayAdj = decayTail[i];

                }

                if (!isSustainTrig[i])
                {
                    newVol -= decayRate[i] + decayAdj;
                    newProcVol -= decayRate[i] + decayAdj;

                    if (newVol < 0)
                        newVol = 0;

                    if (newVol > volMax)
                        newVol = volMax;

                    if (newProcVol < 0)
                        newProcVol = 0;

                    if (newProcVol > 1)
                        newProcVol = 1;


                    StartCoroutine(AudioFX.decaySound(audioSource[i], audioSource[i].volume, newVol));

                    if (isAddSinWave || isAddSqrWave || isAddSawWave && procToneObj[i].gameObject.transform.GetComponent<UniProcTone>())
                        procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().masterProcVol = newProcVol;


                }



                if (decayTimer[i] <= 0f)
                {

                    decayTimer[i] = decayTimerDurDef;
                    isDecayTrig[i] = false;
                    audioSource[i].loop = false;

                }

                if (audioSource[i].volume <= 0)
                    audioSource[i].loop = false;


                if (procToneObj.Length > 0 && procToneObj[i] != null && procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().masterProcVol <= 0)
                {

                    if (procToneObj[i].gameObject.transform.GetComponent<UniProcTone>())
                        procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().isStopAll = true;

                    decayTimer[i] = decayTimerDurDef;

                }



            }



            if (keyReleaseTrig[i] && isKeyUp[i])
            {

                audioSource[i].loop = false;
                keyReleaseTrig[i] = false;

            }

            if (audioSource[i].volume <= 0)
            {
                // isDecayTrig[i] = false;
                decayTimer[i] = decayTimerDurDef;

            }



        }
        #endregion


        //arpeggiator
        if (isArpOn)
        {
            if (arpTimer != arpTimerDur)
            {
             //   GetComponent<UniLogicChip>().setStatusLed(false);
            //    GetComponent<UniLogicChip>().setStatusLed(false);
            }



            arpTimer--;

            if (arpTimer <= 0f)
            {
              //  GetComponent<UniLogicChip>().setStatusLed(true);
              //  GetComponent<UniLogicChip>().setStatusLed(true);

                arpTimer = arpTimerDur;
                runAppeg();

            }

        }


        #region Run shifters

        // pitch shifter
        if (isPitchShift)
        {

         //   if (isMaster)
         //       GetComponent<UniLogicChip>().setStatusLed(true);

            StartCoroutine(AudioFX.audioMain(audioSource, pitchMax, pitchMin, pitchShifterRate, isPitchReverse, pitchRateTick, "PITCH", ""));

            pitchShifterTimer--;

            if (pitchShifterTimer <= 0f)
            {
                pitchShifterTimer = pitchShifterTimerDur;
                if (!isPitchReverse)
                    isPitchReverse = true;
                else
                    isPitchReverse = false;
              //  if (isMaster)
               //     GetComponent<UniLogicChip>().setStatusLed(false);
            }
        }

        // pass filters
        if (isLowCutShift)
        {
          //  GetComponent<UniLogicChip>().setStatusLed(true);


            for (int i = 0; i < audioDistortion.Length; i++)
            {
                StartCoroutine(AudioFX.lowFilterShifter(audioLowPass, lowCutMax, lowCutMin, lowCutShifterRate[i], isLowCutReverse, lowCutRateTick, false, "LOWCUT"));


                lowCutShifterTimer--;

                if (lowCutShifterTimer <= 0f)
                {
                    lowCutShifterTimer = lowCutShifterTimerDur[i];
                    if (!isLowCutReverse)
                        isLowCutReverse = true;
                    else
                        isLowCutReverse = false;

                 //   if (isMaster && isLowCutReverse)
                 //       GetComponent<UniLogicChip>().setStatusLed(false);

                }
            }

        }

        // volume shifter
        if (isVolShift)
        {

            StartCoroutine(AudioFX.audioMain(audioSource, volMax, volMin, volShifterRate, isVolReverse, volRateTick, "VOL", ""));

         //   if (isMaster)
         //       GetComponent<UniLogicChip>().setStatusLed(true);

            volShifterTimer--;

            if (volShifterTimer <= 0f)
            {
                volShifterTimer = volTimerDurDef;
                if (!isVolReverse)
                    isVolReverse = true;
                else
                    isVolReverse = false;


              //  if (isMaster)
              //      GetComponent<UniLogicChip>().setStatusLed(false);
            }
        }

        #endregion

    }


    #endregion

    #region Initialization routinines

    void createAudioObjects()
    {


        audioSource = new AudioSource[numNotes];
        for (int i = 0; i < audioSource.Length; i++)
        {
            if (GetComponent<UniLogicChip>().ledObj.Length > 0 && GetComponent<UniLogicChip>().ledObj[i])
            {
                if (!GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioSource>())
                    GetComponent<UniLogicChip>().ledObj[i].gameObject.AddComponent<AudioSource>();

                audioSource[i] = GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioSource>();
                audioSource[i].volume = volDef;
                audioSource[i].velocityUpdateMode = AudioVelocityUpdateMode.Fixed;

                audioSource[i].bypassListenerEffects = true;
                audioSource[i].bypassReverbZones = true;
                audioSource[i].playOnAwake = false;
                audioSource[i].reverbZoneMix = 0;

                //  audioSource[i].panStereo = -1;

            }

        }

        audioChorus = new AudioChorusFilter[numNotes];
        for (int i = 0; i < audioChorus.Length; i++)
        {

            if (GetComponent<UniLogicChip>().ledObj.Length > 0 && GetComponent<UniLogicChip>().ledObj[i])
            {

                if (!GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioChorusFilter>())
                    GetComponent<UniLogicChip>().ledObj[i].gameObject.AddComponent<AudioChorusFilter>();

                audioChorus[i] = GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioChorusFilter>();
                audioChorus[i].enabled = false;

            }

        }

        audioEcho = new AudioEchoFilter[numNotes];
        for (int i = 0; i < audioEcho.Length; i++)
        {
            if (GetComponent<UniLogicChip>().ledObj.Length > 0 && GetComponent<UniLogicChip>().ledObj[i])
            {
                if (!GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioEchoFilter>())
                    GetComponent<UniLogicChip>().ledObj[i].gameObject.AddComponent<AudioEchoFilter>();

                audioEcho[i] = GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioEchoFilter>();
                audioEcho[i].enabled = false;
                audioEcho[i].delay = echoDelayDef;



            }
        }

        audioLowPass = new AudioLowPassFilter[numNotes];
        for (int i = 0; i < audioLowPass.Length; i++)
        {
            if (GetComponent<UniLogicChip>().ledObj.Length > 0 && GetComponent<UniLogicChip>().ledObj[i])
            {

                if (!GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioLowPassFilter>())
                    GetComponent<UniLogicChip>().ledObj[i].gameObject.AddComponent<AudioLowPassFilter>();

                audioLowPass[i] = GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioLowPassFilter>();
                audioLowPass[i].cutoffFrequency = lowCutDef;
                audioLowPass[i].enabled = false;

            }
        }

        audioHighPass = new AudioHighPassFilter[numNotes];
        for (int i = 0; i < audioHighPass.Length; i++)
        {
            if (GetComponent<UniLogicChip>().ledObj.Length > 0 && GetComponent<UniLogicChip>().ledObj[i])
            {

                if (!GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioHighPassFilter>())
                    GetComponent<UniLogicChip>().ledObj[i].gameObject.AddComponent<AudioHighPassFilter>();

                audioHighPass[i] = GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioHighPassFilter>();
                audioHighPass[i].cutoffFrequency = highCutDef;
                audioHighPass[i].enabled = false;

            }
        }

        audioReverb = new AudioReverbFilter[numNotes];
        for (int i = 0; i < audioReverb.Length; i++)
        {
            if (GetComponent<UniLogicChip>().ledObj.Length > 0 && GetComponent<UniLogicChip>().ledObj[i])
            {
                if (!GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioReverbFilter>())
                    GetComponent<UniLogicChip>().ledObj[i].gameObject.AddComponent<AudioReverbFilter>();

                audioReverb[i] = GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioReverbFilter>();
                audioReverb[i].enabled = false;

            }
        }

        audioDistortion = new AudioDistortionFilter[numNotes];
        for (int i = 0; i < audioDistortion.Length; i++)
        {
            if (GetComponent<UniLogicChip>().ledObj.Length > 0 && GetComponent<UniLogicChip>().ledObj[i])
            {
                if (!GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioDistortionFilter>())
                    GetComponent<UniLogicChip>().ledObj[i].gameObject.AddComponent<AudioDistortionFilter>();

                audioDistortion[i] = GetComponent<UniLogicChip>().ledObj[i].GetComponent<AudioDistortionFilter>();
                audioDistortion[i].enabled = false;
                audioDistortion[i].distortionLevel = distortLevDef;

            }
        }


        if (procToneObj.Length > 0 && gameObject.GetComponentInChildren<UniProcTone>())
        {
            audioProcEcho = new AudioEchoFilter[numNotes];

            for (int i = 0; i < procToneObj.Length; i++)
            {

                if (procToneObj[i] && procToneObj[i].gameObject.transform.GetComponent<UniProcTone>())
                {

                    if (!procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioEchoFilter>())
                        procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().gameObject.AddComponent<AudioEchoFilter>();

                    audioProcEcho[i] = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioEchoFilter>();
                    audioProcEcho[i].delay = echoDelayDef;
                    audioProcEcho[i].enabled = false;

                }

            }



            audioProcChorus = new AudioChorusFilter[numNotes];


            for (int i = 0; i < audioProcChorus.Length; i++)
            {

                if (procToneObj[i] && procToneObj[i].gameObject.transform.GetComponent<UniProcTone>())
                {

                    if (!procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioChorusFilter>())
                        procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().gameObject.AddComponent<AudioChorusFilter>();

                    audioProcChorus[i] = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioChorusFilter>();
                    audioProcChorus[i].enabled = false;

                }

            }





            audioProcLowPass = new AudioLowPassFilter[numNotes];

            for (int i = 0; i < procToneObj.Length; i++)
            {

                if (procToneObj[i] && procToneObj[i].gameObject.transform.GetComponent<UniProcTone>())
                {

                    if (!procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioLowPassFilter>())
                        procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().gameObject.AddComponent<AudioLowPassFilter>();

                    audioProcLowPass[i] = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioLowPassFilter>();
                    audioProcLowPass[i].cutoffFrequency = lowCutDef;
                    audioProcLowPass[i].enabled = false;

                }

            }







            audioProcDistortion = new AudioDistortionFilter[numNotes];

            for (int i = 0; i < audioProcDistortion.Length; i++)
            {

                if (procToneObj[i] && procToneObj[i].gameObject.transform.GetComponent<UniProcTone>())
                {

                    if (!procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioDistortionFilter>())
                        procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().gameObject.AddComponent<AudioDistortionFilter>();

                    audioProcDistortion[i] = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioDistortionFilter>();
                    audioProcDistortion[i].distortionLevel = 0;
                    audioProcDistortion[i].enabled = false;

                }

            }





            audioProcReverb = new AudioReverbFilter[numNotes];

            for (int i = 0; i < audioProcReverb.Length; i++)
            {

                if (procToneObj[i] && procToneObj[i].gameObject.transform.GetComponent<UniProcTone>())
                {

                    if (!procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioReverbFilter>())
                        procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().gameObject.AddComponent<AudioReverbFilter>();

                    audioProcReverb[i] = procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().GetComponent<AudioReverbFilter>();
                    audioProcReverb[i].enabled = false;

                }

            }


            for (int i = 0; i < numNotes; i++)
                if (procToneObj[i])
                    procToneObj[i].SetActive(false);

        }



    }

    void initParams()
    {
        volSet = volDef;

        //decay
        isDecayTrig = new bool[numNotes];
        isAttackTrig = new bool[numNotes];
        attackTimer = new float[numNotes];
        attacklowCutRate = new float[numNotes];
        decayRate = new float[numNotes];
        decayTail = new float[numNotes];
        lowCutShifterTimerDur = new float[numNotes];
        lowCutShifterRate = new float[numNotes];
        decayTimer = new float[numNotes];
        decayProcTimer = new float[numNotes];
        sustainTimer = new float[numNotes];

        attackTimerDur = attackTimerDurDef;

        for (int i = 0; i < numNotes; i++)
            for (int ii = 0; ii < numNotes; ii++)
            {
                decayTimer[i] = decayTimerDurDef;
                attackTimer[i] = attackTimerDur;
                attacklowCutRate[i] = attackRateDialTickDef;
                decayRate[i] = decayRateDef;
                lowCutShifterTimerDur[i] = lowCutTimerDurDef;
                lowCutShifterTimer = lowCutShifterTimerDur[i];
                lowCutShifterRate[i] = lowCutRateTick;
                decayTail[i] = decayTailDef;
                //   if(procToneObj[i] !=null)
                //     procToneObj[i].gameObject.transform.GetComponent<UniProcTone>().masterProcVol = volDef;


            }

        //sustain
        isSustainTrig = new bool[numNotes];
        sustainRate = sustainLenTickDef;
        if (sustainDur == 0)
            sustainDur = sustainTimerDurDef;

        for (int i = 0; i < numNotes; i++)
            for (int ii = 0; ii < numNotes; ii++)
                sustainTimer[i] = sustainDur;


        keyVelocity = new float[numNotes];

        //arp
        if (arpTimerDur == 0)
            arpTimerDur = 20;

        arpNote = new int[numNotes];
        arpAdv = new int[numNotes];
        arpDec = new int[numNotes];
        arpTimer = arpTimerDur;


        for (int i = 0; i < numOctaves; i++)
        {
            arpAdv[i] = 0;
            arpDec[i] = 0;
        }

        //vol/pitchdefaults
        if (volShifterTimerDur == 0)
            volShifterTimerDur = volTimerDurDef;

        volShifterTimer = volShifterTimerDur;
        volShifterRate = volRateTick;

      //  volProcAdj = volDef;

        if (pitchShifterTimerDur == 0)
            pitchShifterTimerDur = pitchTimerDurDef;

        pitchShifterTimer = pitchShifterTimerDur;
        pitchShifterRate = pitchRateTick;

        pitchSet = pitchDef + globalPitchAdj;
        pitchQuantTick = pitchQuantTickDef;


        for (int i = 0; i < audioSource.Length; i++)
            audioSource[i].pitch = pitchSet;

        keyPressTrig = new bool[numNotes];
        keyReleaseTrig = new bool[numNotes];

        isStopNote = new bool[numNotes];

        for (int i = 0; i < numNotes; i++)
        {
            arpNote[i] = -1;
            isStopNote[i] = false;
        }



        waveNameExt = new string[12];
        waveNameExt[0] = "";
        waveNameExt[1] = "_sin";
        waveNameExt[2] = "_sq";
        waveNameExt[3] = "_saw";
        waveNameExt[4] = "_pluck";
        waveNameExt[5] = "_perc";
        waveNameExt[6] = "_kit";
        waveNameExt[7] = "_usr1";
        waveNameExt[8] = "_usr2";
        waveNameExt[9] = "_usr3";
        waveNameExt[10] = "_usr4";
        waveNameExt[11] = "_usr5";

        _samples = new float[QSamples];
        _spectrum = new float[QSamples];
        _fSample = AudioSettings.outputSampleRate;



    }

    void addSounds()
    {

        soundIndex.Add("C_sin", allClips.C_sin);
        soundIndex.Add("CS_sin", allClips.CS_sin);
        soundIndex.Add("D_sin", allClips.D_sin);
        soundIndex.Add("DS_sin", allClips.DS_sin);
        soundIndex.Add("E_sin", allClips.E_sin);
        soundIndex.Add("F_sin", allClips.F_sin);
        soundIndex.Add("FS_sin", allClips.FS_sin);
        soundIndex.Add("G_sin", allClips.G_sin);
        soundIndex.Add("GS_sin", allClips.GS_sin);
        soundIndex.Add("A_sin", allClips.A_sin);
        soundIndex.Add("AS_sin", allClips.AS_sin);
        soundIndex.Add("B_sin", allClips.B_sin);

        soundIndex.Add("C_sq", allClips.C_sq);
        soundIndex.Add("CS_sq", allClips.CS_sq);
        soundIndex.Add("D_sq", allClips.D_sq);
        soundIndex.Add("DS_sq", allClips.DS_sq);
        soundIndex.Add("E_sq", allClips.E_sq);
        soundIndex.Add("F_sq", allClips.F_sq);
        soundIndex.Add("FS_sq", allClips.FS_sq);
        soundIndex.Add("G_sq", allClips.G_sq);
        soundIndex.Add("GS_sq", allClips.GS_sq);
        soundIndex.Add("A_sq", allClips.A_sq);
        soundIndex.Add("AS_sq", allClips.AS_sq);
        soundIndex.Add("B_sq", allClips.B_sq);

        soundIndex.Add("C_saw", allClips.C_saw);
        soundIndex.Add("CS_saw", allClips.CS_saw);
        soundIndex.Add("D_saw", allClips.D_saw);
        soundIndex.Add("DS_saw", allClips.DS_saw);
        soundIndex.Add("E_saw", allClips.E_saw);
        soundIndex.Add("F_saw", allClips.F_saw);
        soundIndex.Add("FS_saw", allClips.FS_saw);
        soundIndex.Add("G_saw", allClips.G_saw);
        soundIndex.Add("GS_saw", allClips.GS_saw);
        soundIndex.Add("A_saw", allClips.A_saw);
        soundIndex.Add("AS_saw", allClips.AS_saw);
        soundIndex.Add("B_saw", allClips.B_saw);

        soundIndex.Add("C_pluck", allClips.C_pluck);
        soundIndex.Add("CS_pluck", allClips.CS_pluck);
        soundIndex.Add("D_pluck", allClips.D_pluck);
        soundIndex.Add("DS_pluck", allClips.DS_pluck);
        soundIndex.Add("E_pluck", allClips.E_pluck);
        soundIndex.Add("F_pluck", allClips.F_pluck);
        soundIndex.Add("FS_pluck", allClips.FS_pluck);
        soundIndex.Add("G_pluck", allClips.G_pluck);
        soundIndex.Add("GS_pluck", allClips.GS_pluck);
        soundIndex.Add("A_pluck", allClips.A_pluck);
        soundIndex.Add("AS_pluck", allClips.AS_pluck);
        soundIndex.Add("B_pluck", allClips.B_pluck);

        soundIndex.Add("C_perc", allClips.C_perc);
        soundIndex.Add("CS_perc", allClips.CS_perc);
        soundIndex.Add("D_perc", allClips.D_perc);
        soundIndex.Add("DS_perc", allClips.DS_perc);
        soundIndex.Add("E_perc", allClips.E_perc);
        soundIndex.Add("F_perc", allClips.F_perc);
        soundIndex.Add("FS_perc", allClips.FS_perc);
        soundIndex.Add("G_perc", allClips.G_perc);
        soundIndex.Add("GS_perc", allClips.GS_perc);
        soundIndex.Add("A_perc", allClips.A_perc);
        soundIndex.Add("AS_perc", allClips.AS_perc);
        soundIndex.Add("B_perc", allClips.B_perc);

        soundIndex.Add("C_kit", allClips.C_kit);
        soundIndex.Add("CS_kit", allClips.CS_kit);
        soundIndex.Add("D_kit", allClips.D_kit);
        soundIndex.Add("DS_kit", allClips.DS_kit);
        soundIndex.Add("E_kit", allClips.E_kit);
        soundIndex.Add("F_kit", allClips.F_kit);
        soundIndex.Add("FS_kit", allClips.FS_kit);
        soundIndex.Add("G_kit", allClips.G_kit);
        soundIndex.Add("GS_kit", allClips.GS_kit);
        soundIndex.Add("A_kit", allClips.A_kit);
        soundIndex.Add("AS_kit", allClips.AS_kit);
        soundIndex.Add("B_kit", allClips.B_kit);


        soundIndex.Add("C_usr1", allClips.C_usr1);
        soundIndex.Add("CS_usr1", allClips.CS_usr1);
        soundIndex.Add("D_usr1", allClips.D_usr1);
        soundIndex.Add("DS_usr1", allClips.DS_usr1);
        soundIndex.Add("E_usr1", allClips.E_usr1);
        soundIndex.Add("F_usr1", allClips.F_usr1);
        soundIndex.Add("FS_usr1", allClips.FS_usr1);
        soundIndex.Add("G_usr1", allClips.G_usr1);
        soundIndex.Add("GS_usr1", allClips.GS_usr1);
        soundIndex.Add("A_usr1", allClips.A_usr1);
        soundIndex.Add("AS_usr1", allClips.AS_usr1);
        soundIndex.Add("B_usr1", allClips.B_usr1);

        soundIndex.Add("C_usr2", allClips.C_usr2);
        soundIndex.Add("CS_usr2", allClips.CS_usr2);
        soundIndex.Add("D_usr2", allClips.D_usr2);
        soundIndex.Add("DS_usr2", allClips.DS_usr2);
        soundIndex.Add("E_usr2", allClips.E_usr2);
        soundIndex.Add("F_usr2", allClips.F_usr2);
        soundIndex.Add("FS_usr2", allClips.FS_usr2);
        soundIndex.Add("G_usr2", allClips.G_usr2);
        soundIndex.Add("GS_usr2", allClips.GS_usr2);
        soundIndex.Add("A_usr2", allClips.A_usr2);
        soundIndex.Add("AS_usr2", allClips.AS_usr2);
        soundIndex.Add("B_usr2", allClips.B_usr2);


        soundIndex.Add("C_usr3", allClips.C_usr3);
        soundIndex.Add("CS_usr3", allClips.CS_usr3);
        soundIndex.Add("D_usr3", allClips.D_usr3);
        soundIndex.Add("DS_usr3", allClips.DS_usr3);
        soundIndex.Add("E_usr3", allClips.E_usr3);
        soundIndex.Add("F_usr3", allClips.F_usr3);
        soundIndex.Add("FS_usr3", allClips.FS_usr3);
        soundIndex.Add("G_usr3", allClips.G_usr3);
        soundIndex.Add("GS_usr3", allClips.GS_usr3);
        soundIndex.Add("A_usr3", allClips.A_usr3);
        soundIndex.Add("AS_usr3", allClips.AS_usr3);
        soundIndex.Add("B_usr3", allClips.B_usr3);

    }


    [Serializable]
    public struct AllClips
    {
        public AudioClip C_sin, CS_sin, D_sin, DS_sin, E_sin, F_sin, FS_sin, G_sin, GS_sin, A_sin, AS_sin, B_sin;
        public AudioClip C_sq, CS_sq, D_sq, DS_sq, E_sq, F_sq, FS_sq, G_sq, GS_sq, A_sq, AS_sq, B_sq;
        public AudioClip C_saw, CS_saw, D_saw, DS_saw, E_saw, F_saw, FS_saw, G_saw, GS_saw, A_saw, AS_saw, B_saw;
        public AudioClip C_pluck, CS_pluck, D_pluck, DS_pluck, E_pluck, F_pluck, FS_pluck, G_pluck, GS_pluck, A_pluck, AS_pluck, B_pluck;
        public AudioClip C_perc, CS_perc, D_perc, DS_perc, E_perc, F_perc, FS_perc, G_perc, GS_perc, A_perc, AS_perc, B_perc;
        public AudioClip C_kit, CS_kit, D_kit, DS_kit, E_kit, F_kit, FS_kit, G_kit, GS_kit, A_kit, AS_kit, B_kit;

        public AudioClip C_usr1, CS_usr1, D_usr1, DS_usr1, E_usr1, F_usr1, FS_usr1, G_usr1, GS_usr1, A_usr1, AS_usr1, B_usr1;
        public AudioClip C_usr2, CS_usr2, D_usr2, DS_usr2, E_usr2, F_usr2, FS_usr2, G_usr2, GS_usr2, A_usr2, AS_usr2, B_usr2;
        public AudioClip C_usr3, CS_usr3, D_usr3, DS_usr3, E_usr3, F_usr3, FS_usr3, G_usr3, GS_usr3, A_usr3, AS_usr3, B_usr3;
    }

    #endregion

    #region Audio filter Interfaces
    static class AudioFX
    {

        public static IEnumerator TriggerSound(AudioSource audioSource, AudioClip clip, double vol, bool _isLoop, float _keyVel)
        {

            audioSource.loop = _isLoop;
            audioSource.clip = clip;
            audioSource.volume = (float)vol + _keyVel / keyVelAdj;
            audioSource.Play();

            yield return null;

        }



        public static IEnumerator endSound(AudioSource audioSource)
        {
            audioSource.loop = false;
            audioSource.volume = 0;
            audioSource.gameObject.transform.GetComponentInParent<UniLogicChip>().momentaryTrig = true;
            yield return null;

        }

        public static IEnumerator endProcSound(GameObject audioSource)
        {
            if (audioSource && audioSource.GetComponent<UniProcTone>())
                audioSource.GetComponent<UniProcTone>().isStopAll = true;

            //  audioSource.gameObject.transform.GetComponent<UniProcTone>().masterProcVol = 1;
            yield return null;

        }

        public static IEnumerator decaySound(AudioSource audioSource, float curVol, float delay)
        {
            audioSource.volume = delay;
            yield return null;

        }

        public static IEnumerator lowFilterShifter(AudioLowPassFilter[] _filter, float _upper, float _lower, float _value, bool _isReverse, float _defVal, bool _isKey, string _type)
        {
            if (_filter != null)
            {

                if (!_isKey)
                {
                    if (_value == 0)
                        _value = _defVal;

                    if (_isReverse)
                        _value = -_value;


                    for (int i = 0; i < _filter.Length; i++)
                    {
                        float _valueChange = 0;

                        if (_type.ToUpper() == "LOWCUT")
                            _valueChange = _filter[i].cutoffFrequency;

                        if (_type.ToUpper() == "LOWRES")
                            _valueChange = _filter[i].lowpassResonanceQ;

                        _valueChange += _value;

                        if (_valueChange > _upper)
                            _valueChange = _upper;

                        if (_valueChange < _lower)
                            _valueChange = _lower;

                        if (_type.ToUpper() == "LOWCUT")
                            _filter[i].cutoffFrequency = _valueChange;

                        if (_type.ToUpper() == "LOWRES")
                            _filter[i].lowpassResonanceQ = _valueChange;


                    }
                }
                else
                {

                    float _valueChange = 0;
                    if (_type.ToUpper() == "LOWCUT")
                        _valueChange = _filter[(int)_value].cutoffFrequency;

                    if (_type.ToUpper() == "LOWRES")
                        _valueChange = _filter[(int)_value].lowpassResonanceQ;

                    _valueChange += _defVal;

                    if (_valueChange > _upper)
                        _valueChange = _upper;

                    if (_valueChange < _lower)
                        _valueChange = _lower;

                    if (_type.ToUpper() == "LOWCUT")
                        _filter[(int)_value].cutoffFrequency = _valueChange;

                    if (_type.ToUpper() == "LOWRES")
                        _filter[(int)_value].lowpassResonanceQ = _valueChange;

                }

            }

            yield return null;

        }

        public static IEnumerator highFilterShifter(AudioHighPassFilter[] _filter, float _upper, float _lower, float _value, bool _isReverse, float _defVal, bool _isKey, string _type)
        {
            if (_filter != null)
            {


                if (!_isKey)
                {
                    if (_value == 0)
                        _value = _defVal;

                    if (_isReverse)
                        _value = -_value;

                    for (int i = 0; i < _filter.Length; i++)
                    {
                        float _valueChange = 0;

                        if (_type.ToUpper() == "HIGHCUT")
                            _valueChange = _filter[i].cutoffFrequency;

                        if (_type.ToUpper() == "HIGHRES")
                            _valueChange = _filter[i].highpassResonanceQ;

                        _valueChange += _value;

                        if (_valueChange > _upper)
                            _valueChange = _upper;

                        if (_valueChange < _lower)
                            _valueChange = _lower;

                        if (_type.ToUpper() == "HIGHCUT")
                            _filter[i].cutoffFrequency = _valueChange;

                        if (_type.ToUpper() == "HIGHRES")
                            _filter[i].highpassResonanceQ = _valueChange;


                    }
                }
                else
                {

                    float _valueChange = 0;
                    if (_type.ToUpper() == "HIGHCUT")
                        _valueChange = _filter[(int)_value].cutoffFrequency;

                    if (_type.ToUpper() == "HIGHRES")
                        _valueChange = _filter[(int)_value].highpassResonanceQ;


                    _valueChange += _value;

                    if (_valueChange > _upper)
                        _valueChange = _upper;

                    if (_valueChange < _lower)
                        _valueChange = _lower;

                    if (_type.ToUpper() == "HIGHCUT")
                        _filter[(int)_value].cutoffFrequency = _valueChange;

                    if (_type.ToUpper() == "HIGHRES")
                        _filter[(int)_value].highpassResonanceQ = _valueChange;

                }

            }

            yield return null;

        }

        public static IEnumerator audioMain(AudioSource[] _filter, float _upper, float _lower, float _value, bool _isReverse, float _defVal, string _type, string _param)
        {

            if (_filter.Length == 0)
                yield return null;



            if (_param.ToUpper() != "KEY")
            {
                if (_value == 0)
                    _value = _defVal;


                if (_isReverse)
                    _value = -_value;

                for (int i = 0; i < _filter.Length; i++)
                {
                    float _valueChange = 0;

                    if (_type.ToUpper() == "VOL")
                        _valueChange = _filter[i].volume;

                    if (_type.ToUpper() == "PITCH")
                        _valueChange = _filter[i].pitch;

                    _valueChange += _value;

                    if (_valueChange > _upper)
                        _valueChange = _upper;

                    if (_valueChange < _lower)
                        _valueChange = _lower;


                    if (_type.ToUpper() == "VOL")
                        _filter[i].volume = _valueChange;

                    if (_type.ToUpper() == "PITCH")
                        _filter[i].pitch = _valueChange;

                }
            }
            if (_param.ToUpper() == "KEY")
            {

                _defVal = .05f;

                if (_isReverse)
                    _defVal = -.05f;


                float _valueChange = 0;

                if (_type.ToUpper() == "VOL")
                    _valueChange = _filter[(int)_value].volume;

                if (_type.ToUpper() == "PITCH")
                    _valueChange = _filter[(int)_value].pitch;


                _valueChange += _defVal;

                if (_valueChange > _upper)
                    _valueChange = _upper;

                if (_valueChange < _lower)
                    _valueChange = _lower;

                if (_type.ToUpper() == "VOL")
                    _filter[(int)_value].volume = _valueChange;

                if (_type.ToUpper() == "PITCH")
                    _filter[(int)_value].pitch = _valueChange;

            }
            yield return null;

        }

        public static IEnumerator EchoShifter(AudioEchoFilter[] _filter, float _upper, float _lower, float _value, bool _isReverse, float _defVal, bool _isKey, string _type)
        {
            if (_filter != null)
            {

                if (!_isKey)
                {
                    if (_value == 0)
                        _value = _defVal;

                    if (_isReverse)
                        _value = -_value;


                    for (int i = 0; i < _filter.Length; i++)
                    {
                        float _valueChange = 0;

                        if (_type.ToUpper() == "DELAY")
                            _valueChange = _filter[i].delay;

                        _valueChange += _value;

                        if (_valueChange > _upper)
                            _valueChange = _upper;

                        if (_valueChange < _lower)
                            _valueChange = _lower;

                        if (_type.ToUpper() == "DELAY")
                            _filter[i].delay = _valueChange;


                    }
                }
                else
                {

                    float _valueChange = 0;
                    if (_type.ToUpper() == "DELAY")
                        _valueChange = _filter[(int)_value].delay;



                    _valueChange += _defVal;

                    if (_valueChange > _upper)
                        _valueChange = _upper;

                    if (_valueChange < _lower)
                        _valueChange = _lower;

                    if (_type.ToUpper() == "DELAY")
                        _filter[(int)_value].delay = _valueChange;



                }

            }

            yield return null;

        }


    }

    public void AnalyzeSound()
    {

        pitchAll = "";
        dbAll = "";
        rmsAll = "";

        float[] RmsValue = new float[12];
        float[] DbValue = new float[12];


        for (int ix = 0; ix < 12; ix++)
        {
            audioSource[ix].GetOutputData(_samples, 0);

            int i;
            float sum = 0;
            for (i = 0; i < QSamples; i++)
                sum += _samples[i] * _samples[i];

            RmsValue[ix] = Mathf.Sqrt(sum / QSamples);
            DbValue[ix] = 20 * Mathf.Log10(RmsValue[ix] / RefValue);

            if (DbValue[ix] < -160)
                DbValue[ix] = -160;

            audioSource[ix].GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);

            float maxV = 0;
            var maxN = 0;
            for (i = 0; i < QSamples; i++)
            {
                if (!(_spectrum[i] > maxV) || !(_spectrum[i] > Threshold))
                    continue;

                maxV = _spectrum[i];
                maxN = i;
            }
            float freqN = maxN;

            if (maxN > 0 && maxN < QSamples - 1)
            {
                var dL = _spectrum[maxN - 1] / _spectrum[maxN];
                var dR = _spectrum[maxN + 1] / _spectrum[maxN];
                freqN += 0.5f * (dR * dR - dL * dL);
            }

            pitchValue[ix] = freqN * (_fSample / 2) / QSamples;

        }

        totalRms = 0;
        for (int i = 0; i < 12; i++)
        {
            if (pitchAll != "")
                pitchAll = pitchAll + ", " + pitchValue[i].ToString();
            else
                pitchAll = pitchValue[i].ToString();
            if (dbAll != "")
                dbAll = dbAll + ", " + DbValue[i].ToString();
            else
                dbAll = DbValue[i].ToString();
            if (rmsAll != "")
                rmsAll = rmsAll + ", " + RmsValue[i].ToString();
            else
                rmsAll = RmsValue[i].ToString();

            totalRms += RmsValue[i];


        }


    }


    #endregion


    void pitchQuant()
    {
        if (!isAnalyseSound)
        {
            isAnalyseSound = true;
            return;
        }

        string pitchTxt = "";


        for (int i = 0; i < 12; i++)
        {
            pitchTxt = pitchValue[i].ToString("#0.000");
            float pitchX = float.Parse(pitchTxt);

            if (pitchX > 0)
            {
                if (procToneObj[i].GetComponent<UniProcTone>().procFreq != pitchX)
                {
                    if (debugLev > 0)
                        Debug.Log("pitchout " + pitchX.ToString() + ", " + procToneObj[i].GetComponent<UniProcTone>().baseProcFreq.ToString());

                    if (pitchX < procToneObj[i].GetComponent<UniProcTone>().procFreq)
                    {
                        audioSource[i].pitch += (float)pitchQuantTick;
                    }
                    //   else
                    //   {

                    if (pitchX > procToneObj[i].GetComponent<UniProcTone>().procFreq)
                        audioSource[i].pitch -= (float)pitchQuantTick;
                    //   }


                }

            }



        }

    }

    void pitchQuantX(int _key)
    {
        string pitchTxt = "";
        pitchTxt = pitchValue[_key].ToString("#0.00");
        float pitchX = float.Parse(pitchTxt);

        if (procToneObj[_key].GetComponent<UniProcTone>().procFreq != pitchX)
        {
            if (debugLev > 0)
                Debug.Log("pitchout " + pitchX.ToString() + ", " + procToneObj[_key].GetComponent<UniProcTone>().baseProcFreq.ToString());

            if (pitchX < procToneObj[_key].GetComponent<UniProcTone>().procFreq)
            {
                audioSource[_key].pitch += .01f;
            }
            else
            {

                if (pitchX > procToneObj[_key].GetComponent<UniProcTone>().procFreq)
                    audioSource[_key].pitch -= .01f;
            }


        }

    }

}