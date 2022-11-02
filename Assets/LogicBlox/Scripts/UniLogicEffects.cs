using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//<<<Logic Blox effects v 4.3 - Universal script for prop effects
// Developed by Mike Hogan (2018) - Granby Games - mhogan@remhouse.com
// Update Feb 2019 - fixed unclick sound bug
// Updted April 10 - LB Ver 3 compatability

// Free for personal use, requires permission to resell.

[RequireComponent(typeof(AudioSource))]


public class UniLogicEffects : MonoBehaviour
{

    [Header("Sounds")]

    public bool isPreventBargeIn;
    public AudioClip clickOnAudio;
    public AudioClip clickOffAudio;
    public AudioClip outputOnAudio;
    public AudioClip outputOffAudio;
   
    public bool isAudioContinuous;
    public bool isOffStateDucking;
    public float clipVolume = .3f;

    [Header("GameObject(s) Enable")]
    public bool isRemainOn;
    public GameObject[] toggleOnObject;
    bool[] toggleOnObjTrig;

    public bool toggleOnObjDefState;
  
    [Header("GameObject(s) Disable")]
    public GameObject[] toggleOffObject;
    public bool toggleOffObjDefState;
    bool []toggleOffObjTrig;
    
  
    [HideInInspector]
    public float[] objDelay;


    [Header("Alert status Objects")]

    public bool alertOnSoftFail;
    public bool alertOnHardFail;

    public GameObject toggleNominalObj;
    public GameObject toggleSoftFailObj;
    public GameObject toggleHardFailObj;
        
 
    public bool playSoftFailAudio;
    public bool playHardfailAudio;
    public bool isSilenceAlert;

    public AudioClip softFailAudio;
    public AudioClip hardFailAudio;
    public AudioClip alertAudio;
    public float SoftFailVolume;
    public float hardFailVolume;
    public float alertVolume;

    bool errorDetectedTrig;
    bool floatObjTrig;
     
    GameObject floatObject;

    bool isSoftErrTrig,isHardErrTrig;

    [Header("Encounter Settings")]
    public string encounterGroup;
    public bool isEncounterActive;
    public GameObject nextEncounterObj;

    public int nextEncounterPin;
    public float encounterDelay;
    bool encounterDelayTrig;
    float encounterDelayTimer;

    public bool isGotoObserver;
    bool gotoObserverTrig;

    public GameObject observerCam;
    public GameObject returnToCam;
  
    public float camChangeDelay;
    public float camReturnDelay;
   
    bool camDelayTrig;
    bool camDelayEndTrig;
    float camDelayTimer;
    bool isCamSwitchedTrig;
    bool camReturnTrig;
    bool camReturnEndTrig;
    float camReturnTimer;
    GameObject alarmObject;

    [Header("Animations")]
    public GameObject animObject;
    public AnimationClip animation;
    Animation anim;
    
    public bool startAnim;
    public bool triggerAnim;


    [Header("Switch Indicator Lamps")]
    [HideInInspector]
    public GameObject onStateIndObj;
    [HideInInspector]
    public GameObject offStateIndObj;


    public GameObject auxLevelObj1;
    public GameObject auxLevelObj2;
    public GameObject auxLevelObj3;


    public bool isShakeOnSoftErr;
    public bool isDestructOnHardErr;
    public GameObject shakeObject;

    Vector3 propVect;
    Quaternion propRot;
  

    [HideInInspector]
    AudioSource effectsAudio = new AudioSource();


    float rotSpeedFloat;
    float ampFloat;
    float freqBounceFloat;
    Vector3 posOffsetFloat = new Vector3();
    Vector3 tempPosFloat = new Vector3();



    Vector3 shakeOffsetFloat = new Vector3();
    Vector3 shakePosFloat = new Vector3();

    float shakeRotSpeedFloat = 3f;
    float shakeAmpFloat = .005f;
    float shakeFreqBounceFloat = 12f;

    bool isSystemReady = false;

    public int debugLevel;

    void Start()
    {              
        initBlox();
    }

    void Update()
    {
        if(floatObjTrig)
            runFloatObj(floatObject);


        if (isShakeOnSoftErr && shakeObject && errorDetectedTrig)
            runShakeObj(shakeObject);

        if (animObject && startAnim && animation!=null)
        {
            triggerAnim = true;
            startAnim = false;
            animObject.GetComponent<Animation>().Play();
        }

    }

    void FixedUpdate()
    {
        effectsTimer();

    }

