using System.Collections;
using UnityEditor;
using UnityEngine;


// <<<Logic Blox v 4.4 - Universal script for logic primitives, chips and devices.

// ChipTypes added ("AND", "NAND", "OR", "NOR", "NOT","SWITCH", "UPDN")
// Updated Nov. 22, 2018 - Added Vintage components
// ChipTypes added ("NIXI", "DEK", "TRISWITCH", "THUMBWEEL")
// Updated Jan. 23, 2019 - Added Synthesizer components
// ChipTypes added ("KEYBOARD", "FOOTPEDAL")
// Updated Nov 25,2019 - Added "XOR", "XNOR", "TRIAND" gates
// Updated April 6, 2020 - Updates for ver 3 release . Added pushbutton and digital display to Logic Blox base package
// Updated April 20, 2020 - Added "METER", "FAN", "MOTOR" chip types
// Updated May 15. 2020 - Updates and fixes to object enumeration and positiion controls
// Updated July 10, 2020 - Add sim faults - ver 4 release 
// Chiptypes added ("FAN", "MOTOR")
// Updates to classes for status panel models
// Developed by Mike Hogan (2018) - Granby Games - mhogan@remhouse.com

[RequireComponent(typeof(Rigidbody))]

public class UniLogicChip : MonoBehaviour
{

    #region Constants
    // constants for gate pin mappings (do not change)
    //flip flops and counters
    const int dataPin = 0, clockPin = 1, presetPin = 2, resetPin = 3, outputPin = 4, outputQPin = 5;
    //primitives
    const int primIn0 = 0, primIn1 = 1, primOutput = 2;
    //Triprimitives
    const int tPrimIn0 = 0, tPrimIn1 = 1, tPrimIn2 = 2, tPrimOutput = 3;
    //displays
    const int countPin = 0, dirDownPin = 1, dataStatePin = 2, resetCounterPin = 3, carryOutPin = 4, dSend = 5;
    //relays
    const int relayCoilPin = 0, relayStatePin = 1, relayNOPin = 2, relayComPin = 3, relayNCPin = 4, relayReset = 5;
    const bool relayCommonLevel = true;
    //axis
    public const int dirIn0 = 0, dirIn1 = 1, dirOut0 = 2, dirOut1 = 3, speedUp = 4, speedDn = 5, tacometer = 6;
    //triswitch
    const int swIn0 = 0, swIn1 = 1, swIn2 = 2, swOut0 = 3, swOut1 = 4, swOut2 = 5;
    //thumbWheel
    const int twIn1 = 0, twInDn1 = 1, twIn2 = 2, twInDn2 = 3, twIn3 = 4, twInDn3 = 5, twOut = 6, twSend = 7, twDataEnd = 8, twReset = 9;
    //keyboard
    const int keyIn0 = 0, keyIn1 = 1, keyIn2 = 2, keyIn3 = 3, keyIn4 = 4, keyIn5 = 5, keyIn6 = 6, keyIn7 = 7, keyIn8 = 8, keyIn9 = 9, keyIn10 = 10, keyIn11 = 11, sustain = 12, keysOut = 13, keyStatusLed = 0;

    //tbutton
    const int tButIn0 = 0, tButIn1 = 1, tButIn2 = 2, tButOut0 = 3, tbutOut1 = 4, tButOut2 = 5;

    //status
    const int statNormIn = 0, statSoftErrIn = 1, statHardErrIn = 2, statClr = 3, statOut = 4, statAlarm = 5, statBut0 = 6, statBut1 = 7;

    //internal logic matrix
    public const int gateIn = 0, gateOut = 1, presetGate = 2, resetGate = 3;
    const float maxClock = 3000, minClock = .01f;
    const int maxInChain = 16;
    const float motorTimeLen = 60, windDownLen = 30;
    const float pitchShift = .2f;
    const int tagMax=24;

    #endregion


    #region System vars
    [Header("Chip Parameters")]
    public string chipType;
    public bool defaultState;
    public bool isMomentary;
    public float momDuration;


    public bool isLatch;
    bool isLatched;

   
    bool isColTrig;
    public GameObject dependantObj;
    [Header("Links")]
    public GameObject[] outputLinkObj;
    public int[] outputLinkPin;


    [Header("Command Overrides")]
    public string subFunction;
    public string funcParam;
    public float sendVal;
    public string circuitGroup;
    public string encounterGroup;
    public float delayBetweenStarts;

    [Header("B Chan Links")]
    public GameObject[] output2LinkObj;
    public int[] output2LinkPin;


    [Header("Aux Ojects")]
    public GameObject[] tacObject;


    [HideInInspector]
    public GameObject[] fanoutBusObj;

    [Header("Clock Settings")]
    public bool isClock;

    // [Range(minClock, maxClock)]
    public float clockPulseWidth;
    public float maxClockRepeats;
    public float clockPrecision;
    public bool isMaster;
    public bool isAutoStart;


    bool isRandomPulse;
    bool isGetClockFromGroup = false;

    int displayCounterValue;
    float objectGroupValue;
    bool isSerialDump;

    public bool isClockTrig;

    float momemtaryTimer, clockTimer;


    [Header("CountDown Settings")]
    public bool isCountDown;
    [Range(.001f, 5000f)]
    public float countDownDuration;
    [Range(.001f, 100f)]

    float countDownPulseWith;
    float countDownCount, countDownTimer;

    bool isCountDownTrig;


    [Header("Counter Settings")]
    public bool isCounter;
    public float counterClock;
    public float counterMax;
    bool isCounterTrig;

    [Space]

    [HideInInspector]
    public bool[] pinState;

    [HideInInspector]
    public bool[] gateState;

    [HideInInspector]
    public GameObject[] displayLedObj;
    Color[] originalDispColor;

    [HideInInspector]
    public GameObject[] displayLedSlaveObj;

    [HideInInspector]
    public bool[] driverState;

    [Header("Button/Led Settings")]
    public Color ledOn;
    Color[] originalColor;
    public string animType;    //push, pushT1, pushT2, ,pushT3, breaker,rocker,rocker2,rotate
    public int rotType;
    public float switchSteps;

    float baseSwitchSteps;
    public float maxSwitchSteps;
    public float minSwitchSteps;
    
    public bool isIllumSwitch;
    public bool isButtonHold;
    bool isSpringBackTrig;
    public bool isUnique;
    public bool isOneshot;
 
    public bool isLookAtPlayer;

    public int defaultDigVal;
    bool isButtonBlocked;

    public float defaultPos;
    public float selMaxPos;
    public float selMinPos;

    public float selCurPos;
    public float windDownTimerMax;
       

    [Header("Status Indicator")]
    public GameObject[] statusLedObj;
    Color[] originalStatColor;

    public Color statLedOn;
    bool repeatingAlarm;
    public Color softErrLed;
    public Color hardErrLed;


    [Header("Sim Fault limits")]
    public float softHighErrLimit;
    public float softLowErrLimit;

    public float hardHighErrLimit;
    public float hardLowErrLimit;
    public GameObject defaultIcon;

    public bool errOnClock, errOnStepVal, errOnCount, errOnPosition, errAtOnState;
    [HideInInspector]
    public bool isHighSoftErrTrig, isHighHardErrTrig;
    [HideInInspector]
    public bool isLowSoftErrTrig, isLowHardErrTrig;
    [HideInInspector]
    public bool isLockedState;
    [HideInInspector]
    public int alarmlevel;

    [HideInInspector]
    [Header("Pin Objects")]
    public GameObject[] pinObj;

    [HideInInspector]
    [Header("Button/Led Objects")]
    public GameObject[] ledObj;

    [HideInInspector]
    [Header("Objects to Move")]
    public GameObject[] objectGroup;

    bool[] isButtonHoldTrig, isReleaseButtonHold;

    [Header("System")]

    bool isShowLocalLinks = false;
    bool isShowMasterLinks = false;
    int currentClickPin = -1;
    bool[,] digitLedSegBit = new bool[10, 7];
    
    [HideInInspector]
    public bool momentaryTrig;

    [HideInInspector]
    public int gateLen;

    string chipClass;
    bool logicOn = true;
    const string linkMaterial = "Specular";
    const string linkMaterial2 = "Specular";

    float linkLineWidth = .01f;

    Color LineStartColor = Color.red;
    Color LineEndColor = Color.green;
    Color LineStartColor2 = Color.yellow;
    Color LineEndColor2 = Color.green;

    private MaterialPropertyBlock led_propBlock;
    private MaterialPropertyBlock dispLed_propBlock;
    private MaterialPropertyBlock statLed_propBlock;

    bool isEnableLinks = false;
    bool[] ledOnTrig, dispLedOnTrig, statusLedOnTrig, gateOnTrig;
    bool isDisableLeds;
    bool isSystemReady;
    //   bool finalResetTrig;
    float finalResetTimer, finalResetTimerMax = 30;
    bool clockPosTrig, clockNegTrig;

    bool isInReverse, isDumpData, isWindDown;

    int[] digitPosValue;
    bool preCrash;
    public bool outputState;
    bool isStateChangeTrig;

    int dumpCount;
    float prevRpm, newRpm, currentRpm;
    float motorTimer, motorTimerMax;
    float windDownTimer;

    bool isObjStructErr, isMechErr;

    Vector3 propVect;
    Quaternion propRot;
    GameObject nextObj;
    UniLogicChip nextLink;

    UniParams uniParams;

    MoveType moveType = new MoveType();
    Hashtable moveTypeIdx = new Hashtable();

    [HideInInspector]
    public Camera hudCamera;

    float degreesPerSecond = .5f;
    float amplitude = 0.2f;
    float frequency = .5f;

    bool isFloatTrig;
    bool runEffectsOnTrig,runEffectsOffTrig;
    bool runEffectsClickOnTrig, runEffectsClickOffTrig;

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();

    public int debugLevel;

    #endregion

    void Awake()
    {

     led_propBlock= new MaterialPropertyBlock();
     dispLed_propBlock = new MaterialPropertyBlock(); 
     statLed_propBlock = new MaterialPropertyBlock(); 


        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().isKinematic = true;

        hudCamera = Camera.main;


    }
       
    void Start()
    {

        if (chipType == "")
        {
            isObjStructErr = true;
        }
        else
        {
            populateMoveTypes();
            setupLogicGrid();
            chipReset(false);
        }

    }

    void Update()
    {
       
        if (!isObjStructErr)
        {

            if (isShowLocalLinks && isEnableLinks)
                drawLinks();

            updatePinsAndLeds();
            updateMotors();
            updateEffects();

        }

    

    }

    void FixedUpdate()
    {
      
        if (!isObjStructErr)
        {

            runChipLogic();
            updateLogicTimers();
            scanSimFaults();
        }



    }
    
    #region Data processing

