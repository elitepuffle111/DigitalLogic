using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniLogicSeq : MonoBehaviour
{

    [Header("Start Triggers")]

    public bool startSeq;

    [Header("Devices to turn on")]
    public GameObject[] turnOnDev;
    public float[] devDelay;


    [Header("Objects to turn on")]
    public GameObject[] enableGameObject;
    public float[] objDelay;


    [Header("Sequencer Parts")]
    public GameObject start1But;
    public GameObject start2But;

    public GameObject speedDownBut;
    public GameObject speedUpBut;

    public GameObject start1Led;
    public GameObject start2Led;

    public GameObject displayMaster;

    public GameObject pin0;
    public GameObject pin1;
    
 
    // Start is called before the first frame update
    void Start()
    {
    
        setupLogicGrid();
    }

    // Update is called once per frame
    void Update()
    {

    }


    void OnMouseDown()
    {
      //  int pinInt = 0;
    
       int pinInt = GetComponent<UniLogicChip>().clickPart(gameObject);

        if (pinInt > -1)
        {

            turnOnDevice(pinInt);

        }

    }

    void turnOnDevice(int _dev)
    {
        int pinInt = 0;

        if (turnOnDev.Length > 0 && turnOnDev[_dev] != null && turnOnDev[_dev].GetComponent<UniLogicChip>() && turnOnDev[_dev].GetComponent<UniLogicChip>().pinState[pinInt] == false)
        {
            if (turnOnDev[_dev].GetComponent<UniLogicChip>().pinState[pinInt] == false)
            {
                turnOnDev[_dev].GetComponent<UniLogicChip>().pinState[pinInt] = true;
                turnOnDev[_dev].GetComponent<UniLogicChip>().gateState[pinInt] = true;
            }
            else
            {
                if (turnOnDev[_dev].GetComponent<UniLogicChip>().pinState[pinInt] == true)
                {
                    turnOnDev[_dev].GetComponent<UniLogicChip>().pinState[pinInt] = false;
                    turnOnDev[_dev].GetComponent<UniLogicChip>().gateState[pinInt] = false;
                }
            }
        }
    }

     void setupLogicGrid()
    {

        if (turnOnDev.Length > 0)
            turnOnDev = new GameObject[turnOnDev.Length];

        if (devDelay.Length > 0)
            devDelay = new float[devDelay.Length];

        if (enableGameObject.Length > 0)
            enableGameObject = new GameObject[enableGameObject.Length];

        if (objDelay.Length > 0)
            objDelay = new float[objDelay.Length];

    }


}