    public void changePitch(float incVal)
    {

        if (effectsAudio != null)
        {
            if (incVal == 0)
            {
                effectsAudio.pitch = effectsAudio.pitch = 1;
                return;

            }
            if (incVal < -3)
                incVal = -1;

            if (incVal > 3)
                incVal = 3;

            effectsAudio.pitch = effectsAudio.pitch + incVal;
        }
    }

    public void ToggleFailureAnim(bool _state, int _type)
    {

        if (!isSystemReady)
            return;

        if (_type == 0)
        {
         if (debugLevel > 1)
                Debug.Log("<color=blue>" + gameObject.name + "</color> UniLogicEffects state = nominal ");

            isSoftErrTrig = false;
            isHardErrTrig = false;

            if (toggleSoftFailObj)
                toggleSoftFailObj.SetActive(false);

            if (toggleHardFailObj)
                toggleHardFailObj.SetActive(false);

            if (toggleNominalObj)
            {
                toggleNominalObj.SetActive(true);
                posOffsetFloat = toggleNominalObj.transform.position;
            }


            if (auxLevelObj1)
                auxLevelObj1.SetActive(true);

            if (auxLevelObj2)
                auxLevelObj2.SetActive(false);

            if (auxLevelObj3)
                auxLevelObj3.SetActive(false);


            GetComponent<UniLogicChip>().setExtData(0, true, 3);

            if (isGotoObserver && isCamSwitchedTrig && observerCam)
             {
                if (camReturnDelay > 0)
                {
                    camReturnTrig = true;
                }
                else
                {
                    observerCam.SetActive(false);
                    switchCam(returnToCam);
                    returnToCam.SetActive(true);
                  
                    isCamSwitchedTrig = false;

                    //  returnToCam.transform.parent.SetPositionAndRotation(propVect, propRot);
                 //   if (debugLevel > 1)
                        Debug.Log("<color=blue>" + gameObject.name + "</color> Switch to player cam  ");

                }

            }
            else
            {
                if (isGotoObserver)
                {
                    propRot = transform.parent.rotation;
                    propVect = transform.parent.position;
                    resetObserver();
                }
                else
                {
                    if (errorDetectedTrig)
                    {

                        if (toggleSoftFailObj)
                            toggleSoftFailObj.SetActive(false);

                        if (toggleHardFailObj)
                            toggleHardFailObj.SetActive(false);

                        if (toggleNominalObj)
                            toggleNominalObj.SetActive(true);


                        errorDetectedTrig = false;

                        if (isEncounterActive)
                            runNextEncounter();
                    }
                }

            }


        }

        if (_type == 1)
        {
        
            isSoftErrTrig = true;
            isHardErrTrig = false;

            errorDetectedTrig = true;

            if (debugLevel > 1)
                Debug.Log("<color=blue>" + gameObject.name + "</color> UniLogicEffects state = soft error. AlertEnabled= " + alertOnSoftFail);

            if (auxLevelObj1)
                auxLevelObj1.SetActive(false);

            if (auxLevelObj2)
                auxLevelObj2.SetActive(true);

            if (auxLevelObj3)
                auxLevelObj3.SetActive(false);

            if (alertOnSoftFail)
            {

                if (toggleNominalObj)
                    toggleNominalObj.SetActive(false);

                if (toggleHardFailObj)
                    toggleHardFailObj.SetActive(false);

                if (toggleSoftFailObj)
                {
                    toggleSoftFailObj.SetActive(true);
                    posOffsetFloat = toggleSoftFailObj.transform.position;

                }

                if (playSoftFailAudio && effectsAudio && softFailAudio )
                {
                    effectsAudio.PlayOneShot(softFailAudio, SoftFailVolume);
                }

            }


        }
                
        if (_type == 2)
        {

            isHardErrTrig = true;
            isSoftErrTrig = false;
            errorDetectedTrig = true;

            if (debugLevel > 1)
                Debug.Log("<color=blue>" + gameObject.name + "</color> UniLogicEffects state = hard error.  AlertEnabled= " + alertOnHardFail);

            if (auxLevelObj1)
                auxLevelObj1.SetActive(false);
            if (auxLevelObj2)
                auxLevelObj2.SetActive(false);
            if (auxLevelObj3)
                auxLevelObj3.SetActive(true);



            if (alertOnHardFail)
            {
      
                if (toggleNominalObj)
                    toggleNominalObj.SetActive(false);

                if (toggleSoftFailObj)
                    toggleSoftFailObj.SetActive(false);


                if (toggleHardFailObj && !toggleHardFailObj.activeSelf)
                {

                    toggleHardFailObj.SetActive(true);
                    posOffsetFloat = toggleHardFailObj.transform.position;
                    floatObject = toggleHardFailObj;
                    floatObjTrig = true;

                   
                    

                }

                if (effectsAudio && alertAudio && !isSilenceAlert)
                {
                    effectsAudio.PlayOneShot(alertAudio, alertVolume);
                }



                if (playHardfailAudio && effectsAudio && hardFailAudio)
                {
                    effectsAudio.PlayOneShot(hardFailAudio, hardFailVolume);
                }


                if (isGotoObserver && !isCamSwitchedTrig && observerCam)
                {

                    //    propRot = transform.parent.rotation;
                    //    propVect = transform.parent.position;

                    if (camChangeDelay > 0)
                    {
                        camDelayTrig = true;
                    }
                    else
                    {
                        returnToCam.SetActive(false);
                        switchCam(observerCam);
                        observerCam.SetActive(true);
                     
                        isCamSwitchedTrig = true;
                        //       if (debugLevel > 1)
                        Debug.Log("<color=blue>" + gameObject.name + "</color> Switch to observer cam ");


                    }


                }

            }
        }

     

    }


