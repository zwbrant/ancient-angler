using UnityEngine;
using System.Collections;
using VRTK;

public class FollowRotation : MonoBehaviour
{
    public Transform FollowTarget;
    public bool OnlyCalculateRotation;
    public int FramesPerUpdate = 1;

    public bool Follow;

    internal float LastAngleDiff
    {
        get; private set;
    }

    private static Vector3 _localRefPos = new Vector3(-1f, 0f, 0f);
    private int _frameCount = 0;
    private Vector3 _localPointPosSum = Vector3.zero;
    private Vector3 _lastLocalPointPos = Vector3.zero;

    void Start()
    {
        LastAngleDiff = 0f;
    }

    //float angleDiffSum = 0f;
    void FixedUpdate()
    {
        if (Follow)
        {
            _frameCount++;

            Vector3 localPointPos = transform.InverseTransformPoint(FollowTarget.position);
            localPointPos.y = 0f;

            _localPointPosSum += localPointPos;
            _lastLocalPointPos = localPointPos;

            if (FramesPerUpdate == _frameCount)
            {
                Vector3 avgPos = _localPointPosSum / _frameCount;

                float angleDiff = Vector3.Angle(_localRefPos, localPointPos);

                float denormalizedAngle = DenormalizedAngle(_localRefPos, avgPos);

                // TODO consider flipping the reference point if the angle differnce becomes too big to avoid abrupt flips
                if (angleDiff > .001f)
                {
                    if (denormalizedAngle > 0)
                    {
                        //angleDiffSum += angleDiff * 1.2f;
                        if (!OnlyCalculateRotation)
                            transform.Rotate(0f, angleDiff, 0f);
                        LastAngleDiff = angleDiff;
                    }
                    else
                    {
                        //angleDiffSum = 0f;
                        if (!OnlyCalculateRotation)
                            transform.Rotate(0f, -angleDiff, 0f);
                        LastAngleDiff = -angleDiff;
                    }
                }
                else
                {
                    LastAngleDiff = 0f;
                }


                //angleDiffSum = 0f;
                _frameCount = 0;
                _localPointPosSum = Vector3.zero;
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

}
