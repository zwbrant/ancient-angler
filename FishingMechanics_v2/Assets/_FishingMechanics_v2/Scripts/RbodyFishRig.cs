using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RbodyFishRig : MonoBehaviour
{
    public FishingLine FishingLine;
    public Rigidbody Rbody;
    public Vector3 LocalPosistionOfForce;
    [Header("Automatic Force")]
    public bool EnableAutoForce = true;
    public Vector3 AutoForce;
    public float AutoForceMultiplier = 50f;
    public AnimationCurve AutoForceCurve;
    public float CurveLengthInSeconds = 1f;
    [Header("Manual Curve")]
    public bool AllowManualCntrl = false;
    public float ManualForceMultiplier = 200f;

    public Vector3 CurrentFishForce { get; private set; }

    private float _curveTimer = 0f;
    private bool _isTimerIncreasing = true;

    void Start()
    {
        if (Rbody == null) { Rbody = GetComponent<Rigidbody>(); }
    }

    void Update()
    {
        if (AllowManualCntrl)
        {
            UpdateManualInput();
        }

        if (EnableAutoForce)
        {
            UpdateAutoForce();
        }
 
    }

    private void UpdateManualInput()
    {
        float xInput = Input.GetAxis("Mouse X");
        float yInput = Input.GetAxis("Mouse Y");

        Vector3 localForce = Rbody.transform.TransformDirection(xInput, 0f, yInput)
            * Time.deltaTime * ManualForceMultiplier;

        Rbody.AddForceAtPosition(localForce * ManualForceMultiplier * Time.deltaTime, transform.TransformPoint(LocalPosistionOfForce), ForceMode.Acceleration);
    }

    private void UpdateAutoForce()
    {
        float curvePower = UpdateAutoForceCurve();

        CurrentFishForce = (AutoForce * curvePower) * AutoForceMultiplier * Time.deltaTime;

        Rbody.AddForceAtPosition(CurrentFishForce, Rbody.transform.TransformPoint(LocalPosistionOfForce), ForceMode.Acceleration);
    }

    private float UpdateAutoForceCurve()
    {
        if (_curveTimer >= CurveLengthInSeconds)
            _isTimerIncreasing = false;
        else if (_curveTimer <= 0f)
            _isTimerIncreasing = true;

        if (_isTimerIncreasing)
        {
            _curveTimer += Time.deltaTime;
        }
        else
        {
            _curveTimer -= Time.deltaTime;
        }

        return AutoForceCurve.Evaluate(_curveTimer / CurveLengthInSeconds);
    } 

    private void DrawDebugForceLines(Vector3 lineForce, Vector3 fishForce)
    {

    }


}
