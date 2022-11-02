using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniPinBall : MonoBehaviour
{

    const float volDef = .3f, ballVolDef = .1f;
    public int ballNum;
    public bool isInPlay;
    bool isSewering;
    int sewerTrack;
    public bool isUseBallCam;

    public AudioClip sewerBall;
    public AudioClip rollBall;
    AudioSource audioSource;
    //  AudioSource ballAudioSource;
    public Camera ballCam;
    Vector3 fwd;
    Vector3 lastPosition = Vector3.zero;
    public float ballSpeed;
    public float vol;
    public float ballVol;
    public bool isBounceBall;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        vol = volDef;
        ballVol = ballVolDef;

        if (ballCam)
        {
            fwd = -ballCam.transform.forward;
            fwd.y = 0;
        }

    }


    void Update()
    {
        if (isSewering)
        {
            sewerTrack++;

            if (sewerTrack == 20)
            {
                GetComponent<SphereCollider>().isTrigger = false;
                sewerTrack = 0;
                isSewering = false;
                Destroy(gameObject);
            }

        }


        if (ballCam && isUseBallCam && ballCam.isActiveAndEnabled)
            ballCam.transform.rotation = Quaternion.LookRotation(fwd);




        if (ballSpeed > 2)
        {

            ballVol = ballSpeed / 20;
         
            if (ballVol > ballVolDef)
                ballVol = ballVolDef;

            if (ballVol < 0)
                ballVol = 0f;

            if (isInPlay && !isBounceBall)
            {
                SoundFX(GetComponent<AudioSource>(), rollBall, ballVol, false, true);
                audioSource.loop = true;
            }

        }
        else
        {
            if (!isSewering)
            {
                audioSource.loop = false;
                audioSource.Stop();

            }
        }

    }


    void FixedUpdate()
    {
        ballSpeed = (((transform.position - lastPosition).magnitude) / Time.deltaTime);
        lastPosition = transform.position;


        if (isBounceBall && !audioSource.isPlaying)
           isBounceBall = false;
      
    }

    public void setBallCam()
    {

        if (!isUseBallCam)
        {
            ballCam.enabled = true;
            isUseBallCam = true;
        }
        else
        {
            ballCam.enabled = false;
            isUseBallCam = false;
        }



    }

    void OnCollisionEnter(Collision collision)
    {

        isBounceBall = true;


        if (collision.collider.name.ToLower() == "entersewer")
        {
            vol = volDef;
            SoundFX(GetComponent<AudioSource>(), sewerBall, vol, false, false);
            GetComponent<SphereCollider>().isTrigger = true;
            isSewering = true;
            isInPlay = false;

        }


        if (collision.collider.name.ToLower() == "flap")
        {
           // Debug.Log(collision.collider.name);
            isInPlay = true;
        }

        if (collision.collider.name.ToLower() == "paddle")
        {
             Debug.Log(collision.collider.name);
          
        }



    }

    void OnCollisionExit(Collision collision)
    {
        // GetComponent<SphereCollider>().isTrigger = false;




    }

    public void SoundFX(AudioSource audio, AudioClip sound, float vol, bool block, bool loop)
    {
        StartCoroutine(AudioFX.TriggerSound(audio, sound, vol, block, loop));
    }

    public static class AudioFX
    {
        public static IEnumerator TriggerSound(AudioSource audioSource, AudioClip clip, float vol, bool block, bool longPlay)
        {
            if (block)
            {

                //   if (audioSource.isPlaying)
                //       yield return 0;

                while (audioSource.isPlaying)
                {
                    yield return null;
                }
            }

            if (audioSource.isActiveAndEnabled)
            {
                audioSource.volume = vol;

                if (longPlay)
                {

                    audioSource.clip = clip;
                    audioSource.Play();
                }
                else
                {
                    audioSource.PlayOneShot(clip);
                }
            }

        }
    }

}
