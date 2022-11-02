using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniLogicPatch : MonoBehaviour
{
    
    public GameObject[] updateToObj;

    public float switchSteps;
    public float maxSwitchSteps;
    public float minSwitchSteps;
    
    public float defaultPos;
    public float selMaxPos;
    public float selMinPos;
  
    public float softHighErrLimit;
    public float softLowErrLimit;
    public float hardHighErrLimit;
    public float hardLowErrLimit;

    public bool runpatch;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (runpatch)
        {
            runpatch = false;
            applyPatch();
        }
       

    }



    void applyPatch()
    {

        if (runpatch)
        {
            applyPatchFeild("logic");
        }
    }


    void applyPatchFeild(string _file)
    {

        if (_file == "logic")
        {
            foreach (var obj in FindObjectsOfType(typeof(UniLogicChip)) as UniLogicChip[])
            {
                for (int i = 0; i < updateToObj.Length; i++)
                {
                    if (updateToObj[i]!=null && obj == updateToObj[i])
                    {
                        if (switchSteps != -99)
                            obj.switchSteps = switchSteps;

                        if (switchSteps != -99)
                            obj.maxSwitchSteps = maxSwitchSteps;

                        if (switchSteps != -99)
                            obj.minSwitchSteps = minSwitchSteps;

                        if (switchSteps != -99)
                            obj.defaultPos = defaultPos;

                        if (switchSteps != -99)
                            obj.selMaxPos = selMaxPos;

                        if (switchSteps != -99)
                            obj.selMinPos = selMinPos;

                        if (switchSteps != -99)
                            obj.softHighErrLimit = softHighErrLimit;

                        if (switchSteps != -99)
                            obj.softLowErrLimit = softLowErrLimit;

                        if (switchSteps != -99)
                            if (switchSteps != -99) obj.hardHighErrLimit = hardHighErrLimit;

                        if (switchSteps != -99)
                            obj.hardLowErrLimit = hardLowErrLimit;
                    }
                }
            }
        }




    }


}




