using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class InputEventsTest : MonoBehaviour {
    [Range(0, 3)]
    public float HapticStrength;

    public VRTK_ControllerEvents CntrlEvents;
    public VRTK_InteractGrab InteractGrab;
    public VRTK_ControllerReference CntrlReference;


    // Use this for initialization
    void Start () {
        CntrlEvents = UnityUtilities.TryResolveDependency(CntrlEvents, gameObject);
        InteractGrab = UnityUtilities.TryResolveDependency(InteractGrab, gameObject);

        Invoke("GetControllerReference", 2f);

    }

    private void GetControllerReference()
    {
        CntrlReference = VRTK_ControllerReference.GetControllerReference(CntrlEvents.gameObject);

    }

    private void OnGUI()
    {
        if (EzGui.Button("HAPTIC PULSE", 0, 0, EzCorner.UpperRight))
        {
            VRTK_ControllerHaptics.TriggerHapticPulse(CntrlReference, 33f, 1, .01f);

        }


    }

    // Update is called once per frame
    void FixedUpdate () {
        //if (InteractGrab.IsGrabButtonPressed())
        //      {
        //          print("DOGGYTIME");
        //      }

        //VRTK_ControllerHaptics.TriggerHapticPulse(CntrlReference, HapticStrength);

    }
}
