using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniFunc : MonoBehaviour
{
       
    int currentClickPin;

    public void moveLocalPart(GameObject _part, float _step, int _plane)
    {
        if (_part == null)
            return;

        Quaternion currentRotation = _part.transform.rotation;
        Vector3 currentPosition = _part.transform.position;
        //    Quaternion wantedRotation = Quaternion.Euler(0, 0, angle);
        //    part.transform.rotation = Quaternion.RotateTowards(currentRotation, wantedRotation, Time.deltaTime * 90);

        //  Debug.Log(_plane);

        if (_plane == 0)
            _part.transform.Rotate(0, 0, _step);

        if (_plane == 1)
            _part.transform.Rotate(0, _step, 0);

        if (_plane == 2)
            _part.transform.Rotate(_step, 0, 0);


        if (_plane == 3)
        {

            if (Mathf.Abs(Vector3.Dot(transform.right, Vector3.down)) < 0.825f)
                _part.transform.position = new Vector3(currentPosition.x, currentPosition.y + _step, currentPosition.z);


            if (Mathf.Abs(Vector3.Dot(transform.right, Vector3.down)) > 0.825f)
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

    public int clickPart(GameObject hitObject)
    {
       
        if (Camera.main && hitObject.GetComponent<UniLogicChip>())
        {
            currentClickPin = -1;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool verifyLbHit = false;
            int pinInt = -1;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.GetComponentInParent<UniLogicChip>() && hit.transform.gameObject.GetComponentInParent<UniLogicChip>().chipType!="")
                {
                   GameObject currentClickObj = hit.transform.gameObject;
             
                   int debugLev = currentClickObj.GetComponent<UniLogicChip>().debugLevel;


                    for (int i = 0; i < hitObject.GetComponent<UniLogicChip>().pinObj.Length; i++)
                        if (hit.collider.transform.name == hitObject.GetComponent<UniLogicChip>().pinObj[i].transform.name)
                            verifyLbHit = true;

                    for (int i = 0; i < hitObject.GetComponent<UniLogicChip>().ledObj.Length; i++)
                        if (hit.collider.transform.name == hitObject.GetComponent<UniLogicChip>().ledObj[i].transform.name)
                            verifyLbHit = true;

                    Collider[] hitPin = currentClickObj.GetComponents<Collider>();

                    if (hitPin != null && verifyLbHit)
                    {
                       // Debug.Log("Clicked on object <color=blue>" + gameObject.name + "</color> Target = " + hit.collider.transform.name);
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

                    Debug.LogWarning(gameObject.name + " uniLogicChip.cs script is not attched to object. Or chipType is not defined");

                }

            }

          
                

        }

        return -1;
    }


}
