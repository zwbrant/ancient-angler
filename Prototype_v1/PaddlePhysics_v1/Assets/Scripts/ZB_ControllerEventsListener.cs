using UnityEngine;
using System.Collections;
using VRTK;
using UnityEngine.SceneManagement;

public class ZB_ControllerEventsListener : MonoBehaviour {
    VRTK_ControllerEvents controllerEvents;
    public GameObject attachPointGO;
    public Rigidbody targetRbody;
    public ZB_VerletLine verletLine;
    public ZB_FollowRotation followRotation;

	// Use this for initialization
	void Start () {
        controllerEvents = GetComponent<VRTK_ControllerEvents>();
        controllerEvents.ApplicationMenuPressed += new ControllerInteractionEventHandler(DoApplicationMenuPressed);
        controllerEvents.ApplicationMenuReleased += new ControllerInteractionEventHandler(DoApplicationMenuReleased);
        controllerEvents.TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
        controllerEvents.TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);
        controllerEvents.TouchpadPressed += new ControllerInteractionEventHandler(DoTouchPadPressed);
        controllerEvents.TouchpadReleased += new ControllerInteractionEventHandler(DoTouchpadReleased);

        controllerEvents.GripPressed += new ControllerInteractionEventHandler(DoGripPressed);

    }

    // Update is called once per frame
    void Update () {
	
	}

    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (verletLine != null) {
            verletLine.ReleaseLine = true;
        }
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (verletLine != null)
        {
            verletLine.ReleaseLine = false;
        }
    }

    private void DoTouchPadPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (verletLine != null)
        {
            verletLine.removeSegment = true;
        }
    }

    private void DoTouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        if (verletLine != null)
        {
            verletLine.removeSegment = false;
        }
    }

    private void DoApplicationMenuPressed(object sender, ControllerInteractionEventArgs e)
    {
    }

    private void DoApplicationMenuReleased(object sender, ControllerInteractionEventArgs e)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void DoGripPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (followRotation != null && followRotation == true)
            followRotation.follow = false;
    } 
}