    public void resetObjects()
    {
        if(debugLevel>2)
           Debug.Log("<color=blue>" + gameObject.name + "</color> resets");

        isSystemReady = false;
        changePitch(0);

        effectsAudio.loop = false;
        effectsAudio.Stop();
        effectsAudio.clip = null;

        isHardErrTrig = false;
        isSoftErrTrig = false;
        errorDetectedTrig = false;
        floatObjTrig = false;

        if (alarmObject)
            alarmObject.SetActive(false);


        for (int i = 0; i < toggleOnObject.Length; i++)
            if (toggleOnObject[i])
                toggleOnObject[i].SetActive(false);


        for (int i = 0; i < toggleOffObject.Length; i++)

            if (toggleOffObject[i])
                toggleOffObject[i].SetActive(false);


        if (toggleNominalObj)
            toggleNominalObj.SetActive(false);

        if (toggleSoftFailObj)
            toggleSoftFailObj.SetActive(false);

        if (toggleHardFailObj)
            toggleHardFailObj.SetActive(false);

        if (auxLevelObj1)
            auxLevelObj1.SetActive(false);

        if (auxLevelObj2)
            auxLevelObj2.SetActive(false);

        if (auxLevelObj3)
            auxLevelObj3.SetActive(false);

        if (nextEncounterObj)
            nextEncounterObj.SetActive(false);

        isSystemReady = true;

    }


    public void resetObserver()
    {

        errorDetectedTrig = false;
        isHardErrTrig = false;
        isSoftErrTrig = false;
        isGotoObserver = false;

        gotoObserverTrig = false;
        camDelayTrig = false;
        camDelayTimer = 0;
        camDelayEndTrig = false;
        camReturnTrig = false;
        camReturnTimer = 0;
        camReturnEndTrig = false;
        isCamSwitchedTrig = false;

        switchCam(returnToCam);
    }

    void effectsTimer()
    {

        if (!isSystemReady)
            return;
        
        if (camDelayTrig && observerCam)
        {

            camDelayTimer++;
            if (camDelayTimer >= camChangeDelay)
            {

                camDelayTrig = false;
                camDelayTimer = 0;
                camDelayEndTrig = false;

                returnToCam.SetActive(false);
                switchCam(observerCam);
                observerCam.SetActive(true);
                
                
                isCamSwitchedTrig = true;
            //    if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> Switch to observer cam from timer");



            }
        }

        if (camReturnTrig && observerCam)
        {

            camReturnTimer++;
            if (camReturnTimer >= camReturnDelay)
            {

                camReturnTrig = false;
                camReturnTimer = 0;
                camReturnEndTrig = true;

                observerCam.SetActive(false);
                switchCam(returnToCam);
                returnToCam.SetActive(true);
              
                isCamSwitchedTrig = false;

            //    if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> Switch to player cam from timer");

                //  returnToCam.transform.parent.SetPositionAndRotation(propVect, propRot);

                if (isEncounterActive)
                    runNextEncounter();


            }
        }

               
    }

    void runNextEncounter()
    {

        if (nextEncounterObj)
        {

            if (nextEncounterObj.GetComponent<UniLogicChip>())
            {

                if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> run next encounter = " + nextEncounterObj.name);

                GetComponent<UniLogicChip>().setExtData(nextEncounterPin, true, 5);
            }

        }

    }

