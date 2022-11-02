using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

//<<<Logic Blox v 3.0 - Universal script for spawining objects
// Developed by Mike Hogan (2018) - Granby Games - mhogan@remhouse.com
// Updted April 10 - LB Ver 3 compatability

public class UniSpawn : MonoBehaviour
{
    [Header("General Settings")]
    public string encounterName;
    public string attackGroupName;

    public GameObject mobPreFab;
    public float spawnInterval = 3f;
    public bool isJustArmy;
    float startSpawnDelayTimer;
    public float startSpawnDelayTimerMax = 30;
     
 
    [Header("Encounter States")]
 
    public int mobCount;
    public int countSpawns;
    public bool isArmySpawned;
    public bool isStartArmy;

    public bool IsEncounterStarted;
    public bool isFinalCompletion;

    
    [Header("Army Settings")]
    public bool armyEarlySpawn;
    public Transform armySpawnPoint;
    public int armySize = 10;
    public int armyNumInLine = 5;
    int armylineSizeCount;

    public float armyRowSpace = 2, armyColSpace = 3;
    public float formationNoise;
     
    float armyNpcPosX, armyNpcPosY, armyNpcPosZ;
    public int debugLevel;

    
    void Start()
    {
    
        if (armySpawnPoint)
        {
            armyNpcPosX = armySpawnPoint.position.x;
            armyNpcPosY = armySpawnPoint.position.y;
            armyNpcPosZ = armySpawnPoint.position.z;
        }

        if (isJustArmy)
            mobCount = armySize;

        else

        if (debugLevel > 0)
            Debug.Log("UniSpawn controller initalized for - " + encounterName + ", Startup delay= " + startSpawnDelayTimerMax.ToString());

    }
    
    void Update()
    {
        if (isStartArmy)
        {
            generateArmy();
            isStartArmy = false;

         }


    }

    void FixedUpdate()
    {

    }
       
    void CmdSetupSpawns()
    {
        if (IsEncounterStarted == false && !isFinalCompletion)
        {

            if (armySpawnPoint)
            {
                armyNpcPosX = armySpawnPoint.position.x;
                armyNpcPosY = armySpawnPoint.position.y;
                armyNpcPosZ = armySpawnPoint.position.z;
            }


            if (!isStartArmy && !isArmySpawned)
            {
                mobCount = armySize;
                isStartArmy = true;

            }

        }

        IsEncounterStarted = true;

    }
     
    void generateArmy()
    {
        float rndX = 0, rndZ = 0;

        while (countSpawns < armySize)
        {
            if (formationNoise > 0)
            {
                rndX = Random.Range(0f, formationNoise);
                rndZ = Random.Range(0f, formationNoise);
            }

            Vector3 armylineup = new Vector3(armyNpcPosX + rndX, armyNpcPosY, armyNpcPosZ + rndZ);
            if (debugLevel > 2)
                Debug.Log("Instantiating : " + mobPreFab.name + ", Index: " + countSpawns + "Encounter: " + encounterName);

            GameObject armySpawn = Instantiate(mobPreFab, armylineup, armySpawnPoint.rotation);

            armyNpcPosX = armyNpcPosX + armyColSpace;
            armylineSizeCount++;

            if (armylineSizeCount >= armyNumInLine)
            {
                armyNpcPosX = armySpawnPoint.position.x;
                armylineSizeCount = 0;
                armyNpcPosZ = armyNpcPosZ - armyColSpace;
            }


            countSpawns++;

            if (countSpawns >= armySize)
            {
                isStartArmy = false;
                isArmySpawned = true;

            }

        }
    }


}
 