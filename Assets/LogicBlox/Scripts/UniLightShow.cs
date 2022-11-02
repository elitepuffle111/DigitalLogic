using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniLightShow : MonoBehaviour
{
    public const int numNotes = 12;
    public const float maxLightLev = 3;


    public const float decayLenDef = 36.3f;
    public const float attackLenDef = 40;
    public const float rmsLowTrigLevDef = .02f;
    public const float pitchTrigLevDef = .25f;
    public const float fadeTickDef = .11f;
    public const float areaTickDef = 50;
   
    public GameObject audioMasterObj;

    public string circuitGroup;

    [Range(0f, 50f)]
    public float decayLenVal;

    [Range(0f, 100f)]
    public float attackLenVal;

    [Range(0f, .001f)]
    public float rmsHighTrigLev;

    [Range(1f, 10)]
    public float pitchLevTrig;

    [Range(.0001f, 1f)]
    public float fadeTick;

    [Range(50, .01f)]
    public float areaTick;

    public bool isRunShow;
    
    public GameObject[] rmsToneTrigObj = new GameObject[numNotes];
  
    float[] rmsValues = new float[numNotes];
    float[] pitchValues = new float[numNotes];
    float[] dbValues = new float[numNotes];
    float[] lightIntensity = new float[numNotes];
    float [] lightSize = new float[numNotes];
       
    bool[] isDecayTrig = new bool[numNotes];
    bool[] isAttackTrig = new bool[numNotes];
    bool[] isPitchTrig = new bool[numNotes];

    float[] decayTimer = new float[numNotes];
    float[] attackTimer = new float[numNotes];
    float[] pitchTimer = new float[numNotes];
    Color[] pitchColorBase = new Color[numNotes];


    UniTones uniTones;
    int sourceOctave;

    void Start()
    {
       if (audioMasterObj)
           uniTones = audioMasterObj.GetComponent<UniTones>();
         else
           uniTones = GetComponent<UniTones>();
  
        sourceOctave = uniTones.octave;
        loadParams();

    }

    void Update()
    {
        if (uniTones && isRunShow)
        {
            getSounds();
            runEffects();
            updateLogicTimers();

        }
    }


    void getSounds()
    {
        uniTones.AnalyzeSound();

        string rmsString = uniTones.rmsAll;
        float[] rmsFloatData = Array.ConvertAll(rmsString.Split(','), float.Parse);
        rmsValues = rmsFloatData;

        string pitchString = uniTones.pitchAll;
        float[] pitchFloatData = Array.ConvertAll(pitchString.Split(','), float.Parse);
        pitchValues = pitchFloatData;

        string dbString = uniTones.dbAll;
        float[] dbFloatData = Array.ConvertAll(dbString.Split(','), float.Parse);
        dbValues = dbFloatData;


    }

    void loadParams()
    {
        if (decayLenVal==0)
        decayLenVal = decayLenDef;

        if (attackLenVal == 0)
            attackLenVal = attackLenDef;

        if (rmsHighTrigLev==0)
        rmsHighTrigLev = rmsLowTrigLevDef;

        if (fadeTick==0)
        fadeTick = fadeTickDef;

        if (pitchLevTrig == 0)
            pitchLevTrig = pitchTrigLevDef;

        if (areaTick == 0)
            areaTick = areaTickDef;

        for (int i = 0; i < rmsToneTrigObj.Length; i++)
        {
            decayTimer[i] = decayLenVal;
            lightIntensity[i]=rmsToneTrigObj[i].GetComponent<Light>().intensity;
            lightSize[i] = rmsToneTrigObj[i].GetComponent<Light>().range;
            pitchColorBase[i] = rmsToneTrigObj[i].GetComponent<Light>().color;
        }
    }

    void runEffects()
    {

        for (int i = 0; i < rmsToneTrigObj.Length; i++)
        {

            if (rmsValues[i] > rmsHighTrigLev && !rmsToneTrigObj[i].activeSelf)
            {
                rmsToneTrigObj[i].SetActive(true);
                isAttackTrig[i] = true;
            }

            if (rmsValues[i] < rmsLowTrigLevDef && rmsToneTrigObj[i].activeSelf)
            {
                isDecayTrig[ i] = true;
     
            }


            if (pitchValues[i] > pitchLevTrig )
            {
               
            isPitchTrig[i] = true;
            }


        }


    }

    void updateLogicTimers()
    {
        for (int i = 0; i < numNotes; i++)
        {

            if (isDecayTrig[i])
            {
                decayTimer[i]--;

                rmsToneTrigObj[i].GetComponent<Light>().intensity = rmsToneTrigObj[i].GetComponent<Light>().intensity - fadeTick;
                 if (decayTimer[i] <= 0f)
                {
                    decayTimer[i] = decayLenVal;
                    isDecayTrig[i] = false;
                    rmsToneTrigObj[i].GetComponent<Light>().intensity = lightIntensity[i];
                    rmsToneTrigObj[i].SetActive(false);
                    isAttackTrig[i] = false;

                }

            }

            if (isAttackTrig[ i])
            {
                attackTimer[i]--;
                rmsToneTrigObj[i].GetComponent<Light>().range= rmsToneTrigObj[i].GetComponent<Light>().range + lightIntensity[i]/areaTick;

                rmsToneTrigObj[i].GetComponent<Light>().color = Color.Lerp(pitchColorBase[i], Color.black, Mathf.PingPong(Time.time, 1));

                if (attackTimer[i] <= 0f)
                {
                    attackTimer[i] = decayLenVal;
                    isAttackTrig[i] = false;
                    rmsToneTrigObj[i].GetComponent<Light>().range = lightSize[i];
                    rmsToneTrigObj[i].GetComponent<Light>().color =pitchColorBase[i];

                }

            }

            if (isPitchTrig[i])
            {
                pitchTimer[i]--;
        
                if (pitchTimer[i] <= 0f)
                {
                    pitchTimer[i] = decayLenVal;
                    isPitchTrig[i] = false;
               }

            }
        }


    }
}