    public void ToggleAnim(bool state, bool isClick , bool isOutputBlocked)
    {
     
        if (state == true)
        {
            if (clickOnAudio != null && isClick)
            {
                playAudio(clickOnAudio, clipVolume, true, false);
                if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> initiate CLICK ON audio clip = " + clickOnAudio.name );

            }

            if (outputOnAudio != null && !isClick)
            {
                playAudio(outputOnAudio, clipVolume, true, false);
                if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> initiate DEVICE ON audio clip = " + outputOnAudio.name );
            }



            if (auxLevelObj1 && ! errorDetectedTrig)
                auxLevelObj1.SetActive(true);

            if (!isOutputBlocked && !isClick)
            {
                for (int i = 0; i < toggleOnObject.Length; i++)
                {
                    if (toggleOnObject[i] && toggleOnObject[i] != null)
                    {
                        toggleOnObjTrig[i] = true;
                        toggleOnObject[i].SetActive(true);
                        if (debugLevel > 1)
                            Debug.Log("<color=blue>" + gameObject.name + "</color> UniLogicEffects state=TRUE enables toggleOnObject = <color=blue>" + toggleOnObject[i].name + "</color>");
                        
                    }
                }

                if (!isOutputBlocked && !toggleOffObjDefState)
                {
                    for (int i = 0; i < toggleOffObject.Length; i++)
                    {
                        if (toggleOffObject[i] && toggleOffObject[i] != null)
                        {
                            toggleOffObjTrig[i] = false;
                            toggleOffObject[i].SetActive(false);

                            if (debugLevel > 1)
                                Debug.Log("<color=blue>" + gameObject.name + "</color> UniLogicEffects state=TRUE disables toggleOffObject = <color=blue>" + toggleOffObject[i].name + "</color>");
                          
                        }

                    }
                }
            }

        }
      
        if (state == false)
        {

            if (clickOffAudio && isClick)
            {
                playAudio(clickOffAudio, clipVolume, false, false);
                if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> Send Click Off audio clip = "+ clickOffAudio.name);

            }

            if (outputOffAudio && !isClick)
            {
        
               playAudio(outputOffAudio, clipVolume, false, true);
                if (debugLevel > 1)
                    Debug.Log("<color=blue>" + gameObject.name + "</color> Send Device Off audio clip = " + outputOffAudio.name);

            }

            if (isOffStateDucking && effectsAudio.isPlaying)
            {
                effectsAudio.loop = false;
                effectsAudio.Stop();

            }


            if (!isOutputBlocked && !isClick)
            {

                for (int i = 0; i < toggleOffObject.Length; i++)
                {
                    if (toggleOffObject[i] && toggleOffObject[i] != null)
                    {
                        toggleOffObjTrig[i] = true;
                        toggleOffObject[i].SetActive(true);

                        if (debugLevel > 1)
                            Debug.Log("<color=blue>" + gameObject.name + "</color> UniLogicEffects state=FALSE Enables ToggleOffObject <color=blue>" + toggleOffObject[i].name + "</color>");
                       
                    }

                }


                if (!isRemainOn && !isOutputBlocked)
                {
                    for (int i = 0; i < toggleOnObject.Length; i++)
                    {
                        if (toggleOnObject[i] && toggleOnObject[i] != null)
                        {
                            toggleOnObjTrig[i] = false;
                            toggleOnObject[i].SetActive(false);

                            if (debugLevel > 1)
                                Debug.Log("<color=blue>" + gameObject.name + "</color> UniLogicEffects state=FALSE Disables ToggleOnObject <color=blue>" + toggleOnObject[i].name + "</color> ");
                         
                        }

                    }
                }
            }
        }


    }

