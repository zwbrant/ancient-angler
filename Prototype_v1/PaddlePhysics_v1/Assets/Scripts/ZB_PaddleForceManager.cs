using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZB_PaddleForceManager : MonoBehaviour {
    public Rigidbody forceTarget;
    public List<ZB_PaddleBlade> paddleBlades = new List<ZB_PaddleBlade>();
    [Range(0, 500000)]
    public float paddleThrustMultiplier = 5000f;
    [Range(0, 3000)]
    public float hapticIntensity = 0.4f;
    [Range(0, 100)]
    public float velocityDeadZone;
    public bool restrictOptimalVelocity;
    [Range (1, 300)]
    public float maxOptimalVelocity;
    [Range(0, 1)]
    public float velocityRestricter;


	// Use this for initialization
	void Start () {
	    foreach (ZB_PaddleBlade paddle in paddleBlades)
            paddle.associatedBoat = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        var rotation =  forceTarget.transform.rotation.eulerAngles;
        rotation.x = 0;
        rotation.z = 0;

        forceTarget.transform.eulerAngles = rotation;

        ApplyPaddleForces();
    }

    float totalPaddleForce = 0f;
    void ApplyPaddleForces()
    {
        foreach (ZB_PaddleBlade paddleBlade in paddleBlades)
        {
            if (paddleBlade.mainControlActions != null || paddleBlade.secondaryControlActions != null)
            {
                Vector3 paddleForce = CalculatePaddleForce(paddleBlade);
                if (paddleForce.magnitude > .01f)
                {
                    //print(paddleForce.magnitude);
                    totalPaddleForce += paddleForce.magnitude;

                    if (paddleBlade.forcePoint != null)
                        forceTarget.AddForceAtPosition(paddleForce * paddleThrustMultiplier, paddleBlade.forcePoint.transform.position);
                    else
                        forceTarget.AddForceAtPosition(paddleForce * paddleThrustMultiplier, paddleBlade.transform.position);

                    if (paddleBlade.mainControlActions != null)
                    {
                        float hapticFeedback = (paddleForce.magnitude * hapticIntensity);
                        paddleBlade.mainControlActions.TriggerHapticPulse((ushort)hapticFeedback);
                        if (paddleBlade.secondaryControlActions != null)
                            paddleBlade.secondaryControlActions.TriggerHapticPulse((ushort)(hapticFeedback * .33f));
                    }
                }
            }
            //if (totalPaddleForce > 0)
            //    print(totalPaddleForce);
            totalPaddleForce = 0f;
        }
    }

    public Vector3 CalculatePaddleForce(ZB_PaddleBlade paddleBlade)
    {
        float velocity = paddleBlade.Velocity;
        //print(velocity);
        if (velocity > velocityDeadZone)
        {
            float submergedArea = paddleBlade.SubmergedArea;
            if (submergedArea > 0)
            {
                Vector3 thrust = paddleBlade.RawForce;
                //Vector3 paddleDirection = paddleBlade.Direction;
                //Vector3 thrust = -paddleDirection * submergedArea * paddleThrust;
                //float angleOfImpact = Vector3.Angle(paddleBlade.transform.up, paddleDirection);

                //// normalize between the sides of the paddle
                //if (angleOfImpact > 90)
                //{
                //    angleOfImpact = 180 - angleOfImpact;
                //}
                //// normalized. the greater the angle of impact (obliqueness), the less the intensity 
                //float intensityOfImpact = 1f - (angleOfImpact * 0.0111111111111111f);
                //thrust *= (intensityOfImpact);

                Vector3 normalizedThrust = thrust.normalized;
                if (restrictOptimalVelocity && velocity > maxOptimalVelocity)
                    thrust = normalizedThrust * velocityRestricter;


                // *Debugging*
                //print("Paddle thrust: " + thrust * 2);
                //print("Intensity of impact: " + intensityOfImpact);
                //print("Paddle angle: " + angleOfImpact);
                //print("Paddle direction: " + paddleRSubmersion.Direction);
                //print("Paddle velocity: " + paddleRSubmersion.Velocity);

                //Debug.DrawLine(paddleRSubmersion.transform.position, paddleRSubmersion.transform.right, Color.yellow);
                //Debug.DrawLine(paddleRSubmersion.transform.position, -paddleRSubmersion.Direction, Color.green);

                //
                //forceLine.SetPositions(new Vector3[] { paddleRSubmersion.transform.position + paddleRSubmersion.transform.up,
                //                                       paddleRSubmersion.transform.position,
                //                                        paddleRSubmersion.transform.position + paddleRSubmersion.Direction * 10 });
                //forceLine.SetVertexCount(2);
                //forceLine.SetPositions(new Vector3[] { forcePointR.transform.position, currThrust });

                //if (currThrust.magnitude <= 0)
                //{
                //    forceLine.SetPositions(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 0) });
                //}
                //else {

                //}

                return thrust;
            }
        }
        return Vector3.zero;
    }
}
