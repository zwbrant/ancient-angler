using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Fish))]
public class RbodyFishResistance : MonoBehaviour {
    public Rigidbody Rbody;
    public Vector3 PositionOfForceOnRbody;
    [Header("Automatic Force")]
    public bool EnableAutoForce = false;
    // Global position the fish will try to swim to
    public Vector3 FishPositionTarget;
    public Vector3 AutoForce;
    public float AutoForceMultiplier = 1f;
    public float YResistanceMultiplier = 1f;
    public AnimationCurve AutoForceCurve;
    public float CurveLengthInSeconds = 1f;
    [Header("Manual Curve")]
    public bool AllowManualCntrl = false;
    public float ManualForceMultiplier = 1f;

    public Vector3 CurrentForce { get; private set; }
    public float CurrentAutoForceCurve { get; private set; }
    public Fish Fish { get; private set; }
    private Vector3 GlobalPointOfForce { get { return transform.TransformPoint(PositionOfForceOnRbody); } }

    private float _curveTimer = 0f;
    private bool _isTimerIncreasing = true;

    private void Awake()
    {
        if (Rbody == null) { Rbody = GetComponent<Rigidbody>(); }
        if (Rbody == null)
        {
            Debug.LogException(new System.Exception("No rigidbody component assigned"));
        }
    }

    // Use this for initialization
    void Start () {
		
	}

    private void Update()
    {
        if (EnableAutoForce)
        {
            UpdateAutoForceCurve();
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (AllowManualCntrl)
        {
            UpdateManualInput();
        }

        if (EnableAutoForce)
        {
            //UpdateAutoForce();
            UpdateTargetPositionForce();
        }
    }

    private void UpdateManualInput()
    {
        float xInput = Input.GetAxis("Mouse X");
        float yInput = Input.GetAxis("Mouse Y");

        Vector3 localForce = Rbody.transform.TransformDirection(xInput, 0f, yInput)
            * Time.deltaTime * ManualForceMultiplier;

        Rbody.AddForceAtPosition(localForce * ManualForceMultiplier, GlobalPointOfForce, ForceMode.Acceleration);
    }

    private void UpdateAutoForce()
    {
        CurrentForce = (AutoForce * CurrentAutoForceCurve) * AutoForceMultiplier;

        Rbody.AddForceAtPosition(transform.TransformDirection(CurrentForce), GlobalPointOfForce, ForceMode.Acceleration);
       // Debug.DrawLine(GlobalPointOfForce, transform.TransformDirection(CurrentForce));
    }

    private void UpdateTargetPositionForce()
    {
        var vectorToTarget = FishPositionTarget - GlobalPointOfForce;
        vectorToTarget.y *= YResistanceMultiplier;
        vectorToTarget = Vector3.Normalize(vectorToTarget);
        Debug.DrawLine(GlobalPointOfForce, FishPositionTarget);

        CurrentForce = (vectorToTarget * CurrentAutoForceCurve) * AutoForceMultiplier;

        Rbody.AddForceAtPosition(CurrentForce, GlobalPointOfForce, ForceMode.Acceleration);
        //Debug.DrawRay(GlobalPointOfForce, transform.TransformDirection(CurrentForce), Color.red);
    }

    private void UpdateAutoForceCurve()
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

        CurrentAutoForceCurve = AutoForceCurve.Evaluate(_curveTimer / CurveLengthInSeconds);
    }
}
