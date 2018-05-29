using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

public class GlobalInputEvents : MonoBehaviour
{


    public VRTK_ControllerEvents CntrlEvents;
    public VRTK_InteractGrab InteractGrab;
    public VRTK_ControllerReference CntrlReference;


    // Use this for initialization
    void Start()
    {
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


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (CntrlEvents.buttonTwoPressed)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        //VRTK_ControllerHaptics.TriggerHapticPulse(CntrlReference, HapticStrength);

    }
}
