using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniHudAgent : MonoBehaviour
{
    [Header("Main ControlNode")]
    public GameObject mainLinkObject;


    [Header("Press number 1-5")]
    public GameObject outputLinkObject1;
    public GameObject outputLinkObject2;
    public GameObject outputLinkObject3;
    public GameObject outputLinkObject4;
    public GameObject outputLinkObject5;


    [Header("Pulse output")]
    public bool isPulseObj1;
    public bool isPulseObj2;
    public bool isPulseObj3;
    public bool isPulseObj4;
    public bool isPulseObj5;


    [Header("Toggle output")]
    public bool isToggleObj1;
    public bool isToggleObj2;
    public bool isToggleObj3;
    public bool isToggleObj4;
    public bool isToggleObj5;


    private KeyCode key_toggle1 = KeyCode.Alpha1;
    private KeyCode key_toggle2 = KeyCode.Alpha2;
    private KeyCode key_toggle3 = KeyCode.Alpha3;
    private KeyCode key_toggle4 = KeyCode.Alpha4;
    private KeyCode key_toggle5 = KeyCode.Alpha5;


    private KeyCode key_esc = KeyCode.Escape;

    bool linkTrig1, linkTrig2, linkTrig3,linkTrig4,linkTrig5;
    bool mainLinkTrig;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        scanKey();
        scanTouch();
            

    }


    void scanKey()
    {
        if (Input.GetKeyDown(key_toggle1))
        {

            if (outputLinkObject1 && !linkTrig1)
            {
                outputLinkObject1.GetComponent<UniLogicChip>().setExtData(0, true, 0);
                linkTrig1 = true;

            }
            else
            {
                if (isToggleObj1 && outputLinkObject1)
                {
                    outputLinkObject1.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                    linkTrig1 = false;
                }

            }
        }

        if (Input.GetKeyDown(key_toggle2))
        {

            if (outputLinkObject2 && !linkTrig2)
            {
                outputLinkObject2.GetComponent<UniLogicChip>().setExtData(0, true, 0);
                linkTrig2 = true;

            }
            else
            {
                if (isToggleObj2 && outputLinkObject2)
                {
                    outputLinkObject2.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                    linkTrig2 = false;
                }

            }
        }

        if (Input.GetKeyDown(key_toggle3))
        {

            if (outputLinkObject3 && !linkTrig3)
            {
                outputLinkObject3.GetComponent<UniLogicChip>().setExtData(0, true, 0);
                linkTrig3 = true;

            }
            else
            {
                if (isToggleObj3 && outputLinkObject3)
                {
                    outputLinkObject3.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                    linkTrig3 = false;
                }

            }
        }

        if (Input.GetKeyDown(key_toggle4))
        {

            if (outputLinkObject4 && !linkTrig4)
            {
                outputLinkObject4.GetComponent<UniLogicChip>().setExtData(0, true, 0);
                linkTrig4 = true;

            }

            else
            {
                if (isToggleObj4 && outputLinkObject4)
                {
                    outputLinkObject4.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                    linkTrig4 = false;
                }

            }
        }

        if (Input.GetKeyDown(key_toggle5))
        {

            if (outputLinkObject5 && !linkTrig5)
            {
                outputLinkObject5.GetComponent<UniLogicChip>().setExtData(0, true, 0);
                linkTrig5 = true;

            }

            else
            {
                if (isToggleObj5 && outputLinkObject5)
                {
                    outputLinkObject5.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                    linkTrig5 = false;
                }

            }
        }

        if (Input.GetKeyUp(key_toggle1))
        {

            if (isPulseObj1 && outputLinkObject1)
            {
                outputLinkObject1.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                linkTrig1 = false;
            }

        }

        if (Input.GetKeyUp(key_toggle2))
        {

            if (isPulseObj2 && outputLinkObject2)
            {
                outputLinkObject2.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                linkTrig2 = false;
            }

        }

        if (Input.GetKeyUp(key_toggle3))
        {

            if (isPulseObj3 && outputLinkObject3)
            {
                outputLinkObject3.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                linkTrig3 = false;
            }

        }

        if (Input.GetKeyUp(key_toggle4))
        {

            if (isPulseObj4 && outputLinkObject4)
            {
                outputLinkObject4.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                linkTrig4 = false;
            }

        }

        if (Input.GetKeyUp(key_toggle5))
        {

            if (isPulseObj5 && outputLinkObject5)
            {
                outputLinkObject5.GetComponent<UniLogicChip>().setExtData(0, false, 0);
                linkTrig5 = false;
            }

        }


        if(Input.GetKeyDown(key_esc))
        {
            if (!mainLinkTrig)
            {
                mainLinkTrig = true;

                foreach (var obj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
                {
                   // obj.GetComponent<UniLogicChip>().hudCamera = GetComponent<Camera>();
                }

            }
        }

        if (Input.GetKeyUp(key_esc))
        {
           
                mainLinkTrig = false;
        
        }




    }


    void scanTouch()
    {
        ///to do here

    }


}



