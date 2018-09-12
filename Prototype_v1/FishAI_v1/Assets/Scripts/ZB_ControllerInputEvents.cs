using UnityEngine;
using System.Collections;
using VRTK;
using UnityEngine.SceneManagement;

public class ZB_ControllerInputEvents : MonoBehaviour {

    void Start()
    {
        StartCoroutine(InitEvents());
    }

    public IEnumerator InitEvents()
    {
        yield return new WaitUntil(CheckForControllers);
        VRTK_ControllerEvents controllerEventsL = ZB_SceneSingletons.ControllerEventsL;
        VRTK_ControllerEvents controllerEventsR = ZB_SceneSingletons.ControllerEventsR;

        controllerEventsL.ApplicationMenuPressed += new ControllerInteractionEventHandler(DoApplicationMenuPressed);
        controllerEventsL.ApplicationMenuReleased += new ControllerInteractionEventHandler(DoApplicationMenuReleased);
        controllerEventsL.TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
        controllerEventsL.TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);
        controllerEventsL.TouchpadPressed += new ControllerInteractionEventHandler(DoTouchPadPressed);
        controllerEventsL.TouchpadReleased += new ControllerInteractionEventHandler(DoTouchpadReleased);
        controllerEventsL.GripPressed += new ControllerInteractionEventHandler(DoGripPressed);
        controllerEventsL.GripReleased += new ControllerInteractionEventHandler(DoGripReleased);

        controllerEventsR.ApplicationMenuPressed += new ControllerInteractionEventHandler(DoApplicationMenuPressed);
        controllerEventsR.ApplicationMenuReleased += new ControllerInteractionEventHandler(DoApplicationMenuReleased);
        controllerEventsR.TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
        controllerEventsR.TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);
        controllerEventsR.TouchpadPressed += new ControllerInteractionEventHandler(DoTouchPadPressed);
        controllerEventsR.TouchpadReleased += new ControllerInteractionEventHandler(DoTouchpadReleased);
        controllerEventsR.GripPressed += new ControllerInteractionEventHandler(DoGripPressed);
        controllerEventsR.GripReleased += new ControllerInteractionEventHandler(DoGripReleased);

    }

    public static bool CheckForControllers()
    {
        if (ZB_SceneSingletons.ControllerEventsL != null && ZB_SceneSingletons.controllerR != null)
            return true;
        else
            return false;
    }

    public virtual void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {

    }

    public virtual void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {

    }

    public virtual void DoTouchPadPressed(object sender, ControllerInteractionEventArgs e)
    {

    }

    public virtual void DoTouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {

    }

    public virtual void DoApplicationMenuPressed(object sender, ControllerInteractionEventArgs e)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public virtual void DoApplicationMenuReleased(object sender, ControllerInteractionEventArgs e)
    {

    }

    public virtual void DoGripPressed(object sender, ControllerInteractionEventArgs e)
    {

    }

    public virtual void DoGripReleased(object sender, ControllerInteractionEventArgs e)
    {

    }

}
