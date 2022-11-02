using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// <<<Logic Blox - Universal script for moving props
// Developed by Mike Hogan (2018) - Granby Games - mhogan@remhouse.com
// Updated April 6, 2020 - Updates for ver 3 release.


public class UniNav : MonoBehaviour
{
    public string circuitGroup;
    public bool isDistanceLimited;
    public float maxPosX = 200, maxPosY = 200, maxPosZ = 200;
    public bool isRandomSpeed;
    public float speed = 10f;
    public float lifeDuration = 8f;
    private float lifeTimer;

    public bool isRandomAngle;
    public bool isGoUp, isGoDown, isGoForward, isGoback, isGoRight, isGoLeft;


    float startPosX, startPosY, startPosZ;

    Transform homePoint;

    public string startupFacingDir;


    public float degreesPerSecond = 15.0f;
    public float amplitude = 0.5f;
    public float frequency = 1f;

    Vector3 posOffset = new Vector3();
    Vector3 tempPos = new Vector3();


    void Start()
    {
        // compat adj
        if (maxPosX < 3)
            maxPosX = 20;
        if (maxPosY < 3)
            maxPosY = 20;
        if (maxPosZ < 3)
            maxPosZ = 20;

        lifeTimer = lifeDuration;
        setStartDir();

        if (!homePoint)
            homePoint = gameObject.transform;

        posOffset = transform.position;
    }

    void Update()
    {

        transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);

        // Float up/down with a Sin()
        tempPos = posOffset;
        tempPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

        transform.position = tempPos;


     //   steerObject();
     //   checkDistance();


    }



    private void OnTriggerEnter(Collider other)
    {
        float angle = 180;
        float a = angle;

        if (isRandomAngle)
        {
            a = Random.Range(0, 360);
            angle = a;
        }



        if (isGoUp)
        {
            isGoDown = false;
            rotatePart(gameObject, angle, 0);
        }

        if (isGoDown)
        {
            isGoUp = false;
            rotatePart(gameObject, angle, 0);
        }


        if (isGoForward)
        {
            isGoback = false;
            rotatePart(gameObject, angle, 1);
        }

        if (isGoback)
        {
            isGoForward = false;
            rotatePart(gameObject, angle, 1);
        }



    }

    void steerObject()
    {

        float x = speed;
        if (isRandomSpeed)
        {
            x = Random.Range(speed - (speed / 2), speed + (speed / 2));
            if (x < 3f)
                x = 3f;

        }

        if (isGoUp)
        {
            isGoDown = false;
            transform.position += transform.forward * x * Time.deltaTime;
        }


        if (isGoDown)
        {
            isGoUp = false;
            transform.position -= transform.forward * x * Time.deltaTime;
        }


        if (isGoForward)
        {
            isGoback = false;
            transform.position += transform.up * x * Time.deltaTime;
        }


        if (isGoback)
        {
            isGoForward = false;
            transform.position -= transform.up * x * Time.deltaTime;
        }


        if (isGoLeft)
        {
            isGoRight = false;
            transform.position += transform.right * x * Time.deltaTime;
        }

        if (isGoRight)
        {
            isGoLeft = false;
            transform.position -= transform.right * x * Time.deltaTime;
        }


    }

    void checkDistance()
    {
        if (isDistanceLimited)
        {
            float angle = 180;
            float a = angle;

            if (isRandomAngle)
            {
                a = Random.Range(0, 360);
                angle = a;
            }

            if (transform.localPosition.x > maxPosX)
            {
                rotatePart(gameObject, angle, 0);
            }

            else
            {
                if (transform.localPosition.x < -maxPosX)
                {

                    rotatePart(gameObject, angle, 0);
                }
            }




            if (transform.localPosition.y > maxPosY)
            {
                rotatePart(gameObject, angle, 2);
            }
            else
            {
                if (transform.localPosition.y < -maxPosY)
                {
                    rotatePart(gameObject, angle, 2);
                }
            }


            if (transform.localPosition.z > maxPosZ)
            {
                rotatePart(gameObject, angle, 0);
            }
            else
            {

                if (transform.localPosition.z < -maxPosZ)
                {
                    rotatePart(gameObject, angle, 0);
                }
            }
        }



        if (transform.localPosition.x > maxPosX * 2 || transform.localPosition.x < -maxPosX * 2)
        {
            Destroy(transform.gameObject);
        }

        if (transform.localPosition.y > maxPosY * 2 || transform.localPosition.y < -maxPosY * 2)
        {
            Destroy(transform.gameObject);
        }

        if (transform.localPosition.z > maxPosY * 2 || transform.localPosition.z < -maxPosZ * 2)
        {
            Destroy(transform.gameObject);
        }

    }

    void rotatePart(GameObject part, float angle, int plane)
    {
        Quaternion currentRotation = part.transform.rotation;

        if (plane == 0)
        {
            part.transform.Rotate(0, 0, angle);
        }
        if (plane == 1)
        {
            part.transform.Rotate(0, angle, 0);
        }
        if (plane == 2)
        {
            part.transform.Rotate(angle, 0, 0);
        }



    }

    void setStartDir()
    {
        if (startupFacingDir == "")
        {
            startPosX = transform.localPosition.x;
            startPosY = transform.localPosition.y;
            startPosZ = transform.localPosition.z;
        }

        if (startupFacingDir.ToLower() == "up")
        {
            startPosX = transform.localPosition.z;
            startPosY = transform.localPosition.y;
            startPosZ = transform.localPosition.x;
        }

        if (startupFacingDir.ToLower() == "right")
        {
            startPosX = transform.localPosition.x;
            startPosY = transform.localPosition.y;
            startPosZ = transform.localPosition.z;
        }

        if (startupFacingDir.ToLower() == "left")
        {
            startPosX = transform.localPosition.y;
            startPosY = transform.localPosition.z;
            startPosZ = transform.localPosition.x;
        }

        if (startupFacingDir.ToLower() == "down")
        {
            startPosX = transform.localPosition.z;
            startPosY = transform.localPosition.x;
            startPosZ = transform.localPosition.y;
        }

    }

}
