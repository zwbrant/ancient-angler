using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LostPolygon.DynamicWaterSystem;
using System;

public class ZB_PaddleForceManager : MonoBehaviour {
    public Rigidbody forceTarget;
    public float ObliqueDragScale = 1f;
    public List<ZB_PaddleBlade> paddleBlades = new List<ZB_PaddleBlade>();
    [Range(0, 10000)]
    public float paddleThrustMultiplier = 2500f;
    [Range(0, 4000)]
    public float RedirectMulti = 2500f;
    [Range(0, 3000)]
    public float hapticIntensity = 0.4f;
    [Range(0, 100)]
    public float velocityDeadZone;
    public bool restrictOptimalVelocity;
    [Range (1, 300)]
    public float maxOptimalVelocity;
    [Range(0, 1)]
    public float velocityRestricter;
    public TextMesh DebugText;



	// Use this for initialization
	void Start () {
	    foreach (ZB_PaddleBlade paddle in paddleBlades)
            paddle.associatedBoat = this;

        buoyForce = forceTarget.GetComponent<BuoyancyForce>();
        initDrag = buoyForce.DragInFluid;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private BuoyancyForce buoyForce;
    private float initDrag;

    void FixedUpdate()
    {
        //var rotation =  forceTarget.transform.rotation.eulerAngles;
        //rotation.x = 0;
        //rotation.z = 0;

        //forceTarget.transform.eulerAngles = rotation;

        ProcessPaddleForces();

        AddObliqueDrag();

        RedirectInertia();
    }

    float totalPaddleForce = 0f;
    void ProcessPaddleForces()
    {
        foreach (ZB_PaddleBlade blade in paddleBlades)
        {
            if (blade.mainControlActions != null || blade.secondaryControlActions != null)
            {
                Vector3 paddleForce = CalculatePaddleForce(blade);
                if (paddleForce.magnitude > .01f)
                {
                    //print(paddleForce.magnitude);
                    totalPaddleForce += paddleForce.magnitude;

                    AddPaddleForce(paddleForce, blade);
                    float hapticFeedback = (paddleForce.magnitude * hapticIntensity);

                    if (blade.mainControlActions != null)
                    {
                        blade.mainControlActions.TriggerHapticPulse((ushort)hapticFeedback);

                    }
                    if (blade.secondaryControlActions != null)
                        blade.secondaryControlActions.TriggerHapticPulse((ushort)(hapticFeedback));
                }
            }

            totalPaddleForce = 0f;
        }
    }

    private Vector3 _boatForward = new Vector3(-1, 0, 0);
    private Vector3 _currForwardForce = Vector3.zero;
    void AddPaddleForce(Vector3 paddleForce, ZB_PaddleBlade blade)
    {
        //var velocity = forceTarget.velocity;

        //// convert a portion of the given force to a forward impulse
        //float dot = Vector3.Dot(forceTarget.transform.InverseTransformVector(paddleForce).normalized, _boatForward);
        //float obliqueness = Mathf.Abs(dot);

        //if (DebugText.gameObject.activeSelf)
        //    DebugText.text = Math.Round(dot, 2, MidpointRounding.AwayFromZero).ToString();

        //_currForwardForce = forceTarget.transform.TransformVector(_boatForward).normalized * dot * paddleThrustMultiplier * 5f;
        //forceTarget.AddForce(_currForwardForce);

        // add force
        if (blade.ForcePoint != null)
            forceTarget.AddForceAtPosition(paddleForce * paddleThrustMultiplier /** (1f - dot)*/, blade.ForcePoint.transform.position);
        else
            forceTarget.AddForceAtPosition(paddleForce * paddleThrustMultiplier /** (1f - dot)*/, blade.transform.position);

    }

    public Vector3 CalculatePaddleForce(ZB_PaddleBlade paddleBlade)
    {
        float velocity = paddleBlade.Speed;
        //print(velocity);
        if (velocity > velocityDeadZone)
        {
            float submergedArea = paddleBlade.SubmergedArea;
            if (submergedArea > 0)
            {
                Vector3 thrust = paddleBlade.RawForce;

                Vector3 normalizedThrust = thrust.normalized;
                if (restrictOptimalVelocity && velocity > maxOptimalVelocity)
                    thrust = normalizedThrust * velocityRestricter;

                return thrust;
            }
        }
        return Vector3.zero;
    }

    public void RedirectInertia()
    {
        var velocity = forceTarget.velocity;


        // find how obliquely the craft is striking the water
        float dot = Vector3.Dot(forceTarget.transform.InverseTransformVector(velocity).normalized, _boatForward);

        float factor = Mathf.Abs(dot);
        factor -= .5f;

        factor = (factor >= 0f) ? factor / .5f : factor / -.5f ;
        factor = 1f - factor;

        


        if (DebugText.gameObject.activeSelf)
            DebugText.text = Math.Round(dot, 2, MidpointRounding.AwayFromZero).ToString();

        if (factor < .05f || velocity.magnitude < .05f)
            return;

        // add 
        _currForwardForce = _boatForward * dot;
        if (dot > 0)
            forceTarget.AddRelativeForce(_boatForward * factor * RedirectMulti * velocity.magnitude);
        else
            forceTarget.AddRelativeForce(_boatForward * -factor * RedirectMulti * velocity.magnitude);

       // Debug.DrawLine(transform.TransformPoint(Vector3.forward), transform.TransformPoint)

        // counter-act inertia
        forceTarget.AddForce(-velocity * factor * RedirectMulti);


    }

    public void AddObliqueDrag()
    {
        var localDirection = forceTarget.transform.InverseTransformDirection(forceTarget.velocity).normalized;
        var obliqueness = Vector3.Dot(localDirection, new Vector3(-1, 0, 0));
        obliqueness = 1f - obliqueness;

        buoyForce.DragInFluid = initDrag + (initDrag * obliqueness * ObliqueDragScale);
    }

    //private void OnRenderObject()
    //{

    //    ZB_PaddleBlade.LineMaterial.SetPass(0);
    //    GL.PushMatrix();
    //    GL.Begin(GL.LINES);

    //    GL.Color(Color.blue);

    //    GL.Vertex(forceTarget.transform.TransformPoint(new Vector3(-1, 0, .1f)));
    //    GL.Vertex(forceTarget.transform.TransformPoint(new Vector3(-1, 0, .1f)) + _currForwardForce * 1f);



    //    GL.End();
    //    GL.PopMatrix();
    //}
}
