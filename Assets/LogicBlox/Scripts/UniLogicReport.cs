using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class UniLogicReport : MonoBehaviour
{
    public string reportName;
  
    [Header("Chip Report")]

    public bool isGetChipParams;
    public bool isGetLinks;
    public bool isGetClockInfo;
    public bool isGetButInfo;
    public bool isGetAlertInfo;
    public bool isGetAll;



    [Header("Effects Report")]
    public bool isGetEffectsParams;

    [Header("Control")]
    public bool isRunReport;
    public string reportMsg;
       

    string reportTxt,reportHeader;
    string chipReportPath = "Assets/logicblox/ChipParamReport.txt";
    string effectsReportPath = "Assets/logicblox/EffectsParamReport.txt";

    // Start is called before the first frame update
    void Start()
    {
        isRunReport = false;
        reportMsg="Click Run to start";
      
      //  deleteReport(chipReportPath);
      // deleteReport(effectsReportPath);
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunReport)
        {
            isRunReport = false;

            reportMsg = "Running Report";
            getObjectData(isGetChipParams, isGetLinks,isGetButInfo,isGetAlertInfo,isGetEffectsParams);

            Debug.Log("run report");

            isGetChipParams =true;
            isGetLinks = true;
            isGetClockInfo = true;
            isGetButInfo = true;
            isGetAlertInfo = true;
            isGetEffectsParams = true;
            isGetAll = true;
            reportMsg = "Report complete";

        }
    }

    void FixedUpdate()
    {

      
    }


    private void OnButtonClicked()
    {
        Debug.Log("Clicked!");
    }


    void getObjectData(bool _getChip, bool _getLinks, bool _getButInfo,bool _getAlerts, bool _getEffects)
    {
        UniLogicChip _objChip;
        UniLogicEffects _objEffects;
        string[] dLogTxt = new string[8];
        string[] dLogHeader = new string[8];
        string _depName = "None";
        string linkpinTxt = "";
        dLogTxt[0] = "";

        string objPath="";


        if (_getChip || isGetAll)
        {
            dLogHeader[0] = "\n" + System.DateTime.Now.ToShortDateString() + " - " + System.DateTime.Now.ToShortTimeString() + " - " + reportName + " - Logic Blox CHIP Parameter Report - Granby Games\n";
            dLogHeader[0] = dLogHeader[0] + "Name,Links(pins),Parent,Type,Default State,IsMomemtary,MomDuration,IsLatch,DependantObj,SubFunction,FuncParam,SendVal,CircuitGroup,Encountergroup";


            if (isGetClockInfo || isGetAll)
               dLogHeader[0] = dLogHeader[0] + ",Isclock,ClockPulseWide,MaxClockRepeats,ClockPrecision,IsMaster,IsAutoStart,IsCountDown,CountDownDuration,IsCounter,CounterMax";
       
            if (isGetButInfo || isGetAll)
                    dLogHeader[0] = dLogHeader[0] + ",AnminType,RotType,SwitchSteps,MaxSwitchSteps,MinSwitchSteps,IsIllumSwitch,IsButtonHold,IsUnique,IsOneShot,isLookAtPlayer,defaultDigVal,DefaultPos,SelMaxPos,SelMinPos,WindDownTimerMax";


            if (isGetAlertInfo || isGetAll)
                dLogHeader[0] = dLogHeader[0] + ",SoftHighErrLimit,HardLowErrLimit,HardHighErrorLimit,HardLowErrLimit,ErrOnClock,ErrOnStepVal,ErrOnCount,ErrOnPosition,ErrOnState";



            foreach (var obj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
            {
                _objChip = obj.GetComponent<UniLogicChip>();

                if (_objChip.dependantObj && _objChip.dependantObj != null)
                    _depName = _objChip.dependantObj.name;

                if (_objChip.outputLinkObj.Length > 0)
                    linkpinTxt = "Links(pins)";
                else
                    linkpinTxt = "none";

                if (_objChip.transform.parent != null)
                {
                    objPath = _objChip.transform.parent.name;


                    if (_objChip.transform.parent.parent != null)
                    {
                        objPath = objPath + "." + _objChip.transform.parent.parent.name;


                        if (_objChip.transform.parent.transform.parent.parent != null)
                            objPath = objPath + "." + _objChip.transform.parent.transform.parent.parent.name;
                    }
                }


                dLogTxt[0] = dLogTxt[0] + "\n" + _objChip.name + "," + linkpinTxt + "," + objPath + "," + _objChip.chipType + "," + _objChip.defaultState + "," +
                    _objChip.isMomentary + "," + _objChip.momDuration + "," + _objChip.isLatch + "," + _depName + "," + _objChip.subFunction + "," + _objChip.funcParam + "," + _objChip.sendVal + "," +
                    _objChip.circuitGroup + "," + _objChip.encounterGroup;


                if (isGetClockInfo || isGetAll)
                {
                    dLogTxt[0] = dLogTxt[0] + "," + _objChip.isClock + "," + _objChip.clockPulseWidth + "," + _objChip.maxClockRepeats + "," + _objChip.clockPrecision + "," +
                       _objChip.isMaster + "," + _objChip.isAutoStart + "," + _objChip.isCountDown + "," + _objChip.countDownDuration + "," + _objChip.isCounter + "," + _objChip.counterMax;

                }
                if (_getButInfo || isGetAll)
                {
                    dLogTxt[0] = dLogTxt[0] + "," + _objChip.animType + "," + _objChip.rotType + "," + _objChip.switchSteps + "," + _objChip.maxSwitchSteps + "," + _objChip.minSwitchSteps + ","
                           + _objChip.isIllumSwitch + "," + _objChip.isButtonHold + "," + _objChip.isUnique + "," + _objChip.isOneshot + "," + _objChip.isLookAtPlayer + "," + _objChip.defaultDigVal + "," +
                          _objChip.defaultPos + "," + _objChip.selMaxPos + "," + _objChip.selMinPos + "," + _objChip.windDownTimerMax;
                }


                if (_getAlerts || isGetAll)
                {
                    dLogTxt[0] = dLogTxt[0] + "," + _objChip.softHighErrLimit + "," + _objChip.hardLowErrLimit + "," + _objChip.hardHighErrLimit + "," + _objChip.hardLowErrLimit + "," + _objChip.errOnClock + "," +
                   _objChip.errOnStepVal + ", " + _objChip.errOnCount + "," + _objChip.errOnPosition + "," + _objChip.errAtOnState;


                }


                // link objects
                if (_objChip.outputLinkObj.Length > 0)
                {

                    dLogTxt[1] = "";

                    for (int i = 0; i < _objChip.outputLinkObj.Length; i++)
                        if (_objChip.outputLinkObj[i])
                            if (_objChip.outputLinkPin.Length > 0 && i < _objChip.outputLinkPin.Length)

                                dLogTxt[1] = dLogTxt[1] + "   OutputLinkObj " + i + " >>," + _objChip.outputLinkObj[i].name + " - Pin(" + _objChip.outputLinkPin[i] + ")\n";

                    if (isGetLinks || isGetAll)
                        dLogTxt[0] = dLogTxt[0] + "\n" + dLogTxt[1];

                }


                // link2 objects
                if (_objChip.output2LinkObj.Length > 0)
                {

                    dLogTxt[2] = "";

                    for (int i = 0; i < _objChip.output2LinkObj.Length; i++)
                        if (_objChip.output2LinkObj[i])
                            if (_objChip.output2LinkPin.Length > 0 && i < _objChip.output2LinkPin.Length)
                                dLogTxt[2] = dLogTxt[2] + "      Output2LinkObj " + i + " >>," + _objChip.output2LinkObj[i].name + " - Pin(" + _objChip.output2LinkPin[i] + ")\n";

                    if (isGetLinks || isGetAll)
                        dLogTxt[0] = dLogTxt[0] + dLogTxt[2];

                }

                //tac object
                if (_objChip.tacObject.Length > 0)
                {

                    dLogTxt[3] = "";

                    for (int i = 0; i < _objChip.tacObject.Length; i++)
                    {

                        if (_objChip.tacObject[i])
                        {
                            dLogTxt[3] = dLogTxt[3] + "         TacObject " + i + " >>," + _objChip.tacObject[i].name + " - Pin(0)\n";
                        }
                    }

                    if (isGetLinks || isGetAll)
                        dLogTxt[0] = dLogTxt[0] + dLogTxt[3];

                }

                               
            }

            reportTxt = reportTxt + dLogHeader[0] + "\n" + dLogTxt[0];

            WriteString(reportTxt, chipReportPath);
        }
               
        if (_getEffects || isGetAll)
        {
            dLogHeader[0] = "\n" + System.DateTime.Now.ToShortDateString() + " - " + System.DateTime.Now.ToShortTimeString() + " - " + reportName + " - Logic Blox EFFECTS Parameter Report - Granby Games\n";
            dLogHeader[0] = dLogHeader[0] + "Name,IsPreventBargeIn,ClickOnAudio,ClickOffAdio,OutputOnAudio,OutputOffAdio,IsAudioContinuous,isOffStateDucking,ClipVolume,IsReaminOn,ToggleOnObject,ToggleOnObjectDef,ToggleOffObject,ToggleOffObjectDef";
            dLogHeader[0] = dLogHeader[0] + ",AlertOnSoftFail,AlertOnHardFail,ToggleNormalObj,ToggleSoftFailObj,ToggleHardFailObj,PlaySoftFailAudio,PlayHardFailAudio,isSilenceAlert,SoftFailAudioVol,HardFailAudioVol,AlertAudioVol";
            dLogHeader[0] = dLogHeader[0] + ",EncounterGroup,IsEncounterActive,NextEncounterObj,NextEncounterPin,EncounterDelay,IsGotoObserver,ObserverCam,CamChangeDelay,CamReturnDelay";
            dLogHeader[0] = dLogHeader[0] + ",AnminObject,Animation,StartAnim,AuxLevelObj1,AuxLevelObj2,AuxLevelObj3,IsShakeOnSoftErr,IsDestructOnHardErr,ShakeObject";

            dLogTxt[0] = "";
            dLogTxt[1] = "";
            dLogTxt[2] = "";
            dLogTxt[3] = "";
            reportTxt = "";

            foreach (var obj in FindObjectsOfType(typeof(UniLogicEffects)) as UniLogicEffects[])
            {
                _objEffects = obj.GetComponent<UniLogicEffects>();

               

                string CONA_TXT = "null";
                string COFFA_TXT = "null";
                string OONA_TXT = "null";
                string OOFFA_TXT = "null";
                string TOO_TXT = "null";
                string TOF_TXT = "null";
                string TNO_TXT = "null";
                string TFO_TXT = "null";
                string THFO_TXT = "null";
                string TSFO_TXT = "null";
                string NEO_TXT = "null";
                string OC_TXT = "null";
                string AO_TXT = "null";
                string ANO_TXT = "null";
                string AN_TXT = "null";
                string AUXO1_TXT = "null";
                string AUXO2_TXT = "null";
                string AUXO3_TXT = "null";
                string SO_TXT = "null";


                if (_objEffects.clickOnAudio)
                    CONA_TXT = _objEffects.clickOnAudio.name;
                if (_objEffects.clickOffAudio)
                    COFFA_TXT = _objEffects.clickOffAudio.name;
                if (_objEffects.outputOnAudio)
                    OONA_TXT = _objEffects.outputOnAudio.name;
                if (_objEffects.outputOffAudio)
                    OOFFA_TXT = _objEffects.outputOffAudio.name;

                if (_objEffects.toggleOnObject.Length > 0 && _objEffects.toggleOnObject[0] != null)
                    TOO_TXT = _objEffects.toggleOnObject[0].name;

                if (_objEffects.toggleOffObject.Length > 0 && _objEffects.toggleOffObject[0] != null)
                    TOF_TXT = _objEffects.toggleOffObject[0].name;

                if (_objEffects.toggleNominalObj)
                    TNO_TXT = _objEffects.toggleNominalObj.name;

                if (_objEffects.toggleSoftFailObj)
                    TFO_TXT = _objEffects.toggleSoftFailObj.name;

                if (_objEffects.toggleHardFailObj)
                    THFO_TXT = _objEffects.toggleHardFailObj.name;

                if (_objEffects.alertAudio != null)
                    AO_TXT = _objEffects.alertAudio.name;

                if (_objEffects.nextEncounterObj)
                    NEO_TXT = _objEffects.nextEncounterObj.name;

                if (_objEffects.observerCam)
                    OC_TXT = _objEffects.observerCam.name;

                if (_objEffects.animObject)
                    ANO_TXT = _objEffects.animObject.name;

                if (_objEffects.animation)
                    AN_TXT = _objEffects.animation.name;

                if (_objEffects.auxLevelObj1)
                    AUXO1_TXT = _objEffects.auxLevelObj1.name;

                if (_objEffects.auxLevelObj2)
                    AUXO2_TXT = _objEffects.auxLevelObj2.name;

                if (_objEffects.auxLevelObj3)
                    AUXO3_TXT = _objEffects.auxLevelObj3.name;

                if (_objEffects.shakeObject)
                    SO_TXT = _objEffects.shakeObject.name;


                dLogTxt[0] = dLogTxt[0] + "\n" + _objEffects.name + "," + _objEffects.isPreventBargeIn + "," + CONA_TXT + "," +
                   COFFA_TXT + "," + OONA_TXT + "," + OOFFA_TXT + "," + _objEffects.isAudioContinuous + "," + _objEffects.isOffStateDucking + "," + _objEffects.clipVolume + "," + _objEffects.isRemainOn + "," +
                    TOO_TXT + "," + _objEffects.toggleOnObjDefState + "," + TOF_TXT + "," + _objEffects.toggleOffObjDefState;

                dLogTxt[0] = dLogTxt[0] + "," + _objEffects.alertOnSoftFail + "," + _objEffects.alertOnHardFail + "," + TNO_TXT + "," + TFO_TXT + "," +
                   THFO_TXT + "," + _objEffects.playSoftFailAudio + "," + _objEffects.playHardfailAudio + "," + _objEffects.isSilenceAlert + "," + _objEffects.SoftFailVolume + "," + _objEffects.hardFailVolume + "," + _objEffects.alertVolume ;

                dLogTxt[0] = dLogTxt[0] + "," + _objEffects.encounterGroup + "," + _objEffects.isEncounterActive + "," + NEO_TXT + "," + _objEffects.nextEncounterPin + "," + _objEffects.encounterDelay + "," +
                       _objEffects.isGotoObserver + ","  + OC_TXT + "," + _objEffects.camChangeDelay + "," + _objEffects.camReturnDelay + "," + ANO_TXT + "," + AN_TXT + "," +
                      _objEffects.startAnim + "," + AUXO1_TXT + "," + AUXO2_TXT + "," + AUXO3_TXT + "," + _objEffects.isShakeOnSoftErr + "," + _objEffects.isDestructOnHardErr + "," + SO_TXT;



                if (_objEffects.toggleOnObject.Length > 0)
                {

                    dLogTxt[1] = "";
                    string tEnd = "";
                    int tLen = _objEffects.toggleOnObject.Length;
                    for (int i = 0; i < _objEffects.toggleOnObject.Length; i++)
                    {

                        if (_objEffects.toggleOnObject[i])
                        {
                            if (i < tLen-2)
                                tEnd = "\n";
                            else
                                tEnd = "";

                            

                            dLogTxt[1] = dLogTxt[1] + ",,,,,,,,,," + _objEffects.toggleOnObject[i].name + tEnd;

                        }
                    }

                    dLogTxt[0] = dLogTxt[0] + dLogTxt[1];

                    if (_objEffects.toggleOffObject.Length > 0)
                    {

                        dLogTxt[2] = "";

                        for (int i = 0; i < _objEffects.toggleOffObject.Length; i++)
                        {

                            if (_objEffects.toggleOffObject[i])
                            {

                                dLogTxt[2] = dLogTxt[2] + ",,,,,,,,,,,," + _objEffects.toggleOffObject[i].name + "\n";
                            }
                        }


                        dLogTxt[0] = dLogTxt[0] + dLogTxt[2];

                    }


                }
            }


            reportTxt = reportTxt + dLogHeader[0] + "\n" + dLogTxt[0];
            WriteString(reportTxt, effectsReportPath);
        }
                    
            

    }


    static void WriteString(string _report_txt, string _path)
    {
    
        StreamWriter writer = new StreamWriter(_path,false);
        writer.WriteLine(_report_txt);
        writer.Close();
        writer.Dispose();

    }



    static void deleteReport( string _path)
    {

        if (File.Exists(_path))
        {
            File.Delete(_path);
        }


    }

    static void ReadString()
    {
        string path = "Assets/Resources/test.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
}
