using UnityEngine;
using System.Collections;
using VRTK;


public class ZB_Reel : MonoBehaviour {
    public ZB_VerletLine verletLine;
    public ZB_FollowRotation handleRotation;
    public float reelingSpeedMulti = 1.3f;
    [Tooltip("in m/sec")]
    public float releaseSpeed = 1f;

    private ReelInputEvents reelInputEvents;
    internal VRTK_ControllerActions reelControllerActions;
    internal VRTK_ControllerActions rodControllerActions;

    internal ZB_HandleInteractObj handleInteractObjCallback;
    internal float initialReleaseSpeed;

    bool releaseLine = false;
    float handleRotationSum = 0f;

	void Start () {   
        reelInputEvents = new ReelInputEvents(this);
        StartCoroutine(reelInputEvents.InitEvents());
        verletLine.reel = this;
        initialReleaseSpeed = releaseSpeed;
    }
	
	void Update () {
        ReelLine();
        if (releaseLine)
        {
            releaseTimer += Time.deltaTime;
            ReleaseLine();
        }
    }

    void FixedUpdate()
    {

    }

    bool firstRelease = false;
    float releaseTimer = 0f;
    internal void ReleaseLine()
    {
        if (releaseTimer >= 1f / (releaseSpeed / verletLine.segmentLength) || firstRelease)
        {
            for (int i = 0; i < 2; i++)
                verletLine.AddSegment();
            //if (rodControllerActions != null)
                //rodControllerActions.TriggerHapticPulse((ushort)200);
            releaseTimer = 0f;
            firstRelease = false;
        }
    }

    internal void ReelLine()
    {
        if (handleRotation.follow)
        {
            if (handleRotation.LastAngleDiff > 0)
            {
                handleRotationSum += handleRotation.LastAngleDiff + (handleRotation.LastAngleDiff * (handleRotation.LastAngleDiff * reelingSpeedMulti));
            }
            else
            {
                handleRotationSum = 0f;
            }

            if (handleRotationSum > 50f)
            {
                if (reelControllerActions != null)
                    reelControllerActions.TriggerHapticPulse((ushort)1500);

                verletLine.RemoveSegment();

                handleRotationSum = 0f;
            }
        }
    }

    public void DelayControllerRelease()
    {
        StartCoroutine(DelayControllerReleaseEnum());
    }

    IEnumerator DelayControllerReleaseEnum()
    {
        yield return new WaitForSeconds(.1f);
        handleInteractObjCallback.held = false;
    }

    public class ReelInputEvents : ZB_ControllerInputEvents
    {
        ZB_Reel reel;

        public ReelInputEvents(ZB_Reel reel)
        {
            this.reel = reel;
        }

        public override void DoGripPressed(object sender, ControllerInteractionEventArgs e)
        {
            base.DoGripPressed(sender, e);
        
            if (reel.handleRotation.follow)
            {
                reel.handleRotation.follow = false;
                reel.handleRotationSum = 0f;
                reel.reelControllerActions.TriggerHapticPulse((ushort)5000);
                reel.DelayControllerRelease();
            }
        }

        public override void DoGripReleased(object sender, ControllerInteractionEventArgs e)
        {

        }

        public override void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
        {
            base.DoTriggerPressed(sender, e);

            reel.releaseSpeed = reel.initialReleaseSpeed;
            reel.firstRelease = true;
            reel.releaseLine = true;
            //reel.verletLine.AddSegment();
        }

        public override void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
        {
            base.DoTriggerReleased(sender, e);

            if (reel.rodControllerActions != null)
                reel.rodControllerActions.TriggerHapticPulse((ushort)7000);

            reel.releaseTimer = 0f;
            reel.releaseLine = false;
        }
    }
}
