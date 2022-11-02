using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UniPlayerCamDemo : MonoBehaviour
{

    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    public Vector2 clampInDegrees = new Vector2(360, 360);

    public Vector2 sensitivity = new Vector2(1, 1);
    public Vector2 smoothing = new Vector2(3, 3);
    Vector2 targetDirection = new Vector2();

    private KeyCode cam_Up = KeyCode.PageUp;
    private KeyCode cam_Dn = KeyCode.PageDown;

    private KeyCode cam_Home = KeyCode.Home;
    private KeyCode cam_FreeMouse = KeyCode.LeftControl;

    private KeyCode obj_Down = KeyCode.R;
    private KeyCode obj_Up = KeyCode.Space;

    public float orbitSensitivity;
    float zoomSpeed = 2;
    float orthographicSizeMin = 2;
    float orthographicSizeMax = 1000;
    float fovMin = .05f;
    float fovMax = 120;
    public float jumpSpeed;

    float targetDirectionOrbit;
    Vector3 offset;

    public Camera playerCamera;

    CharacterController controller;

    private bool camOrbit = false;

    public float fps_Height, fps_Distance, fps_Offset;
    float _fps_Height, _fps_Distance, _fps_Offset;
    public float fps_sens_offset;
    public float fps_smooth_offset;


    public bool freeMouse;

    public float currentSpeed;
    public float gravity = 0;

    public Vector3 moveDirection = Vector3.zero;
    bool isBlockMouse;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        _fps_Distance = transform.position.z;
        _fps_Height = transform.position.y;
        _fps_Offset =  transform.position.x;

      //  var targetOrientation = Quaternion.Euler(targetDirection);

      //  offset = (transform.position - transform.position);
      //  transform.rotation= Quaternion.AngleAxis(0, targetOrientation * Vector3.right) * targetOrientation;

    }


    void Update()
    {
        moveController();
        mouseAim();
        getCommands();

    }

    public int mouseAim()
    {

        if (freeMouse == false)
        {
           

            var targetOrientation = Quaternion.Euler(targetDirection);
            offset = (transform.position - transform.position);

            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x + fps_sens_offset * smoothing.x + fps_smooth_offset, sensitivity.y + fps_sens_offset * smoothing.y + fps_smooth_offset));
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);
            _mouseAbsolute += _smoothMouse;


            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);


            transform.localRotation = Quaternion.AngleAxis(0, targetOrientation * Vector3.right) * targetOrientation;

            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;


            if (camOrbit == false)
                  playerCamera.transform.localRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right) * targetOrientation;


            //mouse zoom
            if (playerCamera.orthographic)
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                    playerCamera.orthographicSize += zoomSpeed;
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    playerCamera.orthographicSize -= zoomSpeed;

                playerCamera.orthographicSize = Mathf.Clamp(playerCamera.orthographicSize, orthographicSizeMin, orthographicSizeMax);
            }
            else
            {
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    playerCamera.fieldOfView += zoomSpeed;
                }

                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    playerCamera.fieldOfView -= zoomSpeed;
                }


                playerCamera.fieldOfView = Mathf.Clamp(playerCamera.fieldOfView, fovMin, fovMax);
            }


        }
        return (int)playerCamera.fieldOfView;

    }




    public void rotateCam(float dir)
    {
        Debug.Log("targetDirectionOrbit cam");


        offset = (playerCamera.transform.position - transform.parent.position);

        Quaternion q = Quaternion.AngleAxis(dir, Vector3.up);
        offset = q * offset;

        playerCamera.transform.rotation = q * playerCamera.transform.rotation;
        playerCamera.transform.position = transform.parent.position + offset;

    }


    void getCommands()
    {
        if (Input.GetKeyDown(cam_FreeMouse))
        {
            if (!freeMouse)
            {
                freeMouse = true;
            }
            else
            {
                if (freeMouse)
                    freeMouse = false;
            }

            return;
        }

        if (Input.GetKey(cam_Up))
        {
            //  _fps_Height = _fps_Height + camFlySpeed;
            //   playerCamera.transform.localPosition = new Vector3(_fps_Offset, _fps_Height, _fps_Distance);
            return;
        }


        if (Input.GetKey(cam_Dn))
        {
            //  _fps_Height = _fps_Height - camFlySpeed;
            //  playerCamera.transform.localPosition = new Vector3(_fps_Offset, _fps_Height, _fps_Distance);
            return;
        }


        if (Input.GetKey(cam_Home))
        {
            playerCamera.transform.localPosition = new Vector3(0, 0, 0);
            return;
        }


    }

    void moveController()
    {
        // float grav = 0;
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= currentSpeed;

        if (Input.GetKey(obj_Up))
        {
            moveDirection.y = jumpSpeed;
            //  grav = 0;

        }
        if (Input.GetKey(obj_Down))
        {
            moveDirection.y = -jumpSpeed;

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }


   

}