    public void playAudio(AudioClip sound, float vol, bool _state ,bool endingSound)
    {
        if (!sound || !effectsAudio || !isSystemReady)
        {
            Debug.LogWarning("<color=blue>" + gameObject.name + "</color> Tried to play sound but Audiosource is not attatched or system not ready");
            return;
        }
       

        effectsAudio.clip = sound;

        if (isPreventBargeIn && effectsAudio.isPlaying)
        {
            effectsAudio.loop = false;
            effectsAudio.Stop();

        }


        if (!_state && outputOffAudio == null && clickOffAudio == null)
        {
            effectsAudio.loop = false;
            effectsAudio.Stop();

            return;

        }

        if (!endingSound)
        {

            if (isAudioContinuous)
            {
                effectsAudio.volume = vol;
                effectsAudio.loop = true;
                effectsAudio.Play();

            }
            else
            {

                effectsAudio.volume = vol;
                effectsAudio.loop = false;
                effectsAudio.PlayOneShot(sound, vol);

            }

            if (debugLevel > 1)
                Debug.Log("<color=blue>" + gameObject.name + "</color> plays oneshot audio = <b>"+sound.name+"</b>");
        }

        else
        {
            
            effectsAudio.Stop();
            effectsAudio.loop = false;
            effectsAudio.volume = vol;
            effectsAudio.Play();
      

            if (debugLevel > 1)
                Debug.Log("<color=blue>" + gameObject.name + "</color> plays ending audio = <b>"+sound.name+"</b>");
        }

    }
      
    public static class AudioFX
    {

        public static IEnumerator TriggerSound(AudioSource audioSource, AudioClip clip, float vol, float fadeTime)
        {
            if (audioSource)
            {
                float startVolume = audioSource.volume;

                while (audioSource.volume > 0)
                {
                    audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
                    yield return null;
                }

                audioSource.Stop();
                audioSource.volume = startVolume;

                if (clip != null)
                {
                    audioSource.volume = vol;
                    audioSource.PlayOneShot(clip);
                }

            }

        }

    }



    public static class SwitchCamX
    {

        public static IEnumerator switchToCam(GameObject _cam)
        {


            foreach (var obj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
            {
                obj.GetComponent<UniLogicChip>().hudCamera = _cam.GetComponent<Camera>();
                yield return null;
            }
     
 



        }

    }

    void switchCam(GameObject _cam)
    {
         foreach (var obj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
          {
            obj.GetComponent<UniLogicChip>().hudCamera = null;
           
        }


        foreach (var obj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
        {
           
            obj.GetComponent<UniLogicChip>().hudCamera = _cam.GetComponent<Camera>();

           
        }
        Debug.Log("<color=blue>" + gameObject.name + "</color> > Change Cam = " + _cam.name);
    }



    void runFloatObj(GameObject _obj)
    {
        _obj.transform.Rotate(new Vector3(0f, Time.deltaTime * rotSpeedFloat, 0f), Space.World);
        tempPosFloat = posOffsetFloat;
        tempPosFloat.y += Mathf.Sin(Time.fixedTime * Mathf.PI * freqBounceFloat) * ampFloat;
        _obj.transform.position = tempPosFloat;

    }


    void runShakeObj(GameObject _obj)
    {

        //  _obj.shakeObject.Rotate(new Vector3(0f, Time.deltaTime * shakeRotSpeedFloat, 0f), Space.World);
        shakePosFloat = shakeOffsetFloat;
        shakePosFloat.y += Mathf.Sin(Time.fixedTime * Mathf.PI * shakeFreqBounceFloat) * shakeAmpFloat;
        shakeObject.transform.position = shakePosFloat;

    }
    void initBlox()
    {

        rotSpeedFloat = 20f;
        ampFloat = .01f;
        freqBounceFloat = 1f;
        // posOffsetFloat = transform.position;
        effectsAudio = GetComponent<AudioSource>();
        returnToCam = Camera.main.gameObject;

        //  anim = gameObject.GetComponent<Animation>();

       
        if (shakeObject)
            shakeOffsetFloat = shakeObject.transform.position;

        for (int i = 0; i < toggleOnObject.Length; i++)
                if (toggleOnObject[i])
                    toggleOnObject[i].SetActive(toggleOnObjDefState);

        if (toggleOnObject.Length > 0)
            toggleOnObjTrig = new bool[toggleOnObject.Length];

        for (int i = 0; i < toggleOffObject.Length; i++)
            if (toggleOffObject[i])
                toggleOffObject[i].SetActive(toggleOffObjDefState);

        if (toggleOffObject.Length > 0)
            toggleOffObjTrig = new bool[toggleOffObject.Length];


        if (toggleSoftFailObj)
            toggleSoftFailObj.SetActive(false);

        if (toggleHardFailObj)
            toggleHardFailObj.SetActive(false);

        if (observerCam)
            observerCam.SetActive(false);

        

        isCamSwitchedTrig = false;
        isSystemReady = true;

        if (debugLevel > 0)
            Debug.Log("<color=blue>" + gameObject.name + "</color> > UniLogicEffects.cs - verbose debug text active at level " + debugLevel);

    }



    IEnumerator waitCoroutine()
    {

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1);

    }
}

