using UnityEngine;
using System.Collections;
using VRTK;

public class ZB_FollowRotation : MonoBehaviour {
    //public VRTK_InteractGrab grabScript;
    //FollowRotationInputEvents controllerInputEvents;

    public Transform followPoint;
    public bool onlyCalculateRotation;
    //Rigidbody rBody;
    internal VRTK_ControllerActions controllerActions;

    LineRenderer lineRenderer;

    //public ZB_VerletLine verletLine;
    public bool follow;

    internal float LastAngleDiff
    {
        get; set;
    }

	void Start () {
        //followPoint = grabScript.gameObject.transform;
        //rBody = GetComponent<Rigidbody>();
        //if (verletLine != null)
        //{
        //    controllerInputEvents = new FollowRotationInputEvents(this);
        //    StartCoroutine(controllerInputEvents.InitEvents());
        //}
        LastAngleDiff = 0f;
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update () {

    }

    static Vector3 localRefPos = new Vector3(-1f, 0f, 0f);

    public int framesPerUpdate = 1;
    int frameCount = 0;
    Vector3 localPointPosSum = Vector3.zero;
    Vector3 lastLocalPointPos = Vector3.zero;
    //float angleDiffSum = 0f;
    void FixedUpdate()
    {
        if (follow)
        {
            frameCount++;

            Vector3 localPointPos = transform.InverseTransformPoint(followPoint.position);
            localPointPos.y = 0f;

            localPointPosSum += localPointPos;
            lastLocalPointPos = localPointPos;

            if (framesPerUpdate == frameCount)
            {
                Vector3 avgPos = localPointPosSum / frameCount;

                float angleDiff = Vector3.Angle(localRefPos, localPointPos);

                float denormalizedAngle = DenormalizedAngle(localRefPos, avgPos);

                //float angleDiff = angleDiffSum / frameCount;

                //if (lineRenderer != null)
                //    lineRenderer.SetPositions(new Vector3[] { transform.TransformPoint(avgPos), this.transform.position, transform.TransformPoint(localRefPos) });

                //print(angleDiff);
                // TODO consider flipping the reference point if the angle differnce becomes too big to avoid abrupt flips
                if (angleDiff > .001f)
                {
                    if (denormalizedAngle > 0)
                    {
                        //angleDiffSum += angleDiff * 1.2f;
                        if (!onlyCalculateRotation)
                            transform.Rotate(0f, angleDiff, 0f);
                        LastAngleDiff = angleDiff;
                    }
                    else
                    {
                        //angleDiffSum = 0f;
                        if (!onlyCalculateRotation)
                            transform.Rotate(0f, -angleDiff, 0f);
                        LastAngleDiff = -angleDiff;
                    }
                }
                else
                {
                    LastAngleDiff = 0f;
                }


                //angleDiffSum = 0f;
                frameCount = 0;
                localPointPosSum = Vector3.zero;
            }
        }
    }

    public static float CalculateAngle(Vector3 from, Vector3 to)
    {
        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.x;
    }

    public static float DenormalizedAngle(Vector3 referenceForward, Vector3 newPosition)
    {
        //Vector3 referenceForward = new Vector3(0f, 0f, 1f);/* some vector that is not Vector3.up */
                                                           // the vector perpendicular to referenceForward (90 degrees clockwise)
                                                           // (used to determine if angle is positive or negative)
        Vector3 referenceRight = Vector3.Cross(Vector3.up, referenceForward);
        // the vector of interest
        //Vector3 newDirection = localPointPos;           /* some vector that we're interested in */
                                                        // Get the angle in degrees between 0 and 180
        float angle = Vector3.Angle(newPosition, referenceForward);
        // Determine if the degree value should be negative.  Here, a positive value
        // from the dot product means that our vector is on the right of the reference vector   
        // whereas a negative value means we're on the left.
        float sign = Mathf.Sign(Vector3.Dot(newPosition, referenceRight));
        float finalAngle = sign * angle;
        return finalAngle;
    }

    //public class FollowRotationInputEvents : ZB_ControllerInputEvents
    //{
    //    ZB_FollowRotation followRotation;

    //    public FollowRotationInputEvents(ZB_FollowRotation followRotation)
    //    {
    //        this.followRotation = followRotation;
            
    //    }

    //    public override void DoGripPressed(object sender, ControllerInteractionEventArgs e)
    //    {
    //        base.DoGripPressed(sender, e);
    //        if (followRotation.follow == true)
    //        {
    //            followRotation.follow = false;
    //            followRotation.controllerActions.TriggerHapticPulse((ushort)4000);
    //        }
    //    }
    //}
}
