using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UniPinBallMach : MonoBehaviour
{

    const float defFlipperForce = 60;
    
    public string circuitGroup;
  
    GameObject ballInPlay;
    public Transform ballSpawnPoint;
    public bool isLoadNewBall;

    public bool isLaunchBall;
    public bool isUseBallCam;
    public bool isBallLoaded;
    public float ballLaunchForce;

  

   
}