    public void getData(int recvPin, bool state, GameObject sender)
    {
        bool isReverse = false;

        if (!isSystemReady || !sender || chipType == "" )
        {
            if (debugLevel > 1)
                Debug.LogWarning("<color=blue>" + gameObject.name + "</color> Sender has invald unilogicchip.cs");

            return;
        }

        UniLogicChip senderObj = sender.GetComponent<UniLogicChip>();
        string _sendChipType = senderObj.chipType.ToUpper();
        string _sendFunc = senderObj.subFunction.ToUpper();
        string _funcParam = senderObj.funcParam.ToUpper();
        string _localChipType = chipType.ToUpper();


        if (debugLevel > 2)
            Debug.Log("<color=blue>" + gameObject.name + "</color> start getdata");

        if (_sendFunc == "MOMLENGTH")
        {

            float dialInc = senderObj.sendVal;


            if (senderObj.gateState[dirOut0])
            {
                momDuration -= dialInc;
                if (momDuration < minClock)
                    momDuration = minClock;
            }

            if (senderObj.gateState[dirOut1])
            {
                momDuration += dialInc;
                if (momDuration > maxClock)
                    momDuration = maxClock;
            }


            return;
        }

        if (_sendFunc == "VOLTAGE")
        {

            float VoltInc = senderObj.sendVal;

            if (senderObj.gateState[dirOut0])
            {
                switchSteps -= VoltInc;
                if (switchSteps < selMinPos)
                    switchSteps = selMinPos;
            }

            if (senderObj.gateState[dirOut1])
            {
                switchSteps += VoltInc;
                if (switchSteps > selMaxPos)
                    switchSteps = selMaxPos;
            }


            return;
        }

        if (_localChipType == "KEYBOARD")
        {
            if (GetComponent<UniTones>())
                GetComponent<UniTones>().runFunc(senderObj.gameObject, senderObj.subFunction.ToString(), senderObj.funcParam.ToString(), senderObj.isInReverse, senderObj.GetComponent<UniLogicChip>().sendVal);

            if (senderObj.subFunction == "")
                pinState[recvPin] = state;

            return;

        }

        //subfunctions to other gates and controls
        if (_sendChipType == "BUFFER" || _sendChipType == "UPDN" || _sendChipType == "DIAL" || _sendChipType == "SLIDE")
        {

            // sets counter direction of click object then sends count signal
            if (isMaster && _sendFunc == "CLOCKRATE")
            {
                //  Debug.Log("send master clock");

                if (senderObj.gateState[dirOut0])
                {
                    isReverse = true;
                    pinState[dirDownPin] = true;
                    gateState[dirDownPin] = true;
                    gateState[recvPin] = state;
                    senderObj.pinState[dirDownPin] = false;

                }


                if (senderObj.gateState[dirOut1])
                {

                    isReverse = false;
                    pinState[dirDownPin] = false;
                    gateState[dirDownPin] = false;
                    gateState[recvPin] = state;
                    senderObj.pinState[dirDownPin] = false;

                }

                //set totals and direction for all chips in circuitgroup 
                if (circuitGroup != "")
                {
                    foreach (var dirObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
                    {
                        if (dirObj.GetComponentInParent<UniLogicChip>().circuitGroup == circuitGroup && dirObj.GetComponentInParent<UniLogicChip>().pinState.Length >= dirDownPin)
                        {
                            if (dirObj.GetComponentInParent<UniLogicChip>().pinState.Length >= dirDownPin)
                                dirObj.GetComponentInParent<UniLogicChip>().pinState[dirDownPin] = isReverse;
                        }

                        if (dirObj.GetComponentInParent<UniLogicChip>().circuitGroup == circuitGroup)
                        {
                            dirObj.GetComponentInParent<UniLogicChip>().objectGroupValue = objectGroupValue;

                            if (dirObj.GetComponentInParent<UniLogicChip>().isGetClockFromGroup)
                            {
                                dirObj.GetComponentInParent<UniLogicChip>().maxClockRepeats = objectGroupValue;
                                dirObj.GetComponentInParent<UniLogicChip>().countDownCount = objectGroupValue;
                                dirObj.GetComponentInParent<UniLogicChip>().clockTimer = dirObj.GetComponentInParent<UniLogicChip>().clockPulseWidth;
                            }

                        }

                    }
                }


            }

            if (_sendFunc == "CLOCKRATE")
            {

                float dialInc = senderObj.sendVal;

                if (dialInc == 0)
                {
                    dialInc = senderObj.clockPrecision;

                    if (dialInc < .1)
                        dialInc = .5f;

                }


                if (senderObj.gateState[dirOut1])
                {
                    clockPulseWidth += dialInc;
                    if (clockPulseWidth > maxClock)
                        clockPulseWidth = maxClock;


                }

                if (senderObj.gateState[dirOut0])
                {
                    clockPulseWidth -= dialInc;
                    if (clockPulseWidth < 0)
                        clockPulseWidth = 0;


                }

                momDuration = clockPulseWidth;

                return;
            }

            if (_sendFunc == "EFFECTSVOL")
            {

                float dialInc = senderObj.sendVal;
                float volInc = GetComponent<UniLogicEffects>().clipVolume;

                if (dialInc == 0)
                {
                    dialInc = senderObj.switchSteps;

                    if (dialInc == 0)
                        dialInc = 1;

                    if (dialInc < .1)
                        dialInc = .1f;
                }

                if (senderObj.gateState[dirOut0])
                {
                    volInc += dialInc;
                    if (volInc > 1)
                        volInc = 1;

                    GetComponent<UniLogicEffects>().clipVolume = volInc;
                }

                if (senderObj.gateState[dirOut1])
                {
                    volInc -= dialInc;
                    if (volInc < 0)
                        volInc = 0;

                    GetComponent<UniLogicEffects>().clipVolume = volInc;

                }

            }


        }


        if (_sendFunc == "DEVOFF" || _sendFunc == "DEVPINOFF")
        {

            if (gameObject != sender.gameObject)
            {
                isSystemReady = false;
                switchSteps = baseSwitchSteps;
                isClockTrig = false;
                isCountDownTrig = false;
                countDownCount = counterMax;
                counterClock = 0;
                maxClockRepeats = 0;
                clockTimer = 0;
                momentaryTrig = false;
                countDownTimer = countDownDuration;
                isButtonBlocked = false;
                isHighSoftErrTrig = false;
                isLowSoftErrTrig = false;
                isHighHardErrTrig = false;
                isLowHardErrTrig = false;

                if (!defaultState)
                    state = !logicOn;
                else
                    state = logicOn;


                motorTimer = 0;
                windDownTimer = 0;

                isWindDown = false;
                alarmlevel = 0;
                isLatched = false;

                if (_funcParam == "PLUSOBJECTS")
                {
                    if (GetComponent<UniLogicEffects>())
                    {
                        GetComponent<UniLogicEffects>().resetObserver();
                        GetComponent<UniLogicEffects>().resetObjects();
                    }

                    
                }

            }
           

        }


        if (_sendFunc == "RESETCAM")
        {
            resetCam();

        }

        if (_sendFunc == "CLEARLATCH" || _sendFunc == "CLEAR")
        {

            if (gameObject != sender.gameObject)
            {

                isLatched = false;

                isClockTrig = false;
                momentaryTrig = false;
                isCounterTrig = false;
                countDownCount = 0;
                counterClock = 0;
                countDownTimer = countDownDuration;
                isHighSoftErrTrig = false;
                isLowSoftErrTrig = false;
                isHighHardErrTrig = false;
                isLowHardErrTrig = false;
                isButtonBlocked = false;

            }


        }

        if (_sendFunc == "TAP")
        {

            if (!isMomentary)
                isMomentary = true;
            else
            {
                if (isMomentary)
                    isMomentary = false;
            }

        }


        if (_sendFunc == "DISPLAY" && chipType.ToUpper() == "DIGDISPLAY")
        {

            if (_funcParam == "SETVALUE")
            {
                // Debug.Log(name +" diplsy dig");
               
                    if (recvPin < 10)
                        setDisplayDigit(recvPin);

                    return;
             
            }

        }


        if (_sendFunc == "UNBLOCK")
        {

            isButtonBlocked = false;

        }

        if (_sendFunc == "PINSHIFT")
        {

            if (!senderObj.GetComponent<UniLogicChip>().momentaryTrig)

            {
                int _selPin = 0;
                int _curPin = -1;


                if (outputLinkPin.Length > 0)
                    _curPin = outputLinkPin[0];


                if (_curPin > 0 && senderObj.GetComponent<UniLogicChip>().isInReverse)
                    _selPin = -1;


                if (_curPin < 12 && !senderObj.GetComponent<UniLogicChip>().isInReverse)
                    _selPin = 1;


                if (outputLinkObj.Length > 0 && outputLinkPin.Length > 0)
                    outputLinkPin[0] = _curPin + _selPin;




            }
        }

        // enaable/disable leds
        if (_sendFunc == "DISABLECIRCUITLEDS")
        {

            if (GetComponent<UniLogicEffects>())
            {

                foreach (var resetObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
                {
                    if (resetObj.GetComponentInParent<UniLogicChip>().subFunction.ToUpper() != "POWER")
                    {

                        if (resetObj.GetComponentInParent<UniLogicChip>().circuitGroup == senderObj.circuitGroup)
                        {
                            if (!resetObj.GetComponentInParent<UniLogicChip>().isDisableLeds)
                            {
                                resetObj.GetComponentInParent<UniLogicChip>().isDisableLeds = true;

                                for (int i = 0; i < resetObj.displayLedObj.Length; i++)
                                {
                                    resetObj.displayLedObj[i].GetComponent<Renderer>().material.color = originalDispColor[i];
                                    setEmission(resetObj.displayLedObj[i], false, originalDispColor[i]);
                                }

                            }
                            else
                                resetObj.GetComponentInParent<UniLogicChip>().isDisableLeds = false;
                        }

                    }
                }
            }
        }

        //set zero didgit  value to TRUE for deks and niks
        if (_localChipType == "DEKATRON" || _localChipType == "UNDEKATRON" || _localChipType == "NIXI" || _localChipType == "DIGDISPLAY")
        {

            if (senderObj.subFunction == "")
            {
                if (!isDumpData)
                    isClockTrig = false;

                if (recvPin <= pinState.Length)
                {
                  //  Debug.Log("<color=blue>" + gameObject.name + " </color> recvPin" +recvPin);

                    pinState[recvPin] = state;
                }
            }

            gateState[carryOutPin] = false;

            return;


        }


        if (recvPin == 0 || recvPin <= gateState.Length)
        {

            if (_sendFunc == "DEVON")
                state = true;

            if (gateState.Length > recvPin)
            {
                gateState[recvPin] = state;
                pinState[recvPin] = state;
            }

            isSystemReady = true;
            if (debugLevel > 2)
                Debug.Log("<color=blue>" + gameObject.name + " </color> process GETDATA from " + sender.name + " to pin " + recvPin + ", State = " + state);


        }
        else
        if (debugLevel > 1)
            Debug.LogWarning("<color=blue>" + gameObject.name + "</color> pin index mismatch. Gate length = " + gateState.Length + " RecvPin = " + recvPin + " State = " + state);

    }

    public void setExtData(int sendPin, bool state, int channel)
    {

        string _localChipType = chipType.ToUpper();
        string _localChipClass = chipClass.ToLower();
        outputState = state;

        if (channel == 0)
        {
            if (outputState)
                runEffectsOnTrig = true;
            else
                runEffectsOffTrig = true;
        }

        
        if (dependantObj != null)
        {
            if (dependantObj.GetComponent<UniLogicChip>().outputState == false)
            {
              //  Debug.Log("<color=blue>" + name + "</color> dependant is off - setdata");
                return;
            }
        }


        if (channel < 3)
        {
            // all others
            if (outputLinkObj != null && outputLinkObj.Length > 0 && outputLinkPin.Length > 0)
            {
                if (_localChipClass != "thumbwheel" && _localChipClass != "scorewheel" && _localChipClass != "footpedal")
                {
                    if (channel == 0)
                    {
                        for (int i = 0; i < outputLinkObj.Length; i++)
                        {
                            if (outputLinkObj[i] && outputLinkObj[i].GetComponent<UniLogicChip>())
                            {
                                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipType != "")
                                {
                                    //sender is not countdown chip
                                    if (!outputLinkObj[i].GetComponent<UniLogicChip>().isCountDown && !outputLinkObj[i].GetComponent<UniLogicChip>().isCountDownTrig)
                                    {
                                        if (outputLinkObj.Length > 0 && outputLinkPin.Length > 0)
                                        {
                                            if (outputLinkObj[i].gameObject != gameObject)
                                            {

                                                //   if (outputLinkObj[i].GetComponent<UniLogicChip>().outputLinkPin.Length > 0)
                                                //   {

                                                if (subFunction.ToUpper() != "DISPLAY")
                                                {
                                                    outputLinkObj[i].GetComponent<UniLogicChip>().getData(outputLinkPin[i], state, gameObject);

                                                    if (debugLevel > 0)
                                                        Debug.Log("<color=blue>" + name + "</color> Channel = " + channel + " pin: " + sendPin.ToString() + " sends " + state.ToString() + " state to external object: <color=blue>" + outputLinkObj[i].name + "</color>, pin:" + outputLinkPin[i].ToString());
                                                    
                                                }


                                                if (subFunction.ToUpper() == "DISPLAY")
                                                {
                                                    outputLinkObj[i].GetComponent<UniLogicChip>().getData((int)sendVal, state, gameObject);

                                                    if (debugLevel > 0)
                                                        Debug.Log("<color=blue>" + name + "</color> Channel = " + channel + " pin: " + sendPin.ToString() + " sends " + state.ToString() + " state to external object: <color=blue>" + outputLinkObj[i].name + "</color>, pin:" + outputLinkPin[i].ToString());

                                                }

                                                //  }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // retrigger countdown advance
                                        outputLinkObj[i].GetComponent<UniLogicChip>().isCountDownTrig = true;

                                        if (outputLinkObj[i].gameObject != gameObject)
                                        {
                                            if (i <= outputLinkObj[i].GetComponent<UniLogicChip>().outputLinkPin.Length)
                                            {
                                                outputLinkObj[i].GetComponent<UniLogicChip>().getData(outputLinkPin[i], false, gameObject);
                                            //    outputState = false;

                                                if (debugLevel > 0)
                                                    Debug.Log("Gate: <color=blue>" + name + "</color> Channel = " + channel + " pin: " + sendPin.ToString() + " sends " + state.ToString() + " state to external object: <color=blue>" + outputLinkObj[i].name + "</color>, pin:" + outputLinkPin[i].ToString());
                                            }

                                        }


                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("<color=blue>" + gameObject.name + "</color> link failed due to: " + outputLinkObj[i].name + " does not have ChipType defined");
                                }
                            }

                        }
                    }

                    if (channel == 1)
                        for (int i = 0; i < output2LinkObj.Length; i++)
                        {
                            if (output2LinkObj[i] && output2LinkObj[i].GetComponent<UniLogicChip>())
                            {
                                if (output2LinkObj[i].GetComponent<UniLogicChip>().chipType != "")
                                {

                                    //sender is not countdown chip
                                    if (!output2LinkObj[i].GetComponent<UniLogicChip>().isCountDown && !output2LinkObj[i].GetComponent<UniLogicChip>().isCountDownTrig)
                                    {
                                        if (output2LinkObj.Length > 0)
                                        {
                                            if (output2LinkObj[i] != null && output2LinkObj[i].gameObject != gameObject)
                                            {
                                                output2LinkObj[i].GetComponent<UniLogicChip>().getData(output2LinkPin[i], state, gameObject);

                                                if (debugLevel > 0)
                                                    Debug.Log("Gate: <color=blue>" + name + "</color> Channel = " + channel + " pin: " + sendPin + " sends " + state + " state to external object: <color=blue>" + output2LinkObj[i].name + "</color>, pin:" + output2LinkPin[i]);
                                            }
                                        }
                                        else
                                            Debug.Log(gameObject.name + " link pin array missmatch. Did you miss an Output2LinkPin ?");
                                    }
                                    else
                                    {
                                        // retrigger countdown advance
                                        output2LinkObj[i].GetComponent<UniLogicChip>().isCountDownTrig = true;
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning(gameObject.name + " link failed due to: " + output2LinkObj[i].name + " does not have ChipType defined");
                                }
                            }

                        }



                }

            }


            if (_localChipClass == "thumbwheel" || _localChipClass == "scorewheel")
            {
                if (channel == 0)
                {
                    if (isSerialDump)
                        sendPin = 0;


                    if (outputLinkObj.Length > 0 && sendPin <= outputLinkObj.Length && outputLinkPin.Length > 0)
                        outputLinkObj[sendPin].GetComponent<UniLogicChip>().getData(outputLinkPin[sendPin], state, gameObject);


                }

                if (channel == 1)
                {
                    if (output2LinkObj.Length > 0 && output2LinkObj[sendPin] != null && sendPin <= output2LinkObj.Length)
                        if (output2LinkObj[sendPin])
                            output2LinkObj[sendPin].GetComponent<UniLogicChip>().getData(output2LinkPin[sendPin], state, gameObject);

                }

                if (channel == 2)
                {

                    if (output2LinkObj.Length > 0 && output2LinkObj[sendPin] != null && sendPin <= output2LinkObj.Length)
                        if (output2LinkObj[sendPin].GetComponent<UniLogicChip>().chipClass.ToUpper() == "VALVE" || output2LinkObj[sendPin].GetComponent<UniLogicChip>().chipClass.ToUpper() == "AXIS" || output2LinkObj[sendPin].GetComponent<UniLogicChip>().chipClass.ToUpper() == "DISPLAY")
                            if (output2LinkObj[sendPin])
                                output2LinkObj[sendPin].GetComponent<UniLogicChip>().getData(dirDownPin, state, gameObject);

                }

            }

            if (_localChipType == "FOOTPEDAL")
            {
                if (channel == 0 && output2LinkObj.Length > 0 && output2LinkObj[0])
                    output2LinkObj[0].GetComponent<UniLogicChip>().getData(output2LinkPin[0], state, gameObject);

                if (channel == 1 && output2LinkObj.Length > 1 && output2LinkObj[1])
                    output2LinkObj[1].GetComponent<UniLogicChip>().getData(output2LinkPin[1], state, gameObject);

                if (channel == 2 && output2LinkObj.Length > 2 && output2LinkObj[2])
                    output2LinkObj[2].GetComponent<UniLogicChip>().getData(output2LinkPin[2], state, gameObject);



            }

            if ((_localChipType == "DEKATRON" || _localChipType == "UNDEKATRON") && channel == 1)
            {
                if (channel == 1 && sendPin < output2LinkObj.Length && output2LinkObj[sendPin])
                    for (int i = 0; i < output2LinkObj.Length; i++)
                        if (sendPin == i)
                            output2LinkObj[sendPin].GetComponent<UniLogicChip>().getData(output2LinkPin[sendPin], state, gameObject);

            }


         //   if (chipType.ToUpper() != "DIGDISPLAY" && chipType.ToUpper() != "DEKATRON" && chipType.ToUpper() != "UNDEKATRON")
         //   {

        //        setStatusLed(state);

       //     }


        }

        if (channel == 3)
        {

            for (int i = 0; i < statusLedObj.Length; i++)
            {
                if (statusLedObj[i] && statusLedObj[i] != null)
                {

                    if (statusLedObj[i].GetComponent<UniLogicChip>() && statusLedObj[i].GetComponent<UniLogicChip>().chipType.ToUpper() == "STATUS")
                    {
                        statusLedObj[i].GetComponent<UniLogicChip>().getData(sendPin, state, gameObject);
                        if (debugLevel > 0)
                            Debug.Log("<color=blue>" + name + "</color> Channel = " + channel + " pin: " + sendPin + " sends " + state + " state to external object: <color=blue>" + statusLedObj[i].name + "</color>, pin:" + sendPin);
                    }


                }
            }
        }

        if (channel == 4)
        {

            for (int i = 0; i < tacObject.Length; i++)
            {
                if (tacObject[i] != null)
                    tacObject[i].GetComponent<UniLogicChip>().getData(0, state, gameObject);

            }

        }

        if (channel == 5) //chanel to start next encounter
        {

            if (GetComponent<UniLogicEffects>() && GetComponent<UniLogicEffects>().nextEncounterObj.GetComponent<UniLogicChip>())
            {
                int xpin = GetComponent<UniLogicEffects>().nextEncounterPin;
                GetComponent<UniLogicEffects>().nextEncounterObj.GetComponent<UniLogicChip>().getData(xpin, true, gameObject);
            }

        }


        
        if (chipType.ToUpper() != "DIGDISPLAY" && chipType.ToUpper() != "DEKATRON" && chipType.ToUpper() != "UNDEKATRON")
           setStatusLed(state);
             

        updatePinsAndLeds();

        if (state && channel==0 &&  chipClass.ToUpper()!="VALVE" && chipClass.ToUpper() != "DISPLAY")
           runGlobalFunctions();
           

    }

    void signalFanout(int pinType)
    {
        if (fanoutBusObj != null && fanoutBusObj.Length > 0 && outputLinkPin.Length > 0)
        {
            for (int i = 0; i < fanoutBusObj.Length; i++)
            {
                if (fanoutBusObj[i] && fanoutBusObj[i].GetComponent<UniLogicChip>())
                {
                    if (fanoutBusObj[i].GetComponent<UniLogicChip>().gateLen > 0 && pinType <= fanoutBusObj[i].GetComponent<UniLogicChip>().gateLen)
                    {
                        if (fanoutBusObj[i].gameObject != gameObject)
                            fanoutBusObj[i].GetComponent<UniLogicChip>().getData(pinType, true, gameObject);
                    }
                }
            }
        }

    }

    #endregion

    #region Mouse actions

    void OnMouseDown()
    {
          if (isButtonBlocked)
            return;

        int pinInt = clickPart(gameObject);

        if (pinInt > -1)
        {

            currentClickPin = pinInt;

            if (chipType.ToUpper() == "KEYBOARD" && GetComponent<UniTones>())
            {
                GetComponent<UniTones>().procKeyDown(pinInt);
                pinState[pinInt] = true;
                return;
            }

            if (!isClockTrig)
            {

                if (pinState[pinInt] == false)
                {

                    pinState[pinInt] = true;
                    gateState[pinInt] = true;
                    runEffectsClickOnTrig = true;
                }
                else
                {
                    if (pinState[pinInt] == true && !isLatched)
                    {
                        pinState[pinInt] = false;
                        gateState[pinInt] = false;
                        runEffectsClickOffTrig = true;
                    }
                }


                if (chipType.ToUpper() == "BUFFER")
                {
                    if (isLatched && pinInt == 1)
                    {

                        isLatched = false;
                        pinState[0] = false;
                        gateState[0] = false;


                    }

                }


            }

            if (isClockTrig)
            {
                isClockTrig = false;
                gateState[pinInt] = false;
                pinState[pinInt] = false;

            }
            else
             if (isClock)
                isClockTrig = true;


            if (isCountDown)
            {
                if (isCountDownTrig)
                {
                    isCountDownTrig = false;
                    gateState[pinInt] = false;
                    pinState[pinInt] = false;
                    countDownTimer = countDownDuration;
                }
                else
                {
                    if (gateState[pinInt] == true)
                    {
                        gateState[pinInt] = false;
                        pinState[pinInt] = false;

                        isCountDownTrig = true;
                        ledObj[0].GetComponent<Renderer>().material.color = ledOn;
                        countDownTimer = countDownDuration;

                    }

                }

            }

        }



    }

    void OnMouseEnter()
    {
        //Debug.Log("mouse over " + gameObject.name);
    }

    void OnMouseUp()
    {

        if (currentClickPin > -1 && currentClickPin < 12 && GetComponent<UniTones>())
        {
            GetComponent<UniTones>().procKeyUp(currentClickPin);
            // GetComponent<UniTones>().keyReleaseTrig[currentClickPin] = true;

        }


        if (isButtonHold)
        {
            for (int i = 0; i < ledObj.Length; i++)
            {
                if (isButtonHoldTrig[i])
                {
                    isReleaseButtonHold[i] = true;
                    isButtonHoldTrig[i] = false;

                    if (chipClass.ToLower() == "axis")
                    {
                        pinState[dirIn0] = false;
                        pinState[dirIn1] = false;
                        gateState[dirIn0] = false;
                        gateState[dirIn1] = false;

                    }

                }

            }

        }

        //   if (isMomentary)
        //   {
        //       momentaryTrig = true;
        //  }

        if (!isMomentary)
        {

            if (chipType.ToUpper() == "FLIPPER")
            {
                gateState[gateIn] = false;
            }
        }


        currentClickPin = -1;

    }

    #endregion

    #region Collision actions

    void OnCollisionEnter(Collision collision)
    {
        //  if (isColTrig)
        // {
        //     if (!preCrash)
        //           gateState[0] = true;
        //   }

    }

    void OnCollisionExit(Collision collision)
    {
        // if (isStatLatch)
        //  {
        //   gateState[0] = false;
        //   }

        //  isSpringBackTrig = true;
    }

    #endregion

    #region Remote and global commands
    void runGlobalFunctions()
    {
        if (preCrash || subFunction == "")
            return;

     
      //  Debug.Log("Scan gobal functions");


        if (subFunction.ToUpper() == "FLOAT")
        {

            foreach (var obj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
            {

                if (!obj.GetComponent<UniLogicChip>().isFloatTrig)
                {
                    obj.GetComponent<UniLogicChip>().isFloatTrig = true;
                    obj.GetComponent<UniLogicChip>().GetComponent<Rigidbody>().isKinematic = false;
                }
                else
                {
                    if (obj.GetComponent<UniLogicChip>().isFloatTrig)
                    {
                        obj.GetComponent<UniLogicChip>().isFloatTrig = false;
                        obj.GetComponent<UniLogicChip>().GetComponent<Rigidbody>().isKinematic = true;
                    }
                }



            }


        }

        if (subFunction.ToUpper() == "LOADBALL")
        {

            foreach (var machObj in FindObjectsOfType(typeof(UniPinBallMach)) as UniPinBallMach[])
            {

                if (machObj.GetComponentInParent<UniPinBallMach>().circuitGroup == circuitGroup)
                    machObj.GetComponentInParent<UniPinBallMach>().isLoadNewBall = true;

            }

            return;
        }

        if (subFunction.ToUpper() == "LAUNCHBALL")
        {

            foreach (var machObj in FindObjectsOfType(typeof(UniPinBallMach)) as UniPinBallMach[])
            {

                if (machObj.GetComponentInParent<UniPinBallMach>().circuitGroup == circuitGroup)
                    machObj.GetComponentInParent<UniPinBallMach>().isLaunchBall = true;

            }

            return;
        }

        if (subFunction.ToUpper() == "SHOWLINKS")
        {


            if (debugLevel > 0)
                Debug.Log("run sub function showlinks");

            if (!isShowMasterLinks)
            {

                foreach (var linkLineObj in FindObjectsOfType(typeof(LineRenderer)) as LineRenderer[])
                {
                    if (linkLineObj.GetComponent<UniLogicChip>())
                    {
                        linkLineObj.enabled = true;
                        linkLineObj.GetComponent<UniLogicChip>().defineLinkLines(true);
                        linkLineObj.GetComponent<UniLogicChip>().isShowLocalLinks = true;

                    }
                }

                isShowMasterLinks = true;
            }
            else
            {
                if (isShowMasterLinks)
                {
                    if (debugLevel > 0)
                        Debug.Log("run sub function clearlinks");

                    foreach (var linkLineObj in FindObjectsOfType(typeof(LineRenderer)) as LineRenderer[])
                    {
                        if (linkLineObj.GetComponent<UniLogicChip>())
                        {
                            linkLineObj.GetComponent<UniLogicChip>().isShowLocalLinks = false;
                            linkLineObj.GetComponent<UniLogicChip>().clearLinkLines();
                            linkLineObj.enabled = false;
                        }

                    }

                    isShowMasterLinks = false;
                    clearLinkLines();
                }
            }

        }

        if (subFunction.ToUpper() == "DEBUG")
        {
            if (debugLevel > 0)
                Debug.Log("Run sub function to toggle Debug");

            if (debugLevel < 1)
            {
                foreach (var debugObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
                    if (debugObj.GetComponentInParent<UniLogicChip>().debugLevel < 1)
                        debugObj.GetComponentInParent<UniLogicChip>().debugLevel = 1;
            }
            else
            {
                foreach (var debugObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
                    debugObj.GetComponentInParent<UniLogicChip>().debugLevel = 0;


            }
        }

        if (subFunction.ToUpper() == "RESET")
        {

            if (funcParam.ToUpper() == "CIRCUIT" && circuitGroup != "")
            {

                foreach (var effectObj in FindObjectsOfType(typeof(UniLogicEffects)) as UniLogicEffects[])
                {
                    if (effectObj.GetComponent<UniLogicChip>().circuitGroup.ToUpper() == circuitGroup.ToUpper())
                    {
                        if (!effectObj.GetComponent<UniLogicChip>().isLockedState)
                        {

                            for (int i = 0; i < effectObj.GetComponent<UniLogicEffects>().toggleOnObject.Length; i++)
                            {

                                effectObj.resetObjects();
                                effectObj.resetObserver();
             
                            }

                        }
                    }
                }


                foreach (var resetObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
                {
                    if (resetObj.GetComponent<UniLogicChip>().circuitGroup.ToUpper() == circuitGroup.ToUpper())
                    {
                        if (!resetObj.GetComponent<UniLogicChip>().isLockedState)
                        {
              
                            resetObj.GetComponent<UniLogicChip>().chipReset(true);
                         
                        }
                    }
                }

               



                if (debugLevel > 0)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> Run sub function to reset circuit group <color=purple>" + circuitGroup + "</color>");


            }

            if (funcParam.ToUpper() == "ALL")
            {
                resetGlobal();
            }

            if (funcParam.ToUpper() == "CAMS")
            {
                foreach (var effectObj in FindObjectsOfType(typeof(UniLogicEffects)) as UniLogicEffects[])
                {

                    effectObj.GetComponent<UniLogicEffects>().resetObserver();
                            
                }
            }
        }

        if (subFunction.ToUpper() == "DISABLELEDS")
        {
            if (debugLevel > 0)
                Debug.Log("Run sub function to toggle led power");

            foreach (var resetObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
            {

                if (resetObj.GetComponentInParent<UniLogicChip>().subFunction.ToUpper() != "POWER")
                {
                    if (!resetObj.GetComponentInParent<UniLogicChip>().isDisableLeds)
                    {
                        resetObj.GetComponentInParent<UniLogicChip>().isDisableLeds = true;

                        for (int i = 0; i < resetObj.displayLedObj.Length; i++)
                        {
                            if (resetObj != gameObject)
                            {
                                resetObj.displayLedObj[i].GetComponent<Renderer>().material.color = originalDispColor[i];
                                setEmission(resetObj.displayLedObj[i], false, originalDispColor[i]);
                            }
                        }

                    }
                    else
                        resetObj.GetComponentInParent<UniLogicChip>().isDisableLeds = false;

                }
            }


        }

        if (subFunction.ToUpper() == "CRASH")
        {


            foreach (var debugObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
            {
                if (debugObj.GetComponent<UniLogicChip>().GetComponent<Rigidbody>() && debugObj.GetComponent<UniLogicChip>().GetComponent<Rigidbody>().isKinematic)
                {

                    if (funcParam.ToUpper() == "ALL" || (funcParam.ToUpper() == "CIRCUIT" && (debugObj.GetComponent<UniLogicChip>().circuitGroup.ToUpper() == circuitGroup.ToUpper())))
                    {

                        if (debugObj != gameObject && !debugObj.GetComponent<UniLogicChip>().isLockedState)
                        {
                            debugObj.GetComponent<UniLogicChip>().GetComponent<UniLogicChip>().preCrash = true;

                           debugObj.GetComponent<UniLogicChip>().GetComponent<Rigidbody>().isKinematic = false;

                        //    debugObj.GetComponent<UniLogicChip>().GetComponent<Rigidbody>().useGravity = true;

                            //    debugObj.GetComponentInParent<UniLogicChip>().GetComponent<UniLogicChip>().isClock = false;
                            //    debugObj.GetComponentInParent<UniLogicChip>().GetComponent<UniLogicChip>().isClockTrig = false;
                        }
                    }
                }


            }


            Debug.Log("<color=blue>" + gameObject.name + "</color> Run sub function <color=purple>" + subFunction + "</color> " + funcParam + " " + circuitGroup);

        }

        if (subFunction.ToUpper() == "GLOBALLIGHTSTRIPDELAY")
        {

            foreach (var debugObj in FindObjectsOfType(typeof(UniLightShow)) as UniLightShow[])
            {
                if (sendVal == 0)
                    sendVal = switchSteps;

                float _rateChange = debugObj.decayLenVal;

                if (!isInReverse)
                {
                    _rateChange += sendVal;
                    if (_rateChange > 100)
                        _rateChange = 100;
                }
                else
                {
                    _rateChange -= sendVal;

                    if (_rateChange <= 0)
                        _rateChange = 1;

                }


                debugObj.decayLenVal = _rateChange;

            }


        }

        if (subFunction.ToUpper() == "RESTOREPOS")
        {

            preCrash = false;

            foreach (var obj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
            {
                if (funcParam.ToUpper() == "ALL" || (funcParam.ToUpper() == "CIRCUIT" && (obj.GetComponent<UniLogicChip>().circuitGroup.ToUpper() == circuitGroup.ToUpper())))
                {

                    if (obj.GetComponent<Rigidbody>())
                    {
                        obj.GetComponent<Rigidbody>().isKinematic = true;
                        obj.GetComponent<UniLogicChip>().preCrash = false;
                        obj.GetComponent<UniLogicChip>().isHighHardErrTrig = false;
                        obj.GetComponent<UniLogicChip>().isLowHardErrTrig = false;
                        obj.GetComponent<UniLogicChip>().isHighSoftErrTrig = false;
                        obj.GetComponent<UniLogicChip>().isLowSoftErrTrig = false;
                        obj.GetComponent<UniLogicChip>().transform.SetPositionAndRotation(obj.GetComponent<UniLogicChip>().propVect, obj.GetComponent<UniLogicChip>().propRot);
                    }

                }

            }

           // Debug.Log("<color=blue>" + gameObject.name + "</color> Run sub function <color=purple>" + subFunction + "</color> " + funcParam + " " + circuitGroup);

        }

        if (subFunction.ToUpper() == "MOVEOBJ")
        {


            if (funcParam == "UP")
            {
                foreach (var obj in FindObjectsOfType(typeof(UniNav)) as UniNav[])
                {

                    if (!obj.GetComponent<UniNav>().isGoForward)
                    {
                        obj.GetComponent<UniNav>().isGoForward = true;
                    }
                    else
                    {
                        if (obj.GetComponent<UniNav>().isGoForward)
                        {
                            obj.GetComponent<UniNav>().isGoForward = false;
                        }
                    }
                }
            }

            if (funcParam == "LEFT")
            {
                foreach (var obj in FindObjectsOfType(typeof(UniNav)) as UniNav[])
                {

                    if (!obj.GetComponent<UniNav>().isGoRight)
                    {
                        obj.GetComponent<UniNav>().isGoRight = true;
                    }
                    else
                    {
                        if (obj.GetComponent<UniNav>().isGoRight)
                        {
                            obj.GetComponent<UniNav>().isGoRight = false;
                        }
                    }
                }
            }

            if (funcParam == "FWD")
            {
                foreach (var obj in FindObjectsOfType(typeof(UniNav)) as UniNav[])
                {

                    if (!obj.GetComponent<UniNav>().isGoUp)
                    {
                        obj.GetComponent<UniNav>().isGoUp = true;
                    }
                    else
                    {
                        if (obj.GetComponent<UniNav>().isGoUp)
                        {
                            obj.GetComponent<UniNav>().isGoUp = false;
                        }
                    }
                }
            }

            if (funcParam == "SPEEDUP")
            {
                foreach (var obj in FindObjectsOfType(typeof(UniNav)) as UniNav[])
                {
                    if (circuitGroup.ToLower() == obj.GetComponent<UniNav>().circuitGroup.ToLower())
                    {
                        obj.GetComponent<UniNav>().speed = obj.GetComponent<UniNav>().speed + sendVal;
                    }
                }
            }

            if (funcParam == "SPEEDDOWN")
            {
                foreach (var obj in FindObjectsOfType(typeof(UniNav)) as UniNav[])
                {
                    if (circuitGroup.ToLower() == obj.GetComponent<UniNav>().circuitGroup.ToLower())
                    {
                        obj.GetComponent<UniNav>().speed = obj.GetComponent<UniNav>().speed - sendVal;
                    }
                }
            }

            if (funcParam == "RANDOMANGLE")
            {
                foreach (var obj in FindObjectsOfType(typeof(UniNav)) as UniNav[])
                {
                    if (obj.GetComponent<UniNav>())
                    {
                        if (!obj.GetComponent<UniNav>().isRandomAngle)
                        {
                            obj.GetComponent<UniNav>().isRandomAngle = true;
                        }
                        else
                        {
                            if (obj.GetComponent<UniNav>().isRandomAngle)
                            {
                                obj.GetComponent<UniNav>().isRandomAngle = false;
                            }
                        }
                    }
                }
            }

        }

        if (subFunction.ToUpper() == "ENABLE")
        {

            if (funcParam.ToUpper() == "CAMS")
            {

                if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + " </color> enable cams");


                foreach (var obj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
                {

                    if (obj.GetComponent<UniLogicEffects>())
                    {
                        if (obj.GetComponent<UniLogicEffects>().isGotoObserver == false)
                        {
                            obj.GetComponent<UniLogicEffects>().isGotoObserver = true;
                        }
                        else
                        {
                            if (obj.GetComponent<UniLogicEffects>().isGotoObserver == true)
                            {
                                obj.GetComponent<UniLogicEffects>().isGotoObserver = false;
                            }
                        }

                    }

                }


            }

        }

        


    }

    #endregion

    #region Core logic engine and Timers

    void runChipLogic()
    {
        if (chipType == "")
            return;

        string _chipType = chipType.ToUpper();

        if (_chipType == "KEYBOARD")
        {

            for (int i = 0; i < 12; i++)
            {
                if (GetComponent<UniTones>() && !GetComponent<UniTones>().keyReleaseTrig[i] && GetComponent<UniTones>().keyPressTrig[i])
                {
                    pinState[i] = true;
                    momentaryTrig = true;
                    GetComponent<UniTones>().keyPressTrig[i] = false;
                }

                // switch is pressed


                if (pinState[i] && !gateState[i])
                {
                    gateState[i] = true;
                    gateOnTrig[i] = true;

                    if (objectGroup.Length > 0 && objectGroup[i])
                        moveLocalPart(objectGroup[i], switchSteps, rotType);

                 //   if (statusLedObj[keyStatusLed])
                 //       setStatusLed(true);


                    GetComponent<UniTones>().playAudio(ledObj[i].name, GetComponent<UniTones>().octave, i);
                    setExtData(keysOut, logicOn, 0);
                

                }

                //switch is released
                if (!pinState[i] && gateState[i] == true && !GetComponent<UniTones>().keyPressTrig[i])
                {
                    gateState[i] = false;
                    gateOnTrig[gateOut] = false;

                    if (objectGroup.Length > 0 && objectGroup[i])
                        moveLocalPart(objectGroup[i], -switchSteps, rotType);

                    GetComponent<UniTones>().isStopNote[i] = true;
                    setExtData(keysOut, false, i);
           
                //    if (statusLedObj[keyStatusLed])
                //        setStatusLed(false);

                }

                // needs to stay
                if (GetComponent<UniTones>().keyReleaseTrig[i] && GetComponent<UniTones>().keyPressTrig[i])
                {
                    GetComponent<UniTones>().keyPressTrig[i] = false;
                    GetComponent<UniTones>().keyReleaseTrig[i] = false;
                    momentaryTrig = true;

                }

            }
        }

        if (_chipType == "TBUTTON")
        {

            for (int i = 0; i < 3; i++)
            {

                if (gateState[i] && !gateOnTrig[i])
                {
                    pinState[i] = true;
                    gateOnTrig[i] = true;

                    gateState[i + 3] = true;
                    setExtData(i + 3, true, 0);
              
                 //   runGlobalFunctions();

                    if (isMomentary)
                        momentaryTrig = true;


                }


                if (!gateState[i] && gateOnTrig[i])
                {
                    pinState[i] = false;
                    gateOnTrig[i] = false;

                    gateState[i + 3] = false;
                    setExtData(i + 3, false, 0);
                 
                  //  runGlobalFunctions();


                }
            }
        }

        if (_chipType == "STATUS")
        {

            for (int i = 0; i < gateState.Length; i++)
            {
               
                if (gateState[i] && !gateOnTrig[i])
                {
                   
                    pinState[i] = true;

                    gateState[statOut] = true;
                    setExtData(statOut, true, 0);

                    if (i == statSoftErrIn)
                    {

                        gateState[statHardErrIn] = false;
                        gateOnTrig[statHardErrIn] = false;

                        gateState[statNormIn] = false;
                        gateOnTrig[statNormIn] = false;

                        gateOnTrig[statSoftErrIn] = true;


                       
                    }

                    if (i == statHardErrIn)
                    {
                      
                        gateState[statSoftErrIn] = false;
                        gateOnTrig[statSoftErrIn] = false;

                        gateState[statNormIn] = false;
                        gateOnTrig[statNormIn] = false;

                        gateOnTrig[statHardErrIn] = true;

                    
                        
                    }

                    if (i == statNormIn)
                    {
                       
                        gateState[statSoftErrIn] = false;
                        gateOnTrig[statSoftErrIn] = false;

                        gateState[statHardErrIn] = false;
                        gateOnTrig[statHardErrIn] = false;

                        gateOnTrig[statNormIn] = true;

                   

                        
                    }



                    if (isMomentary)
                        momentaryTrig = true;
                }


                if (!gateState[i] && gateOnTrig[i] && i != statNormIn && i != statSoftErrIn && i != statHardErrIn)
                {
                    pinState[i] = false;
                    gateOnTrig[i] = false;

                    gateState[statOut] = false;
                    setExtData(statOut, false, 0);
                

                }
            }



        }

        if (_chipType == "FOOTPEDAL")
        {
            //left down
            if (gateState[swIn0] && gateState[swOut0] != logicOn)
            {
                gateState[swOut0] = logicOn;
                setExtData(swOut0, logicOn, 0);


                if (objectGroup.Length > 0 && objectGroup[0])
                    moveLocalPart(objectGroup[0], -switchSteps, rotType);
                if (isUnique)
                {
                    gateState[swIn1] = !logicOn;
                    gateState[swIn2] = !logicOn;
                }
            }

            //left up
            if (!gateState[swIn0] && gateState[swOut0] == logicOn)
            {

                gateState[swOut0] = false;
                setExtData(swOut0, false, 0);

                if (objectGroup.Length > 0 && objectGroup[0])
                    moveLocalPart(objectGroup[0], switchSteps, rotType);
            }

            //center down
            if (gateState[swIn1] && gateState[swOut1] != logicOn)
            {
                gateState[swOut1] = logicOn;
                setExtData(swOut1, logicOn, 1);


                if (objectGroup.Length > 0 && objectGroup[1])
                    moveLocalPart(objectGroup[1], -switchSteps, rotType);

                if (isUnique)
                {
                    gateState[swIn0] = !logicOn;
                    gateState[swIn2] = !logicOn;
                }

            }

            // center up
            if (!gateState[swIn1] && gateState[swOut1] == logicOn)
            {
                gateState[swOut1] = false;
                setExtData(swOut1, false, 1);


                if (objectGroup.Length > 0 && objectGroup[1])
                    moveLocalPart(objectGroup[1], switchSteps, rotType);

            }

            //right down
            if (gateState[swIn2] && gateState[swOut2] != logicOn)
            {
                gateState[swOut2] = logicOn;
                setExtData(swOut2, logicOn, 2);

                if (objectGroup.Length > 0 && objectGroup[2])
                    moveLocalPart(objectGroup[2], -switchSteps, rotType);
                if (isUnique)
                {
                    gateState[swIn0] = !logicOn;
                    gateState[swIn1] = !logicOn;
                }

            }

            // right up
            if (!gateState[swIn2] && gateState[swOut2] == logicOn)
            {
                gateState[swOut2] = false;
                setExtData(swOut2, false, 2);

                if (objectGroup.Length > 0 && objectGroup[2])
                    moveLocalPart(objectGroup[2], switchSteps, rotType);
            }


        }

        if (_chipType == "NAND")
        {

            if (gateState[primIn0] && gateState[primIn1] && gateState[primOutput] == true)
            {
                gateState[primOutput] = false;
                setExtData(primOutput, false, 0);
            }
            else
            {

                if (gateState[primIn0] != gateState[primIn1] && gateState[primOutput] == false)
                {
                    gateState[primOutput] = true;
                    setExtData(primOutput, true, 0);
                }
            }


        }

        if (_chipType == "AND")
        {
            if (gateState[primIn0] && gateState[primIn1] && gateState[primOutput] != true)
            {
                gateState[primOutput] = true;
                setExtData(primOutput, true, 0);
            }

            if (gateState[primIn0] != gateState[primIn1] && gateState[primOutput] == true)
            {
                gateState[primOutput] = false;
                setExtData(primOutput, false, 0);
             }



        }

        if (_chipType == "TRIAND")
        {
            if (gateState[tPrimIn0] && gateState[tPrimIn1] && gateState[tPrimIn2] && gateState[tPrimOutput] != true)
            {
                gateState[tPrimOutput] = true;
                setExtData(tPrimOutput, true, 0);
            }

            if ((gateState[tPrimIn0] == !logicOn || gateState[tPrimIn1] != logicOn || gateState[tPrimIn2] != logicOn) && gateState[tPrimOutput] == true)
            {
                if (!isLatched)
                {
                    gateState[tPrimOutput] = false;
                    setExtData(tPrimOutput, false, 0);
                }
            }



            if (gateState[tPrimIn0] && gateState[tPrimIn1] && gateState[tPrimIn2] && gateState[tPrimOutput] && isLatch)
                isLatched = true;


            if (!gateState[tPrimIn0] && !gateState[tPrimIn1] && !gateState[tPrimIn2] && isLatched)
            {
                gateState[tPrimOutput] = false;
                setExtData(tPrimOutput, false, 0);
                isLatched = false;
            }


        }

        if (_chipType == "OR" || _chipType == "NOR")
        {
            if ((gateState[primIn0] || gateState[primIn1]) && gateState[primOutput] != logicOn)
            {
                gateState[primOutput] = logicOn;
                setExtData(primOutput, logicOn, 0);
            }

            else
            {
                if (!gateState[primIn0] && !gateState[primIn1] && gateState[primOutput] == logicOn)
                {
                    gateState[primOutput] = !logicOn;
                    setExtData(primOutput, !logicOn, 0);
                }
            }

        }

        if (_chipType == "XOR" || _chipType == "XNOR")
        {
            if (gateState[primIn0] != gateState[primIn1] && gateState[primIn0] == logicOn || gateState[primIn0] != gateState[primIn1] && gateState[primIn1] == logicOn && gateState[primOutput] != logicOn)
            {
                gateState[primOutput] = logicOn;
                setExtData(primOutput, logicOn, 0);
            }

            else
            {
                if (gateState[primIn0] == gateState[primIn1] && gateState[primOutput] == logicOn)
                {
                    gateState[primOutput] = !logicOn;
                    setExtData(primOutput, !logicOn, 0);

                }
            }

        }

        if (_chipType == "BUFFER" || _chipType == "NOT")
        {
            if (isMomentary)
            {
                if (selMaxPos > 0 && isMomentary)
                    if (momDuration > selMaxPos)
                        momDuration = selMaxPos;

                if (selMinPos > 0 && isMomentary)
                    if (momDuration < selMinPos)
                        momDuration = selMinPos;
            }


            if (gateState[gateIn] && gateState[gateOut] != logicOn)
            {

                if (!isCounter)
                {
                    gateState[gateOut] = logicOn;
                    pinState[gateIn] = logicOn;
                    setExtData(gateOut, logicOn, 0);
          
                 //   runGlobalFunctions();

                    if (isLatch)
                        isLatched = true;

                    if (isMomentary)
                        momentaryTrig = true;
                }
                else
                {
                    if (!isCounterTrig && countDownCount > 0)
                        isCounterTrig = true;


                }

                if (isClock)
                    isClockTrig = true;

            }

            if (!gateState[gateIn] && gateState[gateOut] == logicOn)
            {

                if (!isLatched && !isCounter || (isCounter && !momentaryTrig))
                {

                    gateState[gateOut] = !logicOn;
                    pinState[gateIn] = !logicOn;
                    setExtData(gateOut, !logicOn, 0);
                

                }


            }



        }

        if (_chipType == "FLIPPER")
        {
            bool switchPressed = false;

            // switch is pressed
            if (gateState[gateIn] && gateState[gateOut] != logicOn)
            {
                momemtaryTimer = momDuration;
                gateState[gateOut] = logicOn;
                gateOnTrig[gateOut] = true;
                switchPressed = true;

             //   setStatusLed(true);


                setExtData(gateOut, logicOn, 0);

                var hinge = GetComponentInChildren<HingeJoint>();
                var motor = hinge.motor;

                motor.force = sendVal;
                hinge.motor = motor;
                hinge.useSpring = false;

                hinge.useMotor = true;


            }

            else
            //switch is released
            if (!gateState[gateIn] && gateState[gateOut] == logicOn)
            {
                float _prevVal = sendVal;
                sendVal = 0;
                gateState[gateOut] = !logicOn;
                setExtData(gateOut, !logicOn, 0);

           
             //   setStatusLed(false);

                var hinge = GetComponentInChildren<HingeJoint>();
                var motor = hinge.motor;

                motor.force = -switchSteps;
                hinge.motor = motor;
                hinge.useSpring = true;

                hinge.useMotor = false;

                gateOnTrig[gateOut] = false;
                sendVal = _prevVal;

            }



            if (isMomentary)
                momentaryTrig = true;

            switchPressed = false;

        }

        if (_chipType == "LAMP")
        {

            if (gateState[gateIn] && gateState[gateOut] != logicOn && !gateOnTrig[gateIn])
            {
                gateState[gateOut] = logicOn;
                setExtData(gateOut, logicOn, 0);
                gateOnTrig[gateIn] = true;
            
             //   runGlobalFunctions();

                if (isClock)
                    isClockTrig = true;
                if (isMomentary)
                    momentaryTrig = true;

            }

            else
            if (!gateState[gateIn] && gateState[gateOut] == logicOn && gateOnTrig[gateIn])
            {
                gateState[gateOut] = !logicOn;
                setExtData(gateOut, !logicOn, 0);
                gateOnTrig[gateIn] = false;
             

            }


        }

        if (_chipType == "DIAL" || _chipType == "UPDN" || _chipType == "SLIDE" || _chipType == "METER")
        {
            //press button 0   /////////////////////////////////////////////////////
            if (gateState[dirIn0] && gateState[dirOut0] != logicOn)
            {
                if (isButtonHold)
                    isButtonHoldTrig[dirIn0] = true;

                gateState[dirIn0] = logicOn;
                gateState[dirOut0] = logicOn;
                gateState[dirIn1] = !logicOn;

                setExtData(dirDownPin, !logicOn, 0);
                setExtData(dirOut0, logicOn, 0);

                isInReverse = false;
                displayCounterValue++;

                if (_chipType == "UPDN")
                {
                    if (objectGroup.Length > 0 && objectGroup[0] && animType.ToLower() == "rocker")
                    {
                        if (selCurPos == 2)
                            moveLocalPart(objectGroup[0], switchSteps, rotType);

                        if (selCurPos != 1)
                            moveLocalPart(objectGroup[0], switchSteps, rotType);
                    }

                    selCurPos = 1;

                    if (isClock)
                        isClockTrig = true;

                }
                //min max
                if (_chipType == "DIAL" || _chipType == "METER")
                {
                    if (objectGroup.Length > 0 && objectGroup[0])
                    {
                        if (selMaxPos > 0)
                        {
                            if (selCurPos < selMaxPos)
                            {
                                selCurPos = selCurPos + switchSteps;

                                if (_chipType == "DIAL")
                                {

                                    if (selCurPos > selMaxPos)
                                        selCurPos = selMaxPos;
                                    else
                                        moveLocalPart(objectGroup[0], -switchSteps, rotType);
                                }


                                if (_chipType == "METER")
                                {


                                    if (selCurPos > selMaxPos)
                                        selCurPos = selMaxPos;
                                    else
                                        moveLocalPart(objectGroup[0], switchSteps, rotType);
                                }
                                setExtData(dirOut0, logicOn, 0);
                            }
                        }
                        else
                        {
                            selCurPos = selCurPos + switchSteps;

                            if (_chipType == "DIAL")
                            {
                                if (selCurPos > 360)
                                    selCurPos = 0;
                                moveLocalPart(objectGroup[0], -switchSteps, rotType);
                            }
                            if (_chipType == "METER")
                            {

                                if (selCurPos > 180)
                                    selCurPos = 180;
                                moveLocalPart(objectGroup[0], switchSteps, rotType);
                            }
                            setExtData(dirOut0, logicOn, 0);
                        }


                    }


                }

                if (_chipType == "SLIDE")
                {
                    if (objectGroup.Length > 0 && objectGroup[0] && animType.ToLower() == "slide")
                    {
                        if (selCurPos < selMaxPos)
                        {
                            selCurPos++;
                            moveLocalPart(objectGroup[0], switchSteps, rotType);
                            setExtData(dirOut0, logicOn, 0);
                        }
                    }

                }


                if (isMomentary)
                    momentaryTrig = true;
            }
            else
            //  release button 0 **
            if (!gateState[dirIn0] && gateState[dirOut0] == logicOn)
            {

                gateState[dirIn0] = !logicOn;
                gateState[dirOut0] = !logicOn;
                setExtData(dirOut0, !logicOn, 0);
          
                if (isButtonHold)
                    isReleaseButtonHold[dirIn0] = false;

                if (_chipType == "UPDN")
                {

                    if (objectGroup.Length > 0 && objectGroup[0] && animType.ToLower() == "rocker")
                    {
                        if (selCurPos != 0)
                            moveLocalPart(objectGroup[0], -switchSteps, rotType);
                    }

                    setExtData(dirOut0, !logicOn, 0);

                    selCurPos = 0;
                }


            }

            //press button 1  /////////////////////////////////////////////////////
            if (gateState[dirIn1] && gateState[dirOut1] != logicOn)
            {
                if (isButtonHold)
                    isButtonHoldTrig[dirIn1] = true;

                gateState[dirIn1] = logicOn;
                gateState[dirOut1] = logicOn;
                gateState[dirIn0] = !logicOn;

                setExtData(dirDownPin, logicOn, 0);
                setExtData(dirOut1, logicOn, 0);
             

                isInReverse = true;
                displayCounterValue--;

                if (_chipType == "UPDN")
                {

                    if (objectGroup.Length > 0 && objectGroup[0] && animType.ToLower() == "rocker")
                    {
                        if (selCurPos == 1)
                            moveLocalPart(objectGroup[0], -switchSteps, rotType);

                        if (selCurPos != 2)
                            moveLocalPart(objectGroup[0], -switchSteps, rotType);

                    }

                    selCurPos = 2;
                   
                    if (isClock)
                        isClockTrig = false;


                }

                if (_chipType == "DIAL" || _chipType == "METER")
                {
                    if (objectGroup.Length > 0 && objectGroup[0])
                    {

                        if (selMinPos > 0)
                        {
                            if (selCurPos > selMinPos)
                            {
                                selCurPos = selCurPos - switchSteps;
                                if (selCurPos < selMinPos)
                                    selCurPos = selMinPos;

                                if (_chipType == "DIAL")
                                {
                                    if (selCurPos < 1)
                                        selCurPos = 360;
                                    else
                                        moveLocalPart(objectGroup[0], switchSteps, rotType);
                                }
                                if (_chipType == "METER")
                                {
                                    if (selCurPos < 0)
                                        selCurPos = 0;
                                    else
                                        moveLocalPart(objectGroup[0], -switchSteps, rotType);
                                }

                              //  setExtData(dirOut1, logicOn, 1);
                            }
                        }
                        else
                        {

                            selCurPos = selCurPos - switchSteps;

                            if (_chipType == "DIAL")
                            {
                                if (selCurPos < 0)
                                    selCurPos = 360;
                                moveLocalPart(objectGroup[0], switchSteps, rotType);
                            }


                            if (_chipType == "METER")
                            {
                                if (selCurPos < 0)
                                    selCurPos = 0;
                                else
                                    moveLocalPart(objectGroup[0], -switchSteps, rotType);
                            }

                          //  setExtData(dirOut1, logicOn, 1);

                        }

                    }


                }


                if (_chipType == "SLIDE")
                {
                    if (objectGroup.Length > 0 && objectGroup[0] && animType.ToLower() == "slide")
                    {
                        if (selCurPos > 0)
                        {
                            selCurPos--;
                            moveLocalPart(objectGroup[0], -switchSteps, rotType);
                           // setExtData(dirOut1, logicOn, 0);
                        }
                    }

                }

                if (isMomentary)
                    momentaryTrig = true;

            }
            else
            //release button1 **
            if (!gateState[dirIn1] && gateState[dirOut1] == logicOn)
            {
                gateState[dirIn1] = !logicOn;
                gateState[dirOut1] = !logicOn;
                setExtData(dirOut1, !logicOn, 0);
          
                if (isButtonHold)
                    isReleaseButtonHold[dirIn1] = false;

                if (_chipType == "UPDN")
                {
                    if (objectGroup.Length > 0 && objectGroup[0] && animType.ToLower() == "rocker")
                        if (selCurPos != 0)
                            moveLocalPart(objectGroup[0], switchSteps, rotType);

                    setExtData(dirOut1, !logicOn, 0);
                    selCurPos = 0;
                }

            }



        }

        if (_chipType == "FAN" || _chipType == "MOTOR")
        {


            //press button 0  forward
            if (gateState[dirIn0] && gateState[dirOut0] != logicOn)
            {

                gateState[dirOut0] = logicOn;
                gateState[dirOut1] = !logicOn;
                pinState[dirOut1] = !logicOn;
                gateState[dirIn1] = !logicOn;
                pinState[dirIn1] = !logicOn;

                setExtData(dirOut0, logicOn, 0);

          
                if (GetComponent<UniLogicEffects>())
                    GetComponent<UniLogicEffects>().changePitch(0);

                isInReverse = false;
                isWindDown = false;
                windDownTimer = 0;

                switchSteps = motorTimerMax;
                motorTimer = switchSteps;



            }
            else
            //  toggle but 0 **
            if (!gateState[dirIn0] && gateState[dirOut0] == logicOn)
            {
                gateState[dirOut0] = !logicOn;
                pinState[dirOut0] = !logicOn;
                setExtData(dirOut0, !logicOn, 0);

                windDownTimer = 0;
                isWindDown = true;


            }

            //press button 1  reverse  /////////////////////////////////////////////////////
            if (gateState[dirIn1] && gateState[dirOut1] != logicOn)
            {

                gateState[dirOut1] = logicOn;

                gateState[dirOut0] = !logicOn;
                pinState[dirOut0] = !logicOn;
                gateState[dirIn0] = !logicOn;
                pinState[dirIn0] = !logicOn;
         
                if (GetComponent<UniLogicEffects>())
                    GetComponent<UniLogicEffects>().changePitch(0);

                switchSteps = motorTimerMax;
                motorTimer = switchSteps;

                isWindDown = false;
                windDownTimer = 0;
                isInReverse = true;

            }

            // tog button1 **
            if (!gateState[dirIn1] && gateState[dirOut1] == logicOn)
            {

                gateState[dirOut1] = !logicOn;
                pinState[dirOut1] = !logicOn;
                setExtData(dirOut1, !logicOn, 0);

                windDownTimer = 0;
                isWindDown = true;



            }

            if (gateState[speedUp] && switchSteps < maxSwitchSteps)
            {

             

                gateState[speedUp] = false;
                if (clockPrecision == 0)
                    clockPrecision = 1;


                switchSteps = switchSteps + clockPrecision;
                //   windDownTimerMax++;

                if (switchSteps > motorTimerMax * 2)
                    switchSteps = motorTimerMax;

                if (GetComponent<UniLogicEffects>())
                    GetComponent<UniLogicEffects>().changePitch(+pitchShift);


            }

            if (gateState[speedDn] && switchSteps>minSwitchSteps)
            {
                if (clockPrecision == 0)
                    clockPrecision = 1;

                gateState[speedDn] = false;
                switchSteps = switchSteps - clockPrecision;
                //  windDownTimerMax--;

                if (switchSteps < 1)
                    switchSteps = 1;

                if (GetComponent<UniLogicEffects>())
                    GetComponent<UniLogicEffects>().changePitch(-pitchShift);

            }

        }

        if (_chipType == "TRISWITCH")
        {
            //flip up
            if (gateState[swIn0] && gateState[swOut0] != logicOn)
            {
                gateState[swOut0] = logicOn;
                setExtData(swOut0, logicOn, 0);
            
             //   runGlobalFunctions();


                if (objectGroup.Length > 0 && objectGroup[0] && objectGroup[0] != null)
                    moveLocalPart(objectGroup[0], -switchSteps, rotType);

                gateState[swIn1] = !logicOn;
                gateState[swIn2] = !logicOn;


            }

            //flip up off
            if (!gateState[swIn0] && gateState[swOut0] == logicOn)
            {

                gateState[swOut0] = !logicOn;
                setExtData(swOut0, !logicOn, 0);
           
                if (objectGroup.Length > 0 && objectGroup[0] && objectGroup[0] != null)
                    moveLocalPart(objectGroup[0], switchSteps, rotType);
            }

            //center
            if (gateState[swIn1] && gateState[swOut1] != logicOn)
            {
                gateState[swOut1] = logicOn;
                setExtData(swOut1, logicOn, 0);

                gateState[swIn0] = !logicOn;
                gateState[swIn2] = !logicOn;
            //    runGlobalFunctions();


            }

            // center off
            if (!gateState[swIn1] && gateState[swOut1] == logicOn)
            {
                gateState[swOut1] = !logicOn;
                gateState[swOut1] = !logicOn;
                setExtData(swOut1, !logicOn, 0);

            }

            //down on
            if (gateState[swIn2] && gateState[swOut2] != logicOn)
            {
                gateState[swOut2] = logicOn;
                setExtData(swOut2, logicOn, 1);
           
            //    runGlobalFunctions();

                if (objectGroup.Length > 0 && objectGroup[0] && objectGroup[0] != null)
                    moveLocalPart(objectGroup[0], switchSteps, rotType);

                gateState[swIn0] = !logicOn;
                gateState[swIn1] = !logicOn;



            }

            // down off
            if (!gateState[swIn2] && gateState[swOut2] == logicOn)
            {
                gateState[swOut2] = !logicOn;
                setExtData(swOut2, !logicOn, 1);
           
                if (objectGroup.Length > 0 && objectGroup[0] && objectGroup[0] != null)
                    moveLocalPart(objectGroup[0], -switchSteps, rotType);
            }






        }

        if (_chipType == "THUMBWHEEL" || _chipType == "SCOREWHEEL")
        {

            // stop data dump
            if (isClock && isClockTrig && maxClockRepeats < 1)
            {
                pinState[twSend] = false;
                gateState[twSend] = false;


                if (outputLinkObj.Length > 0)
                    setExtData(0, false, 0);

                if (outputLinkObj.Length > 1)
                    setExtData(1, false, 0);

                if (outputLinkObj.Length > 2)
                    setExtData(2, false, 0);

                isClockTrig = false;
                isClock = false;
                maxClockRepeats = 0;
                displayCounterValue = 0;
                objectGroupValue = 0;
                dumpCount = 0;
                isDumpData = false;


            }


            //1st wheel click up
            if (gateState[twIn1] && gateOnTrig[twIn1] != logicOn)
            {
                gateOnTrig[twIn1] = logicOn;
           
                digitPosValue[0]++;
                if (digitPosValue[0] > 9)
                {
                    digitPosValue[0] = 0;
                    if (_chipType == "SCOREWHEEL")
                        gateState[twIn2] = true;

                }


                if (objectGroup.Length > 0 && objectGroup[0])
                    moveLocalPart(objectGroup[0], 360f / selMaxPos, rotType);

                setExtData(0, false, 2);
                setExtData(0, true, 1);


            }

            //1st wheel up off
            if (!gateState[twIn1] && gateOnTrig[twIn1] == logicOn)
            {
                gateOnTrig[twIn1] = false;
                setExtData(0, false, 1);

            }

            //1st wheel 0 click down
            if (gateState[twInDn1] && gateOnTrig[twInDn1] != logicOn)
            {
                gateOnTrig[twInDn1] = logicOn;
           
                digitPosValue[0]--;
                if (digitPosValue[0] < 0)
                    digitPosValue[0] = 9;


                if (objectGroup.Length > 0 && objectGroup[0])
                    moveLocalPart(objectGroup[0], -(360f / selMaxPos), rotType);

                if (!isClockTrig)
                {
                    setExtData(0, true, 2);
                    setExtData(0, true, 1);

                }

            }

            // 1st wheel down off
            if (!gateState[twInDn1] && gateOnTrig[twInDn1] == logicOn)
            {
                gateOnTrig[twInDn1] = false;
                setExtData(0, false, 1);

            }

            //2nd wheel click up
            if (gateState[twIn2] && gateOnTrig[twIn2] != logicOn)
            {
                gateOnTrig[twIn2] = logicOn;
          
                digitPosValue[1]++;
                if (digitPosValue[1] > 9)
                {
                    digitPosValue[1] = 0;
                    if (_chipType == "SCOREWHEEL")
                        gateState[twIn3] = true;

                }

                if (objectGroup.Length > 1 && objectGroup[1])
                    moveLocalPart(objectGroup[1], 360f / selMaxPos, rotType);


                setExtData(1, false, 2);
                setExtData(1, true, 1);


            }

            //2nd wheel up off
            if (!gateState[twIn2] && gateOnTrig[twIn2] == logicOn)
            {
                gateOnTrig[twIn2] = false;
                setExtData(1, false, 1);

            }

            //2nd wheel cklck down
            if (gateState[twInDn2] && gateOnTrig[twInDn2] != logicOn)
            {
                gateOnTrig[twInDn2] = true;
            
                digitPosValue[1]--;
                if (digitPosValue[1] < 0)
                    digitPosValue[1] = 9;

                if (objectGroup.Length > 1 && objectGroup[1])
                    moveLocalPart(objectGroup[1], -(360f / selMaxPos), rotType);

                if (!isClockTrig)
                {

                    setExtData(1, true, 2);
                    setExtData(1, true, 1);

                }
            }

            //2nd wheel down off
            if (!gateState[twInDn2] && gateOnTrig[twInDn2] == logicOn)
            {
                gateOnTrig[twInDn2] = false;
                setExtData(1, false, 1);

            }

            //3nd wheel click up
            if (gateState[twIn3] && gateOnTrig[twIn3] != logicOn)
            {
                gateOnTrig[twIn3] = logicOn;
           
                digitPosValue[2]++;
                if (digitPosValue[2] > 9)
                {
                    digitPosValue[2] = 0;
                    gateOnTrig[twSend] = true;
                }


                if (objectGroup.Length > 2 && objectGroup[2])
                    moveLocalPart(objectGroup[2], 360f / selMaxPos, rotType);

                setExtData(2, false, 2);
                setExtData(2, true, 1);

            }

            //3nd wheel up off
            if (!gateState[twIn3] && gateOnTrig[twIn3] == logicOn)
            {
                gateOnTrig[twIn3] = false;
                setExtData(2, false, 1);
            }

            //3nd wheel click down
            if (gateState[twInDn3] && gateOnTrig[twInDn3] != logicOn)
            {
                gateOnTrig[twInDn3] = true;
           
                digitPosValue[2]--;
                if (digitPosValue[2] < 0)
                    digitPosValue[2] = 9;


                if (objectGroup.Length > 1 && objectGroup[1])
                    moveLocalPart(objectGroup[2], -(360f / selMaxPos), rotType);


                if (!isClockTrig)
                {

                    setExtData(2, true, 2);
                    setExtData(2, true, 1);
                }

            }

            //3nd wheel down off
            if (!gateState[twInDn3] && gateOnTrig[twInDn3] == logicOn)
            {
                gateOnTrig[twInDn3] = false;
                setExtData(2, false, 1);

            }


            displayCounterValue = digitPosValue[0] * 1 + digitPosValue[1] * 10 + digitPosValue[2] * 100;
            objectGroupValue = displayCounterValue;
            maxClockRepeats = (int)objectGroupValue;

            //dump data
            if ((gateState[twSend] || pinState[twSend]) && !isClockTrig)
            {
                pinState[twSend] = true;
                gateState[twSend] = true;
                isClock = true;
                isClockTrig = true;

            }


            if (gateState[twReset])
                chipReset(false);


        }

        if (_chipType == "RELAY")
        {

            if (gateState[relayCoilPin] && !gateOnTrig[relayCoilPin])
            {
                gateState[relayCoilPin] = true;
                pinState[relayCoilPin] = true;
                gateOnTrig[relayCoilPin] = true;

                gateState[relayNOPin] = relayCommonLevel;
                pinState[relayNOPin] = relayCommonLevel;

                gateState[relayNCPin] = !relayCommonLevel;
               pinState[relayNCPin] = !relayCommonLevel;

                 runEffectsClickOnTrig = true;

                if (objectGroup.Length > 0 && objectGroup[0])
                    moveLocalPart(objectGroup[0], 360 / (256 - switchSteps), rotType);

                setExtData(relayNOPin, !logicOn, 0);
                setExtData(relayNCPin, logicOn, 0);
           


            }

            if (!gateState[relayCoilPin] && gateOnTrig[relayCoilPin])
            {

      
                gateState[relayCoilPin] = false;
                pinState[relayCoilPin] = false;

                gateOnTrig[relayCoilPin] = false;

                gateState[relayNOPin] = !relayCommonLevel;
               pinState[relayNOPin] = !relayCommonLevel;

                gateState[relayNCPin] = relayCommonLevel;
               pinState[relayNCPin] = relayCommonLevel;

                runEffectsClickOffTrig = true;


                if (objectGroup.Length > 0 && objectGroup[0])
                    moveLocalPart(objectGroup[0], -(360 / (256 - switchSteps)), rotType);

                setExtData(relayNOPin, logicOn, 0);
               setExtData(relayNCPin, !logicOn, 0);


            }



        }

        if (_chipType == "DFF")
        {

            if (clockPosTrig)
            {
                if (gateState[outputPin] && !gateOnTrig[outputPin])
                {
                    setExtData(outputPin, true, 0);
                    gateOnTrig[outputPin] = true;
                }
                else
                {
                    if (!gateState[outputPin] && gateOnTrig[outputPin])
                    {
                        setExtData(outputPin, false, 0);
                        gateOnTrig[outputPin] = false;
                    }
                }
                clockPosTrig = false;
            }


            // reset chip if reset pin goes high
            if (pinState[resetPin] == true)
            {
                gateState[resetPin] = true;

                if (isMaster)
                    signalFanout(resetPin);

                pinReset();
                paramInit();

            }


            if (gateState[clockPin] == true)
            {
                clockPosTrig = true;

                if (isMaster)
                    signalFanout(clockPin);
                gateState[clockPin] = false;


                gateState[outputPin] = gateState[dataPin];
                gateState[dataPin] = false;


                gateState[outputQPin] = !gateState[outputPin];
                if (gateState[presetPin])
                    gateState[dataPin] = gateState[outputQPin];

            }

        }

        if (_chipType == "DIGDISPLAYxx")
        {
            for (int i = 0; i < pinState.Length; i++)
            {
                if (pinState[i])
                    gateState[i] = true;

                if (!pinState[i])
                    gateState[i] = false;

            }

            if (gateState[countPin])
            {
                displayCounterValue++;

                if (displayCounterValue >= displayLedObj.Length)
                {
                    setExtData(countPin, true, 0);
                    displayCounterValue = 0;

                }

                setDisplayDigit(displayCounterValue);

                gateState[countPin] = false;
                pinState[countPin] = false;
                //  gateState[carryOutPin] = false;

            }

            if (gateState[resetPin])
            {
                pinReset();
                pinState[resetPin] = false;
                gateState[resetPin] = false;
            }

        }

        if (_chipType == "DEKATRON" || _chipType == "UNDEKATRON" || _chipType == "NIXI" || _chipType == "DIGDISPLAY")
        {

            //reset carry pin if set from end of cycle
            if (gateState[resetCounterPin] || pinState[resetCounterPin])
            {

                isDumpData = false;
                displayCounterValue = 0;
                isDisableLeds = false;
                objectGroupValue = 0;
                isClockTrig = false;

                for (int i = 0; i < pinState.Length; i++)
                    pinState[i] = false;

                for (int i = 0; i < gateState.Length; i++)
                    gateState[i] = false;


                isMomentary = true;


                if (_chipType == "DIGDISPLAY")
                    setDisplayDigit(0);

                if (_chipType == "NIXI")
                {
                    setNixiDigit(0);
                }

                if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> reset display");

                return;

            }


            if (gateState[carryOutPin])
            {
                setStatusLed(true);
                gateState[carryOutPin] = false;
                pinState[carryOutPin] = false;
                gateState[dirDownPin] = false;
                pinState[dirDownPin] = false;


            }


            if (gateState[countPin])
            {
                gateState[countPin] = true;
                pinState[countPin] = true;

                if (isClock)
                    isClockTrig = true;

                if (displayCounterValue % 2 == 0)
                {
                    for (int ii = 0; ii < statusLedObj.Length; ii++)
                        if (statusLedObj[ii])
                            setStatusLed(true);
                }
                else
               {
                    for (int ii = 0; ii < statusLedObj.Length; ii++)
                        if (statusLedObj[ii])
                            setStatusLed(false);
                }



                if (!gateState[dirDownPin] && !isDumpData)
                {
                    isInReverse = false;


                    if (objectGroup.Length > 0 && objectGroup[0] && objectGroup[0].activeSelf)
                        moveLocalPart(objectGroup[0], 360f / displayLedObj.Length, rotType);

                    if (isMaster)
                    {
                        setExtData(dirDownPin, false, 0);
                        objectGroupValue++;

                    }

                    if (displayCounterValue < displayLedObj.Length)
                        displayCounterValue++;


                    //counter reaches max and toggles carry out
                    if (displayCounterValue >= displayLedObj.Length)
                    {

                        displayCounterValue = 0;

                        gateState[carryOutPin] = true;
                        setExtData(countPin, true, 0);
                    }


                    driverState[displayLedObj.Length - 1] = false;


                }

                // if reverse direction pin is clicked and counter >0
                if (gateState[dirDownPin])
                {
                    isInReverse = true;

                    if (objectGroup.Length > 0 && objectGroup[0] && objectGroup[0].activeSelf)
                        moveLocalPart(objectGroup[0], -360f / displayLedObj.Length, rotType);

                    if (isMaster)
                    {

                        if (objectGroupValue > 0)
                        {

                            objectGroupValue--;
                            maxClockRepeats = (int)objectGroupValue;

                            if (isDumpData)
                            {
                                pinState[dataStatePin] = true;
                                setExtData(dataStatePin, true, 1);
                            }

                            //count returs to zero
                            if (objectGroupValue == 0 && isDumpData == true)
                            {
                                gateState[dirDownPin] = false;
                                pinState[dirDownPin] = false;

                                isClock = false;
                                isClockTrig = false;
                                gateState[dSend] = false;

                                objectGroupValue = 0;
                                maxClockRepeats = 0;

                                setChainDir(false);

                                isDumpData = false;
                                pinState[dataStatePin] = false;

                            }
                        }

                    }

                    driverState[displayCounterValue] = false;

                    if (displayCounterValue >= 0)
                        displayCounterValue--;


                    if (displayCounterValue < 0)
                    {


                        displayCounterValue = displayLedObj.Length;
                        setExtData(countPin, true, 0);
                        displayCounterValue = displayLedObj.Length - 1;

                        gateState[carryOutPin] = true;
                        setExtData(dataStatePin, false, 2);


                    }

                    if (chipType.ToUpper() == "DEKATRON" || chipType.ToUpper() == "UNDEKATRON")
                         setStatusLed(false);
                }



                //turn on current number/led
                if (chipType.ToUpper() != "DIGDISPLAY")
                {
                    driverState[displayCounterValue] = true;
                    //remove previous number from display
                    if (displayCounterValue > 0 && displayCounterValue < displayLedObj.Length)
                    {
                        driverState[displayCounterValue - 1] = false;
                        displayLedObj[0].GetComponent<Renderer>().material.color = originalDispColor[0];
                        setEmission(displayLedObj[0], false, originalDispColor[0]);

                    }

                    if (displayCounterValue == 0)
                        driverState[9] = false;

                }
                else
                {
                    setDisplayDigit(displayCounterValue);
                }


                //reset values after processing direction
               //  setStatusLed(1, false);
                pinState[countPin] = false;
                gateState[countPin] = false;
               // gateState[dirDownPin] = false;
              //  pinState[dirDownPin] = false;

            }


            if (gateState[dSend] && !isDumpData)
            {


                //   Debug.Log("dumpdata pin");

            }

            //dump data
            if (chipType.ToUpper() != "DIGDISPLAY" && isMaster && (gateState[dSend] || gateState[dSend]) && !isClockTrig && !isDumpData && objectGroupValue > 0)
            {

                maxClockRepeats = (int)objectGroupValue;

                isClock = true;
                isClockTrig = true;
                isDumpData = true;

                setChainDir(true);

                pinState[dirDownPin] = true;

                pinState[dSend] = false;
                gateState[dSend] = false;

                // Debug.Log("dumpdata");

            }




            for (int i = 0; i < pinState.Length; i++)
            {
                if (pinState[i])
                    gateState[i] = true;

                if (!pinState[i])
                    gateState[i] = false;

            }

            if (isInReverse)
            {
                if (_chipType == "DIGDISPLAY")
                    setStatusLed(true);
            }
            else
            {
                if (_chipType == "DIGDISPLAY")
                    setStatusLed(false);
            }


        }

        if (_chipType == "SWITCH")
        {

            // switch is pressed
            if (gateState[gateIn] && gateState[gateOut] != logicOn)
            {
                if (!isCounter)
                {
                    gateState[gateOut] = logicOn;
                    pinState[gateOut] = logicOn;

                    gateOnTrig[gateOut] = true;

                    setExtData(gateOut, !logicOn, 2);
                    setExtData(gateOut, logicOn, 0);

           
                    if (objectGroup.Length > 0 && objectGroup[0])
                    {

                        if (animType.ToLower() == "rotate")
                            moveLocalPart(objectGroup[0], -switchSteps, rotType);

                        if (animType.ToLower() == "breaker")
                            moveLocalPart(objectGroup[0], -switchSteps, rotType);

                        if (animType.ToLower() == "rocker")
                            moveLocalPart(objectGroup[0], -switchSteps, rotType);

                        if (animType.ToLower() == "rocker2")
                            moveLocalPart(objectGroup[0], switchSteps, rotType);

                        if (animType.ToLower() == "pusht2")
                            objectGroup[0].transform.Translate(Vector3.right * switchSteps);

                        if (animType.ToLower() == "pusht1")
                            objectGroup[0].transform.Translate(Vector3.forward * switchSteps);

                        if (animType.ToLower() == "pusht3")
                            objectGroup[0].transform.Translate(Vector3.up * switchSteps);

                        if (animType.ToLower() == "pusht4")
                            objectGroup[0].transform.Translate(Vector3.down * switchSteps);


                        if (animType.ToLower() == "push" || animType.ToLower() == "pushL")
                            objectGroup[0].transform.Translate(Vector3.back * switchSteps);


                    }

                    if (isUnique && circuitGroup != "")
                    {
                        foreach (var toneObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
                        {

                            if (toneObj.GetComponentInParent<UniLogicChip>().sendVal != sendVal && toneObj.GetComponentInParent<UniLogicChip>().circuitGroup == circuitGroup)
                            {
                                if (toneObj.GetComponentInParent<UniLogicChip>().gateState[gateOut])
                                {
                                    if (toneObj.GetComponentInParent<UniLogicChip>().animType.ToLower() == "push")
                                        toneObj.GetComponentInParent<UniLogicChip>().objectGroup[0].transform.Translate(Vector3.forward * toneObj.GetComponentInParent<UniLogicChip>().switchSteps);

                                    toneObj.GetComponentInParent<UniLogicChip>().pinState[gateIn] = false;
                                    toneObj.GetComponentInParent<UniLogicChip>().gateState[gateIn] = false;
                                    toneObj.GetComponentInParent<UniLogicChip>().pinState[gateOut] = false;
                                    toneObj.GetComponentInParent<UniLogicChip>().gateState[gateOut] = false;
                                    toneObj.GetComponentInParent<UniLogicChip>().gateOnTrig[gateOut] = false;

                                }

                            }
                        }
                    }


                    if (isClock && !isClockTrig && isMaster)
                    {
                        if (objectGroupValue > 0)
                            maxClockRepeats = (int)objectGroupValue;

                        if (countDownCount > 0)
                            isClockTrig = true;
                    }

                    if (isLatch)
                        isLatched = true;

                    if (isMomentary)
                        momentaryTrig = true;
                }
                else
                {
                    if (!isCounterTrig)
                        isCounterTrig = true;
                }



                if (isOneshot)
                    isButtonBlocked = true;
            }


            //switch is released
            if (!gateState[gateIn] && gateState[gateOut] == logicOn && !isLatched)
            {
                float _prevVal = sendVal;
                sendVal = 0;
                counterClock = 0;
                isButtonBlocked = false;
                gateState[gateOut] = !logicOn;
                pinState[gateOut] = !logicOn;

                setExtData(gateOut, !logicOn, 0);
                setExtData(gateOut, logicOn, 1);
             //   outputState = false;

                if (objectGroup.Length > 0 && objectGroup[0] && gateOnTrig[gateOut])
                {

                    if (animType.ToLower() == "rotate")
                        moveLocalPart(objectGroup[0], switchSteps, rotType);
                    if (animType.ToLower() == "breaker")
                        moveLocalPart(objectGroup[0], switchSteps, rotType);

                    if (animType.ToLower() == "rocker")
                        moveLocalPart(objectGroup[0], switchSteps, rotType);

                    if (animType.ToLower() == "rocker2")
                        moveLocalPart(objectGroup[0], -switchSteps, rotType);

                    if (animType.ToLower() == "pusht2")
                        objectGroup[0].transform.Translate(Vector3.left * switchSteps);

                    if (animType.ToLower() == "pusht1")
                        objectGroup[0].transform.Translate(Vector3.back * switchSteps);

                    if (animType.ToLower() == "pusht3")
                        objectGroup[0].transform.Translate(Vector3.down * switchSteps);

                    if (animType.ToLower() == "pusht4")
                        objectGroup[0].transform.Translate(Vector3.up * switchSteps);

                    if (animType.ToLower() == "push" || animType.ToLower() == "pushL")
                        objectGroup[0].transform.Translate(Vector3.forward * switchSteps);


                }

                sendVal = _prevVal;

            }


        }

        if (_chipType == "SEQUENCER")
        {

            // switch is pressed
            if (gateState[gateIn] && gateState[gateOut] != logicOn)
            {

                gateState[gateOut] = logicOn;
                gateOnTrig[gateOut] = true;

                setExtData(gateOut, !logicOn, 2);
                setExtData(gateOut, logicOn, 0);


                if (objectGroup.Length > 0 && objectGroup[0])
                {

                    if (animType.ToLower() == "rotate")
                        moveLocalPart(objectGroup[0], -switchSteps, rotType);

                    if (animType.ToLower() == "breaker")
                        moveLocalPart(objectGroup[0], -switchSteps, rotType);

                    if (animType.ToLower() == "rocker")
                        moveLocalPart(objectGroup[0], -switchSteps, rotType);

                    if (animType.ToLower() == "rocker2")
                        moveLocalPart(objectGroup[0], switchSteps, rotType);

                    if (animType.ToLower() == "pusht2")
                        objectGroup[0].transform.Translate(Vector3.right * switchSteps);

                    if (animType.ToLower() == "pusht1")
                        objectGroup[0].transform.Translate(Vector3.forward * switchSteps);

                    if (animType.ToLower() == "pusht3")
                        objectGroup[0].transform.Translate(Vector3.up * switchSteps);

                    if (animType.ToLower() == "pusht4")
                        objectGroup[0].transform.Translate(Vector3.down * switchSteps);

                    if (animType.ToLower() == "push" || animType.ToLower() == "pushL")
                        objectGroup[0].transform.Translate(Vector3.back * switchSteps);


                }


                if (isClock && !isClockTrig && isMaster)
                {
                    if (objectGroupValue > 0)
                        maxClockRepeats = (int)objectGroupValue;

                    if (countDownCount > 0)
                        isClockTrig = true;
                }


            }

            else
            //switch is released
            if (!gateState[gateIn] && gateState[gateOut] == logicOn && !isLatched)
            {
                float _prevVal = sendVal;
                sendVal = 0;

                gateState[gateOut] = !logicOn;
                setExtData(gateOut, !logicOn, 0);

                if (output2LinkObj.Length > 0)
                    setExtData(gateOut, logicOn, 1);

                if (objectGroup.Length > 0 && objectGroup[0] && gateOnTrig[gateOut])
                {

                    if (animType.ToLower() == "rotate")
                        moveLocalPart(objectGroup[0], switchSteps, rotType);
                    if (animType.ToLower() == "breaker")
                        moveLocalPart(objectGroup[0], switchSteps, rotType);

                    if (animType.ToLower() == "rocker")
                        moveLocalPart(objectGroup[0], switchSteps, rotType);

                    if (animType.ToLower() == "rocker2")
                        moveLocalPart(objectGroup[0], -switchSteps, rotType);

                    if (animType.ToLower() == "pusht2")
                        objectGroup[0].transform.Translate(Vector3.left * switchSteps);

                    if (animType.ToLower() == "pusht1")
                        objectGroup[0].transform.Translate(Vector3.back * switchSteps);

                    if (animType.ToLower() == "pusht3")
                        objectGroup[0].transform.Translate(Vector3.down * switchSteps);

                    if (animType.ToLower() == "pusht4")
                        objectGroup[0].transform.Translate(Vector3.up * switchSteps);

                    if (animType.ToLower() == "push" || animType.ToLower() == "pushL")
                        objectGroup[0].transform.Translate(Vector3.forward * switchSteps);

                }

                sendVal = _prevVal;

            }


        }

        if (_chipType == "IF")
        {
            if (gateState[primIn1] && outputLinkObj.Length > 0)
            {

                for (int i = 0; i < outputLinkObj.Length; i++)
                {
                    if (outputLinkObj[i] && outputLinkObj[i].GetComponent<UniLogicChip>().outputState == !logicOn)
                    {
                        gateState[primOutput] = logicOn;
                        setExtData(primOutput, logicOn, 0);
     
                    }

                }

                gateState[primIn1] = false;

            }

            if (gateState[primIn0] && outputLinkObj.Length > 0)
            {

                for (int i = 0; i < outputLinkObj.Length; i++)
                {
                    if (outputLinkObj[i] && outputLinkObj[i].GetComponent<UniLogicChip>().outputState == logicOn)
                    {
                        gateState[primOutput] = !logicOn;
                        setExtData(primOutput, !logicOn, 0);
                    }

                }

                gateState[primIn0] = false;

            }

        }

   
         alignPins();
     

    }

    void updateLogicTimers() // main timers
    {
        // chip is a timer
        if (isClock && isClockTrig)
        {

            clockTimer --;
            if (clockTimer <= 0f)
            {
                if (isSerialDump)
                {
                    dumpCount++;
                    // Debug.Log("DumpCount = " + dumpCount);
                }


                if (chipType.ToUpper() == "LAMP" || chipType.ToUpper() == "SWITCH" || chipType.ToUpper() == "FLIPPER" || chipType.ToUpper() == "DISPLAY" || chipType.ToUpper() == "BUFFER" ||
                    chipType.ToUpper() == "NOT" || chipType.ToUpper() == "DEKATRON" || chipType.ToUpper() == "UNDEKATRON" || chipType.ToUpper() == "NIXI" || chipType.ToUpper() == "UPDN" ||
                     chipType.ToUpper() == "TBUTTON")
                {
                    if (gateState[gateOut] == logicOn)
                    {
                        gateState[gateIn] = false;
                        pinState[gateIn] = false;
                        runEffectsClickOffTrig = true;
                    }
                    else
                    {
                        if (gateState[gateOut] == !logicOn)
                        {
                            gateState[gateIn] = true;
                            pinState[gateIn] = true;
                            runEffectsClickOnTrig = true;
                           
                        }
                    }

                }



                if (maxClockRepeats > 0 && isClockTrig && isClock)
                {
                    countDownCount--;
                    //       Debug.Log("countDownCount = " + countDownCount + " maxClockRepeats = " + maxClockRepeats);


                    if (countDownCount <= 0)
                    {
                        countDownCount = 0;
                        countDownCount = maxClockRepeats;
                        isClockTrig = false;

                        if (chipClass.ToLower() != "thumbwheel" && chipClass.ToLower() != "scorewheel")
                        {
                            gateState[gateIn] = false;
                            pinState[gateIn] = false;
                            runEffectsClickOffTrig = true;

                        }

                    }


                    if (chipClass.ToLower() == "thumbwheel" || chipClass.ToLower() == "scorewheel")
                    {


                        if (digitPosValue[0] > 0)
                        {

                            if (gateState[twInDn1] == logicOn)
                            {
                                gateState[twInDn1] = false;
                                runEffectsClickOffTrig = true;
                                if (outputLinkObj.Length > 0)
                                    setExtData(0, false, 0);

                            }
                            else
                            {
                                if (gateState[twInDn1] == !logicOn)
                                {
                                    gateState[twInDn1] = true;
                                    runEffectsClickOnTrig = true;
                                    if (outputLinkObj.Length > 0)
                                        setExtData(0, true, 0);
                                  

                                }

                            }

                        }


                        if (digitPosValue[1] > 0)
                        {
                            if (gateState[twInDn2] == logicOn)
                            {
                                gateState[twInDn2] = false;
                                runEffectsClickOffTrig = true;
                                if (outputLinkObj.Length > 1)
                                    setExtData(1, false, 0);
                            }
                            else
                            {
                                if (gateState[twInDn2] == !logicOn)
                                {
                                    gateState[twInDn2] = true;
                                    runEffectsClickOnTrig = true;
                                    if (outputLinkObj.Length > 1)
                                        setExtData(1, true, 0);
                                  

                                }

                            }


                        }



                        if (digitPosValue[2] > 0)
                        {
                            if (gateState[twInDn3] == logicOn)
                            {
                                gateState[twInDn3] = false;
                                runEffectsClickOffTrig = true;
                                if (outputLinkObj.Length > 2)
                                    setExtData(2, false, 0);

                            }
                            else
                            {
                                if (gateState[twInDn3] == !logicOn)
                                {
                                    gateState[twInDn3] = true;
                                    runEffectsClickOnTrig = true;
                                    if (outputLinkObj.Length > 2)
                                        setExtData(2, true, 0);
                                 


                                }

                            }


                        }
                  
                    }


                }


                clockTimer = clockPulseWidth;

                if (isMaster && isDumpData && maxClockRepeats > 0)
                {
                    gateState[countPin] = true;
                }



            }

        }

        // chip is a countdown timer
        if (isCountDown && isCountDownTrig)
        {

            countDownTimer--;

            if (countDownTimer <= 0)
            {
                countDownTimer = countDownDuration;
                //  countDownTimer = countDownDuration;
                //    countDownCount++;

                //   if (countDownCount >= countDownPulseWith)
                //   {
                countDownCount = 0;
                isCountDownTrig = false;
                gateState[gateIn] = true;
                runEffectsClickOnTrig = true;

                setExtData(gateOut, logicOn, 0);
             


                //    }

            }


        }


        // chip is a counter
        if (isCounter && isCounterTrig)
        {

            counterClock++;

            isCounterTrig = false;
            gateState[gateIn] = false;
            pinState[gateIn] = false;


            if (counterClock >= counterMax)
            {
                counterClock = 0;
                isCounterTrig = false;
                gateState[gateOut] = true;
                pinState[gateOut] = true;
                runEffectsClickOnTrig = true;

                setExtData(gateOut, logicOn, 0);

                if (isMomentary)
                {

                    momemtaryTimer = momDuration;
                    momentaryTrig = true;
                }

            }


        }

        // chip is momentary on
        if (momentaryTrig)
        {
            momemtaryTimer--;

            if (momemtaryTimer <= 0f)
            {
                momemtaryTimer = momDuration;
                momentaryTrig = false;

               

                if (chipClass == "gate")
                {
                    gateState[gateIn] = false;

                }

                if (chipClass == "primitive" && gateState[primOutput])
                {
                    gateState[primIn0] = false;
                    gateState[primIn1] = false;
                    gateState[primOutput] = false;

                }

                if (chipClass == "triprimitive" && gateState[primOutput])
                {
                    gateState[tPrimIn0] = false;
                    gateState[tPrimIn1] = false;
                    gateState[tPrimIn2] = false;
                    gateState[tPrimOutput] = false;


                }

                if (chipClass == "axis")
                {

                    gateState[dirIn0] = false;
                    gateState[dirIn1] = false;


                }

                if (chipClass == "triswitch" || chipClass == "footpedal")
                {
                    gateState[swIn0] = false;
                    gateState[swIn1] = false;
                    gateState[swIn2] = false;

                }

                if (chipClass == "thumbwheel" || chipClass == "scorewheel")
                {
                    gateState[twIn1] = false;
                    gateState[twIn2] = false;
                    gateState[twIn3] = false;
                    gateState[twInDn1] = false;
                    gateState[twInDn2] = false;
                    gateState[twInDn3] = false;


                }

                if (chipClass == "status")
                {


                    gateState[statOut] = false;
                    gateState[statBut0] = false;
                    gateState[statBut1] = false;


                    if (repeatingAlarm)
                    {
                        gateState[statAlarm] = false;

                    }


                }


                if (chipClass == "tbutton")
                {


                    //   gateState[tButOut] = false;
                    gateState[tButIn0] = false;
                    gateState[tButIn1] = false;
                    gateState[tButIn2] = false;


                }

                if (chipClass == "valve")
                {

                    for (int i = 0; i < displayLedObj.Length; i++)
                    {
                        setExtData(displayCounterValue, false, 1);

                    }

                }



                if (chipClass == "keyboard")
                {
                    for (int i = 0; i < pinState.Length; i++)
                    {
                        if (pinState[i])
                        {
                            pinState[i] = false;
                        }

                    }
                }

            }

        }

        //  if (finalResetTrig)
        //   {

        //   finalResetTimer--;

        //   if (finalResetTimer <= 0)
        //    {
        //    finalResetTrig = false;
        //    finalResetTimer = finalResetTimerMax;

        //     if (chipType.ToUpper() == "BUFFER" || chipType.ToUpper() == "LAMP" || chipType.ToUpper() == "SWITCH" )
        //   {
        //        if (defaultState)
        //             gateState[gateIn] = logicOn;

        //       }



        //      }

        //   }

        if (isButtonHold && !isInReverse && isButtonHoldTrig[0])
        {
            if (selCurPos != selMaxPos)
            {
                if (gateState[dirOut0])
                {
                    gateState[dirIn0] = false;
                    runEffectsClickOffTrig = true;
                }
                else
                {
                    gateState[dirIn0] = true;
                    runEffectsClickOnTrig = true;
                }
            }


        }
        else
        {
            if (isButtonHold && isInReverse && isButtonHoldTrig[1])
            {
                if (selCurPos != 0)
                {
                    if (gateState[dirOut1])
                    {
                        gateState[dirIn1] = false;
                        runEffectsClickOffTrig = true;
                    }
                    else
                    {
                        gateState[dirIn1] = true;
                        runEffectsClickOnTrig = true;
                    }
                }

            }
        }


    }

    void updateMotors()
    {

        if (chipType.ToUpper() == "FAN" || chipType.ToUpper() == "MOTOR" && (gateState[dirOut0] || gateState[dirOut1]))
        {
            motorTimer --;
            if (motorTimer <= switchSteps)
            {


                motorTimer = motorTimerMax;
                if (isWindDown || ((chipType.ToUpper() == "FAN" || chipType.ToUpper() == "MOTOR") && (gateState[dirOut0] || gateState[dirOut1])))
                {
                    for (int i = 0; i < objectGroup.Length; i++)
                    {
                        if (objectGroup.Length > 0 && objectGroup[i])
                        {
                            if (!isInReverse)
                            {
                                selCurPos++;
                                if (selCurPos > 360)
                                    selCurPos = 0;

                                for (int ii = 0; ii < objectGroup.Length; ii++)
                                {
                                    if (objectGroup[ii])
                                    {
                                        moveLocalPart(objectGroup[ii], switchSteps, rotType);

                                        setExtData(tacometer, logicOn, 4);
                                        if (debugLevel > 1)
                                            Debug.Log("<color=blue>" + objectGroup[ii].name + " </color> rotation object Index = " + i.ToString() + ", Rotation type = " + rotType + ", isReverse = " + isInReverse);
                                    }

                                }



                            }
                            else
                            {
                                selCurPos--;
                                if (selCurPos < 0)
                                    selCurPos = 360;

                                for (int ii = 0; ii < objectGroup.Length; ii++)
                                {
                                    if (objectGroup[ii])
                                    {
                                        moveLocalPart(objectGroup[ii], -switchSteps, rotType);
                                        if (debugLevel > 1)
                                            Debug.Log("<color=blue>" + objectGroup[ii].name + " </color> rotation Object Index: " + i.ToString() + ", Rotation type: " + rotType + " isReverse= " + isInReverse);
                                    }
                                }


                            }



                        }
                    }

                }
            }
        }

        if (isWindDown)
        {
            outputState = false;
            windDownTimer++;
            switchSteps = switchSteps - .1f;
            if (switchSteps < 1)
                switchSteps = 1;

            if (debugLevel > 1)
                Debug.Log("<color=blue>" + gameObject.name + "</color> - windown in progress");


            if (windDownTimer >= windDownTimerMax)
            {
                windDownTimer = 0;
                momentaryTrig = false;
                switchSteps = motorTimerMax;
                motorTimer = motorTimerMax;


                for (int i = 0; i < gateState.Length; i++)
                {
                    gateState[i] = !logicOn;
                    pinState[i] = !logicOn;
                }


                setStatusLed(false);

                isWindDown = false;

                if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> - windown completed");
            }

        }

    }

    void updateEffects()
    {

        if (isFloatTrig)
            floatObj();

        if (isLookAtPlayer)
            gameObject.transform.LookAt(hudCamera.transform);

        if (runEffectsOnTrig || runEffectsClickOnTrig)
        {
            runEffects(true, runEffectsClickOnTrig);
            runEffectsOnTrig = false;
            runEffectsClickOnTrig = false;
        }
     
        if (runEffectsOffTrig || runEffectsClickOffTrig)
        {
            runEffects(false, runEffectsClickOffTrig);
            runEffectsOffTrig = false;
            runEffectsClickOffTrig = false;
        }



    }

    #endregion

    #region Lights and leds


    void updatePinsAndLeds()
    {

        if (chipClass == "")
            return;

        // displays
        if (chipType.ToLower() == "dekatron" || chipType.ToLower() == "digdisplay" || chipType.ToLower() == "nixi")
        {
            updateDisplayLeds();
            return;
        }

        updateGateLeds();


    }

    void updateGateLeds()
    {

        if (!GetComponent<UniLogicChip>() || isObjStructErr)
            return;


        string _chipType = chipType.ToUpper();
        string _chipClass = chipClass.ToLower();

        for (int i = 0; i < ledObj.Length; i++)
        {

            if (ledObj[i] != null && gateState.Length >= ledObj.Length)
            {

                if (gateState[i] == true && !ledOnTrig[i])
                {
                   
                    ledOnTrig[i] = true;
                    pinState[i] = true;
     
                    if ((isIllumSwitch) && ledObj[i].GetComponent<MeshRenderer>())
                    {

                        ledObj[i].GetComponent<Renderer>().GetPropertyBlock(led_propBlock);

                        if (_chipType == "STATUS")
                        {

                            if (i == statNormIn)
                            {
                     
                                ledOnTrig[statSoftErrIn] = false;
                                ledOnTrig[statHardErrIn] = false;
                                ledObj[i].GetComponent<Renderer>().material.color = statLedOn;

                                setEmission(ledObj[i], true, statLedOn);
                              

                            }

                            if (i == statSoftErrIn)
                            {

                                ledOnTrig[statNormIn] = false;
                                ledOnTrig[statHardErrIn] = false;
                                gateOnTrig[statHardErrIn] = false;

                                ledObj[i].GetComponent<Renderer>().material.color = softErrLed;

                                setEmission(ledObj[i], true, softErrLed);
                            }

                            if (i == statHardErrIn)
                            {
                                ledOnTrig[statNormIn] = false;
                                ledOnTrig[statSoftErrIn] = false;
                                gateOnTrig[statSoftErrIn] = false;

                                ledObj[i].GetComponent<Renderer>().material.color = hardErrLed;

                                setEmission(ledObj[i], true, hardErrLed);
                            }

                            if (i == statBut0 || i == statBut1)
                            {
                                ledObj[i].GetComponent<Renderer>().material.color = ledOn;

                                setEmission(ledObj[i], true, ledOn);
                            }


                        }
                        else
                        {
                            if (led_propBlock.GetColor("_Color") != ledOn)
                            {

                                led_propBlock.SetColor("_Color", ledOn);
                              //  led_propBlock.SetColor("_EmissionColor", ledOn*7f);
                                ledObj[i].GetComponent<Renderer>().SetPropertyBlock(led_propBlock);
                                setEmission(ledObj[i], true, ledOn);
                            }


                        }
                    }

                }

               
                if (gateState[i] == false && ledOnTrig[i])
                {

                    ledOnTrig[i] = false;
                    pinState[i] = false;
               
                    if (isIllumSwitch && ledObj[i].GetComponent<MeshRenderer>())
                    {
                        ledObj[i].GetComponent<Renderer>().GetPropertyBlock(led_propBlock);

                        if (led_propBlock.GetColor("_Color") != originalColor[i])
                        {
                            led_propBlock.SetColor("_Color", originalColor[i]);
                         //   led_propBlock.SetColor("_EmissionColor", originalColor[i]*7f);
                            ledObj[i].GetComponent<Renderer>().SetPropertyBlock(led_propBlock);
                            setEmission(ledObj[i], false, originalColor[i]);
                        }

                    }


                }
            }
        }



    }

    void updateDisplayLeds()
    {
        if (displayLedObj.Length > 0)
        {
            for (int i = 0; i < driverState.Length; i++)
            {

                if (driverState[i] == true && !dispLedOnTrig[i])
                { 
                    dispLedOnTrig[i] = true;
                    displayLedObj[i].GetComponent<Renderer>().GetPropertyBlock(dispLed_propBlock);
                    
                    dispLed_propBlock.SetColor("_Color", ledOn);
                    displayLedObj[i].GetComponent<Renderer>().SetPropertyBlock(dispLed_propBlock);
                    setEmission(displayLedObj[i], true, ledOn);

                    if (i < displayLedSlaveObj.Length && displayLedSlaveObj[i])
                    {

                        if (dispLed_propBlock.GetColor("_Color") != ledOn)
                        {

                            dispLed_propBlock.SetColor("_Color", ledOn);
                            displayLedSlaveObj[i].GetComponent<Renderer>().SetPropertyBlock(dispLed_propBlock);
                            setEmission(displayLedSlaveObj[i], true, ledOn);
                        }
                    }


                }
                else
                {
                    if (driverState[i] == false && dispLedOnTrig[i])
                    {

                        dispLedOnTrig[i] = false;

                        displayLedObj[i].GetComponent<Renderer>().GetPropertyBlock(dispLed_propBlock);

                        if (dispLed_propBlock.GetColor("_Color") != originalDispColor[i])
                        {
                            dispLed_propBlock.SetColor("_Color", originalDispColor[i]);
                            displayLedObj[i].GetComponent<Renderer>().SetPropertyBlock(dispLed_propBlock);
                            setEmission(displayLedObj[i], false, originalDispColor[i]);
                        }

                     //   if (i < displayLedSlaveObj.Length && displayLedSlaveObj[i])
                     //   {

                    //        dispLed_propBlock.SetColor("_Color", originalDispColor[i]);
                   //         {

                     //           dispLed_propBlock.SetColor("_Color", originalDispColor[i]);
                   //             displayLedSlaveObj[i].GetComponent<Renderer>().SetPropertyBlock(dispLed_propBlock);
                    //            setEmission(displayLedSlaveObj[i], true, originalDispColor[i]);
                    //        }
                 //       }
                    }

                }
            }

        }

    }

    void setDisplayDigit(int num)
    {

        for (int i = 0; i < 7; i++)
        {
            if (digitLedSegBit[num, i])
                driverState[i] = true;
            else
                driverState[i] = false;
        }


    }

    void setNixiDigit(int num)
    {
        for (int i = 0; i < 10; i++)
        {
            if (i == num)
                driverState[i] = true;
            else
                driverState[i] = false;
        }


    }

    public void setStatusLed(bool state)
    {

        if (chipClass == "keyboard" || statusLedObj.Length == 0)
            return;


        for (int i = 0; i < statusLedObj.Length; i++)
        {
            if (statusLedObj[i] != null && statusLedObj[i].GetComponent<Renderer>())
            {
                statusLedObj[i].GetComponent<Renderer>().GetPropertyBlock(statLed_propBlock);

                if (state)
                {

                    if (alarmlevel == 0)
                    {
                        if (debugLevel > 0)
                            Debug.Log("<color=blue>" + gameObject.name + " </color> alarm=0, color=" + statLedOn.ToString());

                        if (statLed_propBlock.GetColor("_Color") != statLedOn)
                        {
                            statLed_propBlock.SetColor("_Color", statLedOn);
                            statusLedObj[i].GetComponent<Renderer>().SetPropertyBlock(statLed_propBlock);
                            setEmission(statusLedObj[i], true, statLedOn);
                        }


                    }

                    if (alarmlevel == 1)
                    {
                        if (debugLevel > 0)
                            Debug.Log("<color=blue>" + gameObject.name + " </color> alarm=1" + softErrLed.ToString());

                        statusLedObj[i].GetComponent<Renderer>().material.color = softErrLed;
                        setEmission(statusLedObj[i], true, softErrLed);

                    }

                    if (alarmlevel == 2)
                    {
                        if (debugLevel > 0)
                            Debug.Log("<color=blue>" + gameObject.name + " </color> alarm=2" + hardErrLed.ToString());
                        statusLedObj[i].GetComponent<Renderer>().material.color = hardErrLed;
                        setEmission(statusLedObj[i], true, hardErrLed);

                    }

                }

                if (!state)
                {
                    //  setExtData(0, state, 3);

                  
                    if (statLed_propBlock.GetColor("_Color") != originalStatColor[i])
                    {
                       

                        statLed_propBlock.SetColor("_Color", originalStatColor[i]);

                        statusLedObj[i].GetComponent<Renderer>().SetPropertyBlock(statLed_propBlock);
                        setEmission(statusLedObj[i], false, originalStatColor[i]);
                    }

                    if (debugLevel > 0)
                        Debug.Log("<color=blue>" + gameObject.name + " </color> alarm = off");


                }


            }

        }


    }

    #endregion

    #region Move parts

    void moveLocalPart(GameObject _part, float _step, int _plane)
    {
        if (_part == null)
            return;

        Quaternion currentRotation = _part.transform.rotation;
        Vector3 currentPosition = _part.transform.position;
        //    Quaternion wantedRotation = Quaternion.Euler(0, 0, angle);
        //    part.transform.rotation = Quaternion.RotateTowards(currentRotation, wantedRotation, Time.deltaTime * 90);

        //  Debug.Log(_plane);

        if (_plane == moveType.rotLR)
            _part.transform.Rotate(0, 0, _step);

        if (_plane == 1)
            _part.transform.Rotate(0, _step, 0);

        if (_plane == 2)
            _part.transform.Rotate(_step, 0, 0);

        if (_plane == 3)
        {
            //  if (Mathf.Abs(Vector3.Dot(transform.right, Vector3.down)) < 0.825f)
            //     _part.transform.position = new Vector3(currentPosition.x, currentPosition.y + _step, currentPosition.z);

            //  if (Mathf.Abs(Vector3.Dot(transform.right, Vector3.down)) > 0.825f)
            _part.transform.position = new Vector3(currentPosition.x - _step, currentPosition.y, currentPosition.z);
        }

        if (_plane == 4)
        {
            _part.transform.position = new Vector3(currentPosition.x + _step, currentPosition.y, currentPosition.z);

        }

        if (_plane == 5)
        {
            _part.transform.Rotate(0, -_step, 0);
        }

        if (_plane == 6)
        {
            _part.transform.Rotate(-_step, 0, 0);
        }



    }

    void checkMechState()
    {

        propRot = transform.rotation;
        propVect = transform.position;


        if (objectGroup.Length == 0)
            return;

        if (chipType.ToUpper() == "SLIDE")
        {

            if (selMaxPos > 48)
            {
                selMaxPos = 0;
                switchSteps = 0;
            }

            float rotz = transform.rotation.eulerAngles.z;
            float roty = transform.rotation.eulerAngles.y;

            if (switchSteps == 0)
            {
                if (roty != 0)
                    switchSteps = .05f;

                if (rotz == 270)
                    switchSteps = .12f;

                if (rotz == 0)
                    switchSteps = .1f;

            }

            if (selMaxPos == 0)
            {
                if (roty != 0)
                {
                    selMaxPos = 40;
                    selMaxPos = selMaxPos * transform.localScale.y;
                }

                if (rotz == 0f && roty == 0)
                {
                    selMaxPos = 60;
                    selMaxPos = selMaxPos * transform.localScale.y;
                }

                if (rotz == 270 && roty == 0)
                {
                    selMaxPos = 60;
                    selMaxPos = selMaxPos * transform.localScale.x;
                }

            }

        }

        if (chipType.ToUpper() == "DIAL" || chipType.ToUpper() == "METER" || chipType.ToUpper() == "FAN" || chipType.ToUpper() == "MOTOR")
        {

            if (selMaxPos > 0 && switchSteps == 0)
                switchSteps = 360 / selMaxPos;
            else
                 if (switchSteps < 0)
                switchSteps = 10;

            if (switchSteps > 199)
                switchSteps = 10;

        }

        if (chipType.ToUpper() == "RELAY" && gateOnTrig[relayCoilPin])
        {
            if (objectGroup.Length > 0 && objectGroup[0])
                moveLocalPart(objectGroup[0], -(360 / (256 - switchSteps)), rotType);
        }

        if (chipType.ToUpper() == "SWITCH" || chipType.ToUpper() == "FLIPPER")
        {
            float swSize = .05f;


            if (GetComponentInChildren<Collider>())
                swSize = GetComponentInChildren<Collider>().bounds.size.z * 2;

            if (debugLevel > 1)
                Debug.Log("<color=blue>" + gameObject.name + "</color> switch size " + swSize.ToString());

            float newSwSize = swSize / 7;

            if (newSwSize <= 0)
                newSwSize = .05f;

            if (switchSteps == 0)
                switchSteps = newSwSize;

            if (objectGroup.Length > 0 && outputState)
            {

                for (int i = 0; i < objectGroup.Length; i++)
                {
                    if (objectGroup[i])
                    {
                        if (animType.ToLower() == "breaker")
                            moveLocalPart(objectGroup[i], switchSteps, rotType);

                        if (animType.ToLower() == "rocker")
                            moveLocalPart(objectGroup[i], switchSteps, rotType);

                        if (animType.ToLower() == "pusht2")
                            objectGroup[i].transform.Translate(Vector3.left * switchSteps);

                        if (animType.ToLower() == "pusht1")
                            objectGroup[i].transform.Translate(Vector3.back * switchSteps);

                        if (animType.ToLower() == "push" || animType.ToLower() == "pushL")
                            objectGroup[i].transform.Translate(Vector3.forward * switchSteps);

                        if (debugLevel > 1)
                            Debug.Log("<color=blue>" + gameObject.name + "</color> rest position ");
                    }
                }

            }

            outputState = false;

        }

    }

    #endregion

    #region Run effects and emmissions

    private void runEffects(bool state, bool _isClickPin)
    {

        if (!isSystemReady)
            return;

        bool _isOutputBlocked = false;

        if (dependantObj != null)
          if (dependantObj.GetComponent<UniLogicChip>().outputState == false)
                _isOutputBlocked = true;


        if (GetComponent<UniLogicEffects>())
              GetComponent<UniLogicEffects>().ToggleAnim(state, _isClickPin, _isOutputBlocked);
        
    }

    private void setEmission(GameObject obj, bool state, Color color)
    {

        if (obj.GetComponent<MeshRenderer>())
        {
            MeshRenderer meshRend = obj.GetComponent<MeshRenderer>();
            meshRend.material.SetColor("_EmissionColor", color);

            bool isEmissionsEnabled = IsEmissionsEnabled(meshRend.material.GetColor("_EmissionColor"), state);

            if (isEmissionsEnabled)
                meshRend.material.EnableKeyword("_EMISSION");
            else
                meshRend.material.DisableKeyword("_EMISSION");
        }

    }

    static bool IsEmissionsEnabled(Color color, bool state)
    {
        if (state)
            return color.maxColorComponent > (0.1f / 255.0f);
        else
            return false;
    }

    #endregion

    #region Connection link lines and traces

    void defineLinkLines(bool leaveEnabled)
    {
        // if (outputLinkObj.Length == 0)
        //     return;

        if (!gameObject.GetComponent<LineRenderer>())
        {
            LineRenderer lineObj = gameObject.AddComponent<LineRenderer>();

            if (lineObj)
                if (debugLevel > 2)
                    Debug.Log("Line Object renderer created for <color=blue>" + gameObject.name + "</color>");
        }

        gameObject.GetComponent<LineRenderer>().startWidth = linkLineWidth;
        gameObject.GetComponent<LineRenderer>().endWidth = linkLineWidth;
        gameObject.GetComponent<LineRenderer>().startColor = LineStartColor;
        gameObject.GetComponent<LineRenderer>().endColor = LineEndColor;
        gameObject.GetComponent<LineRenderer>().positionCount = 2;
        gameObject.GetComponent<LineRenderer>().material = new Material(Shader.Find(linkMaterial));


        if (!leaveEnabled)
            gameObject.GetComponent<LineRenderer>().enabled = false;


        for (int i = 0; i < pinObj.Length; i++)
        {
            if (pinObj[i])
            {
                if (!pinObj[i].GetComponent<LineRenderer>())
                {
                    LineRenderer line = pinObj[i].AddComponent<LineRenderer>();
                    if (line)
                        if (debugLevel > 2)
                            Debug.Log("Line Pin renderer created for <color=blue>" + gameObject.name + "</color>");

                }

                pinObj[i].GetComponent<LineRenderer>().startWidth = linkLineWidth;
                pinObj[i].GetComponent<LineRenderer>().endWidth = linkLineWidth;
                pinObj[i].GetComponent<LineRenderer>().positionCount = 2;
                pinObj[i].GetComponent<LineRenderer>().startColor = LineStartColor;
                pinObj[i].GetComponent<LineRenderer>().endColor = LineEndColor;

                pinObj[i].GetComponent<LineRenderer>().material = new Material(Shader.Find(linkMaterial));
                if (!leaveEnabled)
                    pinObj[i].GetComponent<LineRenderer>().enabled = false;

            }


        }

        if (GetComponent<UniLogicEffects>())
        {
            for (int i = 0; i < GetComponent<UniLogicEffects>().toggleOnObject.Length; i++)
            {
                if (GetComponent<UniLogicEffects>() && GetComponent<UniLogicEffects>().toggleOnObject[i])
                {


                    if (!GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>())
                    {
                        LineRenderer line = GetComponent<UniLogicEffects>().toggleOnObject[i].AddComponent<LineRenderer>();

                        if (line)
                            if (debugLevel > 2)
                                Debug.Log("Line renderer created for <color=blue>" + gameObject.name + "</color>");

                    }

                    GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>().startWidth = linkLineWidth;
                    GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>().endWidth = linkLineWidth;
                    GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>().positionCount = 2;
                    GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>().startColor = LineStartColor;
                    GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>().endColor = LineEndColor;
                    GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>().material = new Material(Shader.Find(linkMaterial));
                    if (!leaveEnabled)
                        GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>().enabled = false;
                }

            }
        }

    }

    void clearLinkLines()
    {

        if (gameObject.GetComponent<LineRenderer>())
            gameObject.GetComponent<LineRenderer>().positionCount = 0;

        for (int i = 0; i < pinObj.Length; i++)
            if (pinObj[i] && pinObj[i].GetComponent<LineRenderer>())
                pinObj[i].GetComponent<LineRenderer>().positionCount = 0;

        if (GetComponent<UniLogicEffects>())
            for (int i = 0; i < GetComponent<UniLogicEffects>().toggleOnObject.Length; i++)
                if (GetComponent<UniLogicEffects>().toggleOnObject[i] && GetComponent<UniLogicEffects>().toggleOnObject[i] != null && GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>())

                    GetComponent<UniLogicEffects>().toggleOnObject[i].GetComponent<LineRenderer>().positionCount = 0;

        if (debugLevel > 3)
            Debug.Log("<color=blue>" + gameObject.name + "</color> toggle links");

    }

    void drawLinks()
    {

        for (int i = 0; i < outputLinkObj.Length; i++)
        {

            if (outputLinkObj[i] && outputLinkObj[i] != null && outputLinkObj[i].GetComponent<UniLogicChip>())
            {

                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipType.ToUpper() == "LAMP" || outputLinkObj[i].GetComponent<UniLogicChip>().chipType.ToUpper() == "SWITCH" && outputLinkPin[i] == gateIn)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().gameObject, gameObject);

                if (outputLinkPin.Length > 0 && i <= outputLinkPin.Length)
                    if (outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "gate" && outputLinkPin[i] == gateOut)
                        drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[gateOut], gameObject);

                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "primitive" && outputLinkPin[i] == primIn0)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[primIn0], gameObject);

                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "primitive" && outputLinkPin[i] == primIn1)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[primIn1], gameObject);


                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "triprimitive" && outputLinkPin[i] == tPrimIn0)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[tPrimIn0], gameObject);
                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "triprimitive" && outputLinkPin[i] == tPrimIn1)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[tPrimIn1], gameObject);
                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "triprimitive" && outputLinkPin[i] == tPrimIn2)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[tPrimIn2], gameObject);


                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipType.ToUpper() == "VALVE" && outputLinkPin[i] == countPin)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[countPin], gameObject);

                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipType.ToUpper() == "TRISWITCH" && outputLinkPin[i] == swIn0)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[swIn0], gameObject);

                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipType.ToUpper() == "RELAY" && outputLinkPin[i] == relayCoilPin)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[relayCoilPin], gameObject);


                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "axis" && outputLinkPin[i] == dirIn0)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[dirIn0], gameObject);

                if (outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "axis" && outputLinkPin[i] == dirIn1)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[dirIn1], gameObject);

                if ((outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "thumbwheel" || outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "scorewheel") && outputLinkPin[i] == twIn1)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[twIn1], gameObject);
                if ((outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "thumbwheel" || outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "scorewheel") && outputLinkPin[i] == twIn2)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[twIn2], gameObject);
                if ((outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "thumbwheel" || outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "scorewheel") && outputLinkPin[i] == twIn3)
                    drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[twIn3], gameObject);

                for (int ix = 0; ix < 12; ix++)
                {
                    if (outputLinkObj[i].GetComponent<UniLogicChip>().chipClass == "keyboard" && outputLinkPin[i] == ix)
                        drawTrace(outputLinkObj[i].GetComponent<UniLogicChip>().pinObj[ix], gameObject);
                }


            }



        }

        if (GetComponent<UniLogicEffects>())
        {

            for (int i = 0; i < GetComponent<UniLogicEffects>().toggleOnObject.Length; i++)
            {
                if (GetComponent<UniLogicEffects>().toggleOnObject[i] && GetComponent<UniLogicEffects>().toggleOnObject[i] != null)
                {
                    drawTrace(gameObject, GetComponent<UniLogicEffects>().toggleOnObject[i]);
                }
            }

        }

    }

    void drawTrace(GameObject startPoint, GameObject endPoint)
    {

        if (startPoint.GetComponent<LineRenderer>() && startPoint != null && endPoint != null)
        {
            if (startPoint.GetComponent<LineRenderer>().positionCount > 1)
            {
                startPoint.GetComponent<LineRenderer>().SetPosition(0, startPoint.transform.position);
                startPoint.GetComponent<LineRenderer>().SetPosition(1, endPoint.transform.position);
                startPoint.GetComponent<LineRenderer>().startColor = Color.green;

            }
        }
    }


    #endregion

    #region Chip initialzation

    void resetGlobal()
    {

        //reset chip
        foreach (var resetObj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
            if (!resetObj.GetComponent<UniLogicChip>().isLockedState)
            {
                for (int i = 0; i < resetObj.GetComponent<UniLogicChip>().gateLen; i++)
                {
                    resetObj.GetComponent<UniLogicChip>().gateState[i] = false;
                    resetObj.GetComponent<UniLogicChip>().pinState[i] = false;
                }

                resetObj.GetComponent<UniLogicChip>().chipReset(true);
            }

        Debug.Log("<color=blue>" + gameObject.name + "</color> Run sub function to reset " + funcParam + " " + circuitGroup);

    }


    void chipReset(bool isInsitu)
    {
        isSystemReady = false;
        posOffset = transform.position;

        if (isInsitu)
        {
          
            if (gateState.Length > 0)
            {
                gateState[0] = false;
                pinState[0] = false;
               
            }

          

        }

       
        isHighSoftErrTrig = false;
        isLowSoftErrTrig = false;
        isHighHardErrTrig = false;
        isLowHardErrTrig = false;

        isShowLocalLinks = false;
        isDisableLeds = false;
        isClockTrig = false;
        objectGroupValue = 0;
        displayCounterValue = 0;
        dumpCount = 0;
        clockTimer = clockPulseWidth;
        isCountDownTrig = false;
        isInReverse = false;
        isLatched = false;
        isButtonBlocked = false;

        if (countDownDuration == 0)
            countDownDuration = 10;
        countDownTimer = countDownDuration;
        countDownCount = 0;
        currentClickPin = -1;
       
        counterClock = 0;
        isCounterTrig = false;
   
        if (isClock)
            isMomentary = true;

        if (isMomentary)
            momemtaryTimer = momDuration;

        if (isClock)
            countDownCount = maxClockRepeats;
        else
            countDownCount = 0;

        finalResetTimer = finalResetTimerMax;


        pinReset();
        checkMechState();

        if (!isInsitu)
        {
            if (windDownTimerMax == 0)
                windDownTimerMax = windDownLen;

            windDownTimer = 0;
            isWindDown = false;
         
            paramInit();

            if (isEnableLinks)
                defineLinkLines(true);
                                   
        }
        else
        {
          
            isSystemReady = true;
        }

       

    }

    void pinReset()
    {

        for (int i = 0; i < pinState.Length; i++)
            pinState[i] = false;

        for (int i = 0; i < gateState.Length; i++)
            gateState[i] = false;

        for (int i = 0; i < gateOnTrig.Length; i++)
            gateOnTrig[i] = false;

        for (int i = 0; i < ledOnTrig.Length; i++)
            ledOnTrig[i] = false;

        for (int i = 0; i < driverState.Length; i++)
            driverState[i] = false;

        if (chipType.ToUpper() != "KEYBOARD")
        {
            for (int i = 0; i < ledObj.Length; i++)
            {
                if (ledObj[i] != null)
                {
                    if (ledObj[i].GetComponent<Renderer>())
                    {
                        if (isIllumSwitch)
                        {
                            ledObj[i].GetComponentInParent<Renderer>().material.color = originalColor[i];
                            setEmission(ledObj[i], false, originalColor[i]);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < displayLedObj.Length; i++)
            if (displayLedObj[i])
                displayLedObj[i].GetComponent<Renderer>().material.color = originalDispColor[i];


        for (int i = 0; i < digitPosValue.Length; i++)
            digitPosValue[i] = 0;


        if(chipType.ToUpper()=="DIGDISPLAY")
        {
            setDisplayDigit(0);
        }

    }

    void paramInit()
    {

        if (chipType.ToUpper() == "STATUS" || chipType.ToUpper() == "TBUTTON")
        {
            if (isMomentary && momDuration == 0)
                momDuration = 10;

            populateStatusIcons();

            if (defaultState && chipType.ToUpper() == "TBUTTON")
            {
                gateState[tButIn2] = true;
                pinState[tButIn2] = true;
            }

        }

        if (chipType.ToUpper() != "KEYBOARD")
        {

            for (int i = 0; i < ledObj.Length; i++)
            {
                if (ledObj[i] != null && gateState.Length >= ledObj.Length)
                {
                    if (ledObj[i].GetComponentInParent<Renderer>())
                        ledObj[i].GetComponentInParent<Renderer>().material.color = originalColor[i];
                }

            }
        }

        if (chipType.ToUpper() == "NOT")
        {
            if (defaultState)
            {
                gateState[gateOut] = false;
                gateState[gateIn] = true;

            }
        }

        if (chipType.ToUpper() == "TRISWITCH")
        {
            if (objectGroup.Length > 0 && objectGroup[0])
                moveLocalPart(objectGroup[0], 0, rotType);

            if (switchSteps == 0)
                switchSteps = 20;

            if (defaultState)
                gateState[swIn0] = logicOn;

        }

        if (chipType.ToUpper() == "THUMBWHEEL" || chipType.ToUpper() == "SCOREWHEEL")
        {
            if (objectGroup.Length > 0 && objectGroup[0])
                moveLocalPart(objectGroup[0], 0, rotType);

            if (clockPulseWidth == 0)
                clockPulseWidth = 5;

            if (clockPrecision == 0)
                clockPrecision = .2f;

            isMomentary = true;

            if (momDuration == 0)
                momDuration = 5;

            digitPosValue[0] = 0;
            digitPosValue[1] = 0;
            digitPosValue[2] = 0;

            selMaxPos = 10;
            isClock = false;
            maxClockRepeats = 0;


        }

        if (chipType.ToUpper() == "BUFFER" || chipType.ToUpper() == "LAMP" || chipType.ToUpper() == "SWITCH" || chipType.ToUpper() == "FLIPPER")
        {
            if (chipType.ToUpper() == "BUFFER" || chipType.ToUpper() == "LAMP")
                isIllumSwitch = true;

            gateOnTrig[gateOut] = false;

           
            if (chipType.ToUpper() == "BUFFER" || chipType.ToUpper() == "LAMP")
                isIllumSwitch = true;


            if (chipType.ToUpper() == "SWITCH")
            {

                if (animType == "")
                    animType = "push";

                if (isCounter)
                    momemtaryTimer = momDuration;


            }


            if (chipType.ToLower() == "lamp")
            {
                isIllumSwitch = true;
            }


            if (objectGroup.Length > 0)
            {
                if (animType.ToLower() == "rocker")
                    moveLocalPart(objectGroup[0], 0, rotType);

                if (animType.ToLower() == "push")
                    moveLocalPart(objectGroup[0], 0, rotType);

                if (animType.ToLower() == "rotate")
                    moveLocalPart(objectGroup[0], defaultPos, rotType);

            }


            if (defaultState)
            {
                gateState[gateIn] = logicOn;
                pinState[gateIn] = logicOn;
            }
        }

        if (chipType.ToUpper() == "SLIDE")
        {

            if (defaultPos > 0)
            {
                selCurPos = defaultPos;

                if (defaultPos >= selMaxPos)
                    isInReverse = true;

                for (int i = 0; i < defaultPos; i++)
                    moveLocalPart(objectGroup[0], switchSteps, rotType);
            }

            isMomentary = true;
            isButtonHold = true;


        }

        if (chipType.ToUpper() == "UPDN" || chipType.ToUpper() == "DIAL" || chipType.ToUpper() == "METER")
        {

            selCurPos = 0;
            displayCounterValue = 0;
            if (switchSteps == 0)
                switchSteps = 2;
            if (switchSteps > 360)
                switchSteps = 360;

            if (chipType.ToUpper() == "UPDN")
            {
                if (isButtonHold)
                    selMaxPos = 0;
            }

            if (chipType.ToUpper() == "METER")
            {
                animType = "rotate";
                rotType = 6;

                if (selMinPos == 0)
                    selMinPos = 10;

                if (selMaxPos == 0)
                    selMaxPos = 180;

            }


            if (defaultPos > 0)
            {
                if (defaultPos > 360)
                    defaultPos = 360;
                if (defaultPos < 0)
                    defaultPos = 0;

                selCurPos = defaultPos;

                float defPosAdj = defaultPos / switchSteps;
                if (defPosAdj < 0)
                    defPosAdj = 0;


                for (int i = 0; i < defPosAdj; i++)
                {

                    if (chipType.ToUpper() == "METER")
                    {
                        moveLocalPart(objectGroup[0], switchSteps, rotType);
                    }
                    else
                        moveLocalPart(objectGroup[0], -switchSteps, rotType);


                    displayCounterValue++;
                }

            }
        }

        if (chipType.ToUpper() == "FAN" || chipType.ToUpper() == "MOTOR")
        {

            if (switchSteps == 0)
                switchSteps = motorTimeLen;

            motorTimerMax = switchSteps;
            motorTimer = motorTimerMax;

            switchSteps = motorTimerMax;
            motorTimer = motorTimerMax;
            windDownTimer = windDownTimerMax;
            animType = "rotate";

            if (switchSteps == 0)
                switchSteps = motorTimerMax;


            for (int i = 0; i < objectGroup.Length; i++)
            {
                for (int ii = 0; ii < defaultPos; ii++)
                {
                    if (objectGroup[i] && objectGroup[i] != null)
                        moveLocalPart(objectGroup[i], switchSteps, rotType);
                }
            }

            if (defaultState)
            {
                gateState[0] = true;
                pinState[0] = true;
            }

        }

        if (chipType.ToUpper() == "DFF")
        {
            if (isMaster)
                gateState[dataPin] = true;

        }

        if (chipType.ToUpper() == "DIGDISPLAY")
        {
            if (defaultDigVal == 0)
                setDisplayDigit(0);
            else
                setDisplayDigit(defaultDigVal);

        }

        if (chipType.ToUpper() == "NIXI")
        {
            setNixiDigit(0);
            isDisableLeds = false;
            gateState[dirDownPin] = false;
            gateState[carryOutPin] = false;
            pinState[0] = false;
            gateState[0] = false;


        }

        if (chipType.ToUpper() == "DEKATRON" || chipType.ToUpper() == "UNDEKATRON")
        {

            displayLedObj[0].GetComponent<Renderer>().material.color = ledOn;
            setEmission(displayLedObj[0], true, ledOn);

            if (isClock && isAutoStart)
                pinState[countPin] = true;

            if (displayCounterValue > 0)
            {
                for (int i = 0; i < displayCounterValue; i++)
                {
                    if (isInReverse)
                    {
                        moveLocalPart(objectGroup[0], -(360 / displayLedObj.Length), rotType);
                    }

                    if (!isInReverse)
                    {
                        moveLocalPart(objectGroup[0], +(360 / displayLedObj.Length), rotType);
                    }

                }

            }

        }

        if (chipType.ToUpper() == "RELAY")
        {
            gateState[relayNOPin] = relayCommonLevel;
            gateState[relayNCPin] = !relayCommonLevel;
            //  rotatePart(objectGroup[0], 0, rotType);
            gateOnTrig[relayCoilPin] = false;

        }

        if (chipType.ToUpper() == "AND" || chipType.ToUpper() == "IF")
        {
            gateState[primOutput] = false;
            gateState[primIn0] = false;
            gateState[primIn1] = false;

        }

        if (chipType.ToUpper() == "TRIAND")
        {
            gateState[tPrimOutput] = false;
            gateState[tPrimIn0] = false;
            gateState[tPrimIn1] = false;
            gateState[tPrimIn2] = false;

        }

        if (chipType.ToUpper() == "KEYBOARD")
        {
            //  animType = "rocker";
            if (momDuration == 0)
                momDuration = .05f;

            if (switchSteps == 0)
                switchSteps = 3;

            selCurPos = 0;

        }

        if (defaultState && chipType.ToUpper() == "NAND")
        {

            gateState[primIn0] = false;
            gateState[primIn1] = false;
            gateState[primOutput] = true;

        }

       
        /// button holds and momentary resets
         #region button holds and momentary resets

        if (isButtonHold && momDuration > 0)
        {
            isMomentary = true;
            if (momDuration == 0)
                momDuration = 5 + (outputLinkObj.Length * .02f);

        }


        for (int i = 0; i < statusLedObj.Length; i++)
        {

            if (statusLedObj[i] && statusLedObj[i].GetComponent<Renderer>())
            {
                statusLedObj[i].GetComponent<Renderer>().material.color = originalStatColor[i];
                setEmission(statusLedObj[i], false, originalStatColor[i]);
            }
        }



        if (isAutoStart)
            isClockTrig = true;

        //   finalResetTrig = true;

        #endregion


       
        isSystemReady = true;
        baseSwitchSteps = switchSteps;

    }

    void setChainDir(bool _isReverse)
    {

        isInReverse = _isReverse;
        nextObj = gameObject;
        nextLink = nextObj.GetComponent<UniLogicChip>();

        if (nextLink.outputLinkObj.Length > 0 && nextLink.outputLinkObj[0] != null)
        {
            nextObj = outputLinkObj[0].gameObject;
            nextLink = nextObj.GetComponent<UniLogicChip>();

            for (int i = 0; i < maxInChain; i++)
            {
                if (nextLink.outputLinkObj.Length > 0 && nextLink.outputLinkObj[0] != null)
                {
                    nextLink.pinState[dirDownPin] = _isReverse;
                    nextLink.isInReverse = _isReverse;
                    nextObj = nextLink.outputLinkObj[0].gameObject;
                    nextLink = nextObj.GetComponent<UniLogicChip>();

                }
            }
        }


    }

    void populateStatusIcons()
    {
        if (defaultIcon)
            defaultIcon.SetActive(true);

        //    for (int i = 0; i < statusIcons.Length; i++)
        //         if (statusIcons[i])
        //             if (i == defaultIcon)
        //                IconAcnchor = statusIcons[i];


    }

    #endregion

    #region Data structures
    void setupLogicGrid()
    {
        if (chipType == "")
        {
            isObjStructErr = true;
            Debug.LogWarning("<color=blue>" + gameObject.name + "</color> - chipType not defined in uniLogicChip.cs inspector. All LB functions disabled for this object");
            return;
        }

        if (pinObj.Length == 0)
        {
            isObjStructErr = true;
            Debug.LogWarning("<color=blue>" + gameObject.name + "</color> - Logic Blox object has not been setup with valid pinObj. Have pin objects been entered in uniLogicChip.cs inspector ? ");
            return;
        }

        if (pinObj.Length > 0 && !pinObj[0].GetComponentInChildren<Collider>() && debugLevel > 0)
        {
            Debug.LogWarning("<color=blue>" + gameObject.name + "</color> - collider not found in pinObj index 0. Click events are disabled for this objects pin");
        }

        if (ledObj.Length == 0 && debugLevel > 0)
            Debug.LogWarning("<color=blue>" + gameObject.name + "</color> (leds and or buttons) ledObj not found. Leds or buttons disabled for this object. Has led/button object been entered in uniLogicChip.cs inspector?");

        if (ledObj.Length > 0 && ledObj[0] != null && !ledObj[0].GetComponentInChildren<Collider>() && debugLevel > 0)
            Debug.LogWarning("<color=blue>" + gameObject.name + "</color> - collider not found in ledObj index 0. Click events are disabled for this objects led/button");

        if (debugLevel > 0)
            Debug.Log("<color=blue>" + gameObject.name + "</color> > UniLogicChip.cs - verbose debug text enabled at level " + debugLevel);


        for (int i = 0; i < tagMax; i++)
            addGameTag(i.ToString());

        if (ledObj.Length > 0)
            originalColor = new Color[ledObj.Length];

        if (statusLedObj.Length > 0)
            originalStatColor = new Color[statusLedObj.Length];

        if (displayLedObj.Length > 0)
            originalDispColor = new Color[displayLedObj.Length];


        for (int i = 0; i < statusLedObj.Length; i++)
        {
            if (statusLedObj[i])
            {

                if (statusLedObj[i].GetComponent<MeshFilter>() && statusLedObj[i].GetComponent<Renderer>())
                {


              //      statusLedObj[i].GetComponent<Renderer>().GetPropertyBlock(statLed_propBlock);
              //      originalStatColor[i] = statLed_propBlock.GetColor("_Color");

                   if (statusLedObj[i].GetComponent<Renderer>().material && statusLedObj[i].GetComponent<Renderer>().material.HasProperty("_Color"))
                       originalStatColor[i] = statusLedObj[i].GetComponent<Renderer>().material.GetColor("_Color");

                   if (statusLedObj[i].GetComponent<Renderer>().material && statusLedObj[i].GetComponent<Renderer>().material.HasProperty("_TintColor"))
                       originalStatColor[i] = statusLedObj[i].GetComponent<Renderer>().material.GetColor("_TintColor");
                }
            }
        }


        for (int i = 0; i < ledObj.Length; i++)
        {

            if (ledObj[i])
            {
                if (i <= tagMax)
                ledObj[i].tag = i.ToString();

                if (ledObj[i].GetComponent<MeshFilter>() && ledObj[i].GetComponent<Renderer>())
                {


                //    ledObj[i].GetComponent<Renderer>().GetPropertyBlock(led_propBlock);

               //     originalColor[i] = led_propBlock.GetColor("_Color");


                   if (ledObj[i].GetComponent<Renderer>().material && ledObj[i].GetComponent<Renderer>().material.HasProperty("_Color"))
                         originalColor[i] = ledObj[i].GetComponent<Renderer>().material.GetColor("_Color");

                      if (ledObj[i].GetComponent<Renderer>().material && ledObj[i].GetComponent<Renderer>().material.HasProperty("_TintColor"))
                         originalColor[i] = ledObj[i].GetComponent<Renderer>().material.GetColor("_TintColor");


                }
            }


        }


        for (int i = 0; i < pinObj.Length; i++)
            pinObj[i].tag = i.ToString();


        for (int i = 0; i < displayLedObj.Length; i++)
        {
            if (displayLedObj[i])
            {
                displayLedObj[i].tag = i.ToString();
                if (displayLedObj[i].GetComponent<Renderer>().material.HasProperty("_Color"))
                    originalDispColor[i] = displayLedObj[i].GetComponent<Renderer>().material.color;
            }
        }


        chipClass = "";

        if (chipType.ToUpper() == "NOT" || chipType.ToUpper() == "NAND" || chipType.ToUpper() == "NOR" || chipType.ToUpper() == "XNOR")
            logicOn = false;

        if (chipType.ToUpper() == "AND" || chipType.ToUpper() == "OR" || chipType.ToUpper() == "NOT" || chipType.ToUpper() == "NAND" ||
            chipType.ToUpper() == "NOR" || chipType.ToUpper() == "XOR" || chipType.ToUpper() == "XNOR" || chipType == "IF")
            chipClass = "primitive";


        if (chipType.ToUpper() == "TRIAND")
            chipClass = "triprimitive";

        if (chipType.ToUpper() == "BUFFER" || chipType.ToUpper() == "NOT" || chipType.ToUpper() == "SWITCH" || chipType.ToUpper() == "FLIPPER" ||
            chipType.ToUpper() == "LAMP" || chipType.ToUpper() == "RELAY" || chipType.ToUpper() == "SEQUENCER")
            chipClass = "gate";

        if (chipType.ToUpper() == "UPDN" || chipType.ToUpper() == "DIAL" || chipType.ToUpper() == "SLIDE" || chipType.ToUpper() == "METER" || chipType.ToUpper() == "FAN" || chipType.ToUpper() == "MOTOR")
            chipClass = "axis";

        if (chipType.ToUpper() == "DEKATRON" || chipType.ToUpper() == "UNDEKATRON" || chipType.ToUpper() == "NIXI")
            chipClass = "valve";

        if (chipType.ToUpper() == "TRISWITCH" || chipType.ToUpper() == "RELAY" || chipType.ToUpper() == "FOOTPEDAL")
            chipClass = "triswitch";

        if (chipType.ToUpper() == "THUMBWHEEL" || chipType.ToUpper() == "SCOREWHEEL")
            chipClass = "thumbwheel";

        if (chipType.ToUpper() == "DFF")
            chipClass = "flipflop";

        if (chipType.ToUpper() == "DIGDISPLAY")
            chipClass = "display";

        if (chipType.ToUpper() == "KEYBOARD")
            chipClass = "keyboard";

        if (chipType.ToUpper() == "TBUTTON")
            chipClass = "tbutton";

        if (chipType.ToUpper() == "STATUS")
            chipClass = "status";

        int pinSize = 0;

        if (chipClass == "gate")
            pinSize = 2;

        if (chipClass == "primitive")
            pinSize = 3;

        if (chipClass == "triprimitive")
            pinSize = 4;

        if (chipClass == "display" || chipClass == "tbutton")
            pinSize = 6;

        if (chipClass == "axis")
            pinSize = 7;

        if (chipClass == "keyboard")
            pinSize = 14;

        if (chipClass == "thumbwheel")
            pinSize = 10;

        if (chipClass == "scorewheel")
            pinSize = 10;

        if (chipClass == "valve")
            pinSize = 8;

        if (chipClass == "tbutton")
            pinSize = 6;

        if (chipClass == "status")
            pinSize = 8;


        if (chipClass == "flipflop" || chipClass == "triswitch" || chipClass == "footpedal")
            pinSize = 6;

        pinState = new bool[pinSize];
        ledOnTrig = new bool[ledObj.Length];
        statusLedOnTrig = new bool[pinState.Length];

      //  if (isButtonHold)
     //   {
            isButtonHoldTrig = new bool[ledObj.Length];
            isReleaseButtonHold = new bool[ledObj.Length];
     //   }

        digitPosValue = new int[objectGroup.Length];

        if (pinState.Length > 0)
            gateState = new bool[pinState.Length];

        dispLedOnTrig = new bool[displayLedObj.Length];

        if (displayLedObj.Length > 0)
            driverState = new bool[displayLedObj.Length];

        if (digitLedSegBit.Length > 0)
            setupDisplayMatrix();

        if (pinSize > 0)
            gateLen = gateState.Length;

        gateOnTrig = new bool[gateState.Length];


        if (gateLen < 1 || pinSize < 1)
        {
            Debug.LogWarning(gameObject.name + " has not been setup with valid ChipType and or Pin Objects");
            return;
        }

    }

    void setupDisplayMatrix()
    {
        digitLedSegBit[0, 0] = true;
        digitLedSegBit[0, 1] = true;
        digitLedSegBit[0, 2] = true;
        digitLedSegBit[0, 3] = true;
        digitLedSegBit[0, 4] = true;
        digitLedSegBit[0, 5] = true;
        digitLedSegBit[0, 6] = false;

        digitLedSegBit[1, 0] = false;
        digitLedSegBit[1, 1] = true;
        digitLedSegBit[1, 2] = true;
        digitLedSegBit[1, 3] = false;
        digitLedSegBit[1, 4] = false;
        digitLedSegBit[1, 5] = false;
        digitLedSegBit[1, 6] = false;

        digitLedSegBit[2, 0] = true;
        digitLedSegBit[2, 1] = true;
        digitLedSegBit[2, 2] = false;
        digitLedSegBit[2, 3] = true;
        digitLedSegBit[2, 4] = true;
        digitLedSegBit[2, 5] = false;
        digitLedSegBit[2, 6] = true;

        digitLedSegBit[3, 0] = true;
        digitLedSegBit[3, 1] = true;
        digitLedSegBit[3, 2] = true;
        digitLedSegBit[3, 3] = true;
        digitLedSegBit[3, 4] = false;
        digitLedSegBit[3, 5] = false;
        digitLedSegBit[3, 6] = true;

        digitLedSegBit[4, 0] = false;
        digitLedSegBit[4, 1] = true;
        digitLedSegBit[4, 2] = true;
        digitLedSegBit[4, 3] = false;
        digitLedSegBit[4, 4] = false;
        digitLedSegBit[4, 5] = true;
        digitLedSegBit[4, 6] = true;

        digitLedSegBit[5, 0] = true;
        digitLedSegBit[5, 1] = false;
        digitLedSegBit[5, 2] = true;
        digitLedSegBit[5, 3] = true;
        digitLedSegBit[5, 4] = false;
        digitLedSegBit[5, 5] = true;
        digitLedSegBit[5, 6] = true;

        digitLedSegBit[6, 0] = false;
        digitLedSegBit[6, 1] = false;
        digitLedSegBit[6, 2] = true;
        digitLedSegBit[6, 3] = true;
        digitLedSegBit[6, 4] = true;
        digitLedSegBit[6, 5] = true;
        digitLedSegBit[6, 6] = true;

        digitLedSegBit[7, 0] = true;
        digitLedSegBit[7, 1] = true;
        digitLedSegBit[7, 2] = true;
        digitLedSegBit[7, 3] = false;
        digitLedSegBit[7, 4] = false;
        digitLedSegBit[7, 5] = false;
        digitLedSegBit[7, 6] = false;

        digitLedSegBit[8, 0] = true;
        digitLedSegBit[8, 1] = true;
        digitLedSegBit[8, 2] = true;
        digitLedSegBit[8, 3] = true;
        digitLedSegBit[8, 4] = true;
        digitLedSegBit[8, 5] = true;
        digitLedSegBit[8, 6] = true;

        digitLedSegBit[9, 0] = true;
        digitLedSegBit[9, 1] = true;
        digitLedSegBit[9, 2] = true;
        digitLedSegBit[9, 3] = false;
        digitLedSegBit[9, 4] = false;
        digitLedSegBit[9, 5] = true;
        digitLedSegBit[9, 6] = true;



    }

    public int clickPart(GameObject hitObject)
    {

        if (hudCamera)
        {
            currentClickPin = -1;
            Ray ray = hudCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool verifyLbHit = true;
            int pinInt = -1;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject)
                {
                    GameObject currentClickObj = hit.transform.gameObject;


                    if (currentClickObj.GetComponent<UniLogicChip>())
                    {
                        int debugLev = currentClickObj.GetComponent<UniLogicChip>().debugLevel;
                        verifyLbHit = false;

                        for (int i = 0; i < hitObject.GetComponent<UniLogicChip>().pinObj.Length; i++)
                            if (hit.collider.transform.name == hitObject.GetComponent<UniLogicChip>().pinObj[i].transform.name)
                                verifyLbHit = true;

                        for (int i = 0; i < hitObject.GetComponent<UniLogicChip>().ledObj.Length; i++)
                            if (hit.collider.transform.name == hitObject.GetComponent<UniLogicChip>().ledObj[i].transform.name)
                                verifyLbHit = true;
                    }


                    Collider[] hitPin = currentClickObj.GetComponents<Collider>();

                    if (hitPin != null && verifyLbHit)
                    {
                        if (debugLevel > 0)
                            Debug.Log("Mouse click on object <color=blue>" + gameObject.name + "</color> Target object = <color=blue>" + hit.collider.transform.name + "</color>");
                        pinInt = -1;
                        if (!hit.collider.isTrigger && hit.collider.tag != "")
                        {
                            pinInt = int.Parse(hit.collider.tag);
                            return pinInt;
                        }
                        else

                            Debug.LogWarning(gameObject.name + " no valid pin/buton objects found, or not configured");
                        return -1;
                    }



                }
                else
                {
                    Debug.LogWarning(gameObject.name + " uniLogicChip.cs script is not attched to object. Or chipType is not defined");
                }
            }


        }

        return -1;
    }

    static public Bounds RecursiveMeshBB(GameObject go)
    {
        MeshFilter[] mfs = go.GetComponentsInChildren<MeshFilter>();

        if (mfs.Length > 0)
        {
            Bounds b = mfs[0].mesh.bounds;
            for (int i = 1; i < mfs.Length; i++)
            {
                b.Encapsulate(mfs[i].mesh.bounds);
            }
            return b;
        }
        else
            return new Bounds();
    }

    void addGameTag(string _tagText)
    {

#if UNITY_EDITOR

        SerializedObject tagEditor = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagEditor.FindProperty("tags");

        string chkTag = _tagText;
        bool tagExists = false;

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty tString = tagsProp.GetArrayElementAtIndex(i);
            if (tString.stringValue.Equals(chkTag))
            {
                tagExists = true;
                break;
            }
        }

        if (!tagExists)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty tWrite = tagsProp.GetArrayElementAtIndex(0);
            tWrite.stringValue = chkTag;
            tagEditor.ApplyModifiedPropertiesWithoutUndo();
        }




#endif
    }

    #endregion

    /// <summary>
    ///  sim faults ///////////////////////
    /// </summary>
    void scanSimFaults()
    {

        if (isWindDown || !isSystemReady || !GetComponent<UniLogicEffects>())
            return;

        float testParam = -1;

        if (errOnClock)
            testParam = clockPulseWidth;

        if (errOnStepVal)
            testParam = switchSteps;

        if (errOnCount)
            testParam = counterClock;
            

        if (errOnPosition)
            testParam = selCurPos;

        if (errAtOnState)
        {
            if (outputState)
                testParam = 1;
            else
                testParam = 0;

        }


        if (testParam == -1)
            return;

        if (!errAtOnState)
        {
            if (softHighErrLimit > 0)
            {

             
                if (!isHighSoftErrTrig && testParam > softHighErrLimit)
                {
                    alarmlevel = 1;
                    isHighSoftErrTrig = true;

                    isHighHardErrTrig = false;
                    isLowSoftErrTrig = false;
                    isLowHardErrTrig = false;


                    GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 1);
                    setExtData(1, true, 3);
                    runEffectsOnTrig = true;
                }
                else
                {
                    if (isHighSoftErrTrig && testParam < softHighErrLimit)
                    {
                        GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 0);
                        setExtData(0, true, 3);
                        isHighSoftErrTrig = false;
                        runEffectsOnTrig = false;


                    }

                }
            }

            if (softLowErrLimit > 0)
            {


                if (!isLowSoftErrTrig && testParam < softLowErrLimit)
                {
                    alarmlevel = 1;
                    isLowSoftErrTrig = true;
                    runEffectsOnTrig = true;

                    isHighHardErrTrig = false;
                    isHighSoftErrTrig = false;
                    isLowHardErrTrig = false;


                    GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 1);
                    setExtData(1, true, 3);

                }
                else
                {
                    if (isLowSoftErrTrig && testParam > softLowErrLimit)
                    {
                        GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 0);
                        setExtData(0, true, 3);
                        isLowSoftErrTrig = false;
                        runEffectsOnTrig = false;

                    }

                }
            }

            if (hardHighErrLimit > 0)
            {
                if (!isHighHardErrTrig && testParam > hardHighErrLimit)
                {
                    alarmlevel = 2;
                    isHighHardErrTrig = true;
                    runEffectsOnTrig = true;

                    isLowHardErrTrig = false;
                    isHighSoftErrTrig = false;
                    isLowHardErrTrig = false;


                    GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 2);
                    setExtData(2, true, 3);
                }
                else
                {
                    if (isHighHardErrTrig && testParam < hardHighErrLimit)
                    {
                        GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 0);
                        alarmlevel = 0;
                        isHighHardErrTrig = false;
                        runEffectsOnTrig = false;
                        setExtData(0, true, 3);
                    }

                }
            }

            if (hardLowErrLimit > 0)
            {
                if (!isLowHardErrTrig && testParam < hardLowErrLimit)
                {
                    alarmlevel = 2;
                    isLowHardErrTrig = true;
                    isLowHardErrTrig = false;
                    isHighSoftErrTrig = false;
                    isHighHardErrTrig = false;

                    runEffectsOnTrig = true;
                    GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 2);
                    setExtData(2, true, 3);
                }
                else
                {
                    if (isLowHardErrTrig && testParam > hardHighErrLimit)
                    {
                        GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 0);
                        alarmlevel = 0;
                        isLowHardErrTrig = false;

                        isLowHardErrTrig = false;
                        isHighSoftErrTrig = false;
                        isHighHardErrTrig = false;
                        runEffectsOnTrig = false;
                        setExtData(0, true, 3);

                    }

                }


            }


        }


        if (errAtOnState)
        {
            if (gateState[0] && !isHighHardErrTrig)
            {
                alarmlevel = 2;
                GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 2);
                isHighHardErrTrig = true;
                setExtData(2, true, 3);
                runEffectsOnTrig = true;
            }
            else
            {
                if (testParam == 0 && alarmlevel > 0)
                {
                    GetComponent<UniLogicEffects>().ToggleFailureAnim(true, 0);
                    isHighHardErrTrig = false;
                    runEffectsOnTrig = false;
                    alarmlevel = 0;
                }
            }



        }

        if (!isLowSoftErrTrig && !isHighSoftErrTrig && !isLowHardErrTrig && !isHighHardErrTrig && alarmlevel > 0)
        {
            alarmlevel = 0;
            GetComponent<UniLogicEffects>().ToggleFailureAnim(false, 0);
         //   setExtData(0, true, 3);
            GetComponent<UniLogicEffects>().changePitch(0);
            motorTimer = motorTimerMax;
        }


        if (!errAtOnState && !errOnClock && !errOnPosition && !errOnStepVal)
        {
            isLowSoftErrTrig = false; isHighSoftErrTrig = false; isLowHardErrTrig = false; isHighHardErrTrig = false;
        }

    }

    void alignPins()
    {
        //relign output to gate changes
        for (int i = 0; i < pinState.Length; i++)
        {
            if (pinState[i] != gateState[i])
            {
                pinState[i] = gateState[i];

            }

        }

    }
    
    public void resetCam()
    {

        if (GetComponent<UniLogicEffects>())
        {
            GetComponent<UniLogicEffects>().resetObserver();

        }

    }

    void logEntry(GameObject _object, string _logMsg, bool _state)
    {

        string _color;

        if (_state == true)
            _color = "<color=green>";
        else
            _color = "<color=red>";

        Debug.Log("<color=blue>" + _object + "</color> - " + _logMsg + _color + " = " + _state + "</color>");

    }

    void populateMoveTypes()
    {
        moveTypeIdx.Add(0, moveType.rotLR);
        moveTypeIdx.Add(1, moveType.rotUD);
        moveTypeIdx.Add(2, moveType.rotLeft);
        moveTypeIdx.Add(3, moveType.pushDown);
        moveTypeIdx.Add(4, moveType.pushLeft);
        moveTypeIdx.Add(5, moveType.rotDown);
        moveTypeIdx.Add(6, moveType.rotFwd);
    }

    void floatObj()
    {
        transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        transform.position = tempPos;

    }


    [System.Serializable]
    public struct MoveType
    {
        public int rotLR, rotUD, rotLeft, pushDown, pushLeft, rotDown, rotFwd;
    }

    
    IEnumerator waitCoroutine()
    {
    
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1);

    }

}


public static class ExtensionMethod
{
    public static void Reset(this LineRenderer lr)
    {
        lr.positionCount = 0;
    }
}








