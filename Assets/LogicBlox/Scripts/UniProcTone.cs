using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]

public class UniProcTone : MonoBehaviour
{
    
    const float ampMod = 30f, freqMod = 30f, freqInten = 100f;
    private double sampleRate, dataLen;
    double chunkTime, dspTimeStep, curDspTime;

    SawWave sawProcWave;
    SquareWave sqrProcWave;
    SinusWave sinProcWave;
    SinusWave ampModOsc;
    SinusWave freqModOsc;

    public float masterProcVol;

    [Header("Note Frequencies")]
    public double procFreq;
    public double baseProcFreq;   

    [Range(-500, 500)]
    public float procFreqAdj=0;

    public bool isPlaySinWave;
    public bool isPlaySqrWave;
    public bool isPlaySawWave;

    public bool isStopAll;


    [Space(5)]
    [Range(0.0f, 1.0f)]
    public float sinWaveIntensity = 0.25f;
    [Range(0.0f, 1.0f)]
    public float sqrWaveIntensity = 0.25f;
    [Range(0.0f, 1.0f)]
    public float sawWaveIntensity = 0.25f;

    [Header("Proc Amp Mod")]
    public bool isProcAmpMod;
    [Range(0.2f, ampMod)]
    public float ampModOscFreq = 1.0f;
    [Header("Proc Freq Mod")]
    public bool isProcFreqMod;

    [Range(0.2f, freqMod)]
    public float freqModOscFreq = 1.0f;

    [Range(1.0f, freqInten)]
    public float freqModOscIntensity = 10.0f;

    // Start is called before the first frame update
    void Awake()
    {
       
            sawProcWave = new SawWave();
            sqrProcWave = new SquareWave();
            sinProcWave = new SinusWave();
            ampModOsc = new SinusWave();
            freqModOsc = new SinusWave();
            sampleRate = AudioSettings.outputSampleRate;
            baseProcFreq = procFreq;
            masterProcVol = 1;

    }

   
    void Update()
    {
        if (isStopAll)
        {
            isStopAll = false;
            isPlaySinWave = false;
            isPlaySqrWave = false;
            isPlaySawWave = false;
            masterProcVol = 1;
        }  
        
    }


    #region Proc Tones

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaySawWave && !isPlaySinWave && !isPlaySqrWave )
           return;

     //   Debug.Log("Num channels " + channels.ToString());
             
        curDspTime = AudioSettings.dspTime;
        dataLen = data.Length / channels;
        chunkTime = dataLen / sampleRate;
        dspTimeStep = chunkTime / dataLen;

        double preciseDspTime;

        for (int i = 0; i < dataLen; i++)
        {
            preciseDspTime = curDspTime + i * dspTimeStep;
            double signalValue = 0.0;

            double currentFreq = procFreq + procFreqAdj;

            if (isProcFreqMod)
            {
                double freqOffset = freqModOscIntensity * (procFreq+ procFreqAdj) * 0.75 / 100.0;
                currentFreq += mapValueD(freqModOsc.calculateSignalValue(preciseDspTime, freqModOscFreq), -1.0, 1.0, -freqOffset, freqOffset);
            }


            if (isPlaySinWave)
                signalValue += sinWaveIntensity * sinProcWave.calculateSignalValue(preciseDspTime, currentFreq);

            if (isPlaySawWave)
                signalValue += sawWaveIntensity * sawProcWave.calculateSignalValue(preciseDspTime, currentFreq);

            if (isPlaySqrWave)
                signalValue += sqrWaveIntensity * sqrProcWave.calculateSignalValue(preciseDspTime, currentFreq);

            if (isProcAmpMod)
                signalValue *= mapValueD(ampModOsc.calculateSignalValue(preciseDspTime, ampModOscFreq), -1.0, 1.0, 0.0, 1.0);

            float x = masterProcVol * 0.5f * (float)signalValue;

            for (int j = 0; j < 1; j++)
               data[i * channels + j] = x;
        }

    }

    float mapValue(float referenceValue, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (referenceValue - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    double mapValueD(double referenceValue, double fromMin, double fromMax, double toMin, double toMax)
    {
        return toMin + (referenceValue - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }


    #endregion

}
