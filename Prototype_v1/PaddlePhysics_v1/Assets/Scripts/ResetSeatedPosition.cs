using System.Collections;
using UnityEngine;
using VRTK;

public class ResetSeatedPosition : MonoBehaviour
{
    [Tooltip("Desired head position of player when seated")]
    public Transform desiredHeadPosition;

    //Assign these variables in the inspector, or find them some other way (eg. in Start() )
    public Transform steamCamera;
    public Transform cameraRig;


    void Start()
    {
        StartCoroutine(InitEvents());
    }

    public IEnumerator InitEvents()
    {
        yield return new WaitUntil(CheckForControllers);
        VRTK_ControllerEvents controllerEventsL = ZB_SceneSingletons.ControllerEventsL;
        VRTK_ControllerEvents controllerEventsR = ZB_SceneSingletons.ControllerEventsR;

        controllerEventsL.TouchpadPressed += new ControllerInteractionEventHandler(DoTouchPadPressed);

        controllerEventsR.TouchpadPressed += new ControllerInteractionEventHandler(DoTouchPadPressed);


    }

    public static bool CheckForControllers()
    {
        if (ZB_SceneSingletons.ControllerEventsL != null && ZB_SceneSingletons.controllerR != null)
            return true;
        else
            return false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (desiredHeadPosition != null)
            {
                ResetSeatedPos(desiredHeadPosition);
            }
            else
            {
                Debug.LogError("Target Transform required. Assign in inspector.", gameObject);
            }
        }
    }

    private void ResetSeatedPos(Transform desiredHeadPos)
    {
        if ((steamCamera != null) && (cameraRig != null))
        {
            //ROTATION
            // Get current head heading in scene (y-only, to avoid tilting the floor)
            float offsetAngle = steamCamera.rotation.eulerAngles.y;
            // Now rotate CameraRig in opposite direction to compensate
            //cameraRig.Rotate(0f, -offsetAngle, 0f);

            //POSITION
            // Calculate postional offset between CameraRig and Camera
            Vector3 offsetPos = steamCamera.position - cameraRig.position;
            // Reposition CameraRig to desired position minus offset
            cameraRig.position = (desiredHeadPos.position - offsetPos);
        }
        else
        {
            Debug.Log("Error: SteamVR objects not found!");
        }
    }

    public virtual void DoTouchPadPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (desiredHeadPosition != null)
        {
            ResetSeatedPos(desiredHeadPosition);
        }
        else
        {
            Debug.LogError("Target Transform required. Assign in inspector.", gameObject);
        }
    }
}
