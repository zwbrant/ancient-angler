using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum FishState
{
    NavAgent,
    Chasing,
    Hooked
}

public class ZB_SwimToTarget_v1 : MonoBehaviour {
    public float SwimSpeed = 1f;
    public float MaxChaseDist = 3f;
    public Transform[] Waypoints;
    public float LinePullStrength;
    public ZB_CollisionEvent ChaseTrigger;
    public ZB_CollisionEvent MouthTrigger;
    public GameObject HookPoint;
    internal Rigidbody hookPointRBody;
    internal Rigidbody rBody;

    [Range(0f, 500f)]
    public float VerticalResistance = 1f;
    public float TargetDepth = -1f;

    public FishState StartState;

    private FishState currState;
    public FishState CurrState
    {
        get { return currState; }
        set
        {
            SetState(value);
            currState = value;
        }
    }

    // when patrolling
    int targetWaypoint;

    [Tooltip("Only needs to be set if you want the fish to chase a line on start")]
    public ZB_VerletLine TargetLine;
    NavMeshAgent navAgent;

    float initSpeed;

	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();
        hookPointRBody = HookPoint.GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();
        initSpeed = navAgent.speed;
        MouthTrigger.onTriggerEnter += OnMouthTriggerEnter;
        ChaseTrigger.onTriggerEnter += OnChaseTriggerEnter;
        ChaseTrigger.onTriggerExit += OnChaseTriggerExit;

        CurrState = StartState;
	}
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        //if (currState == FishState.Hooked)
        //{

        //    ApplyVerticalResistance();
        //}
        //else if ((targetLine == null || !targetLine.hooked) && leaderLineRend.enabled)
        //{
        //    CurrState = FishState.NavAgent;
        //}

        if (CurrState == FishState.Chasing)
        {
            if (TargetLine != null)
            {
                float targetDistance = Vector3.Distance(transform.position, TargetLine.lineStart.transform.position);
                if (targetDistance < MaxChaseDist)
                    ChaseTarget();
                else
                    CurrState = FishState.NavAgent;
            } 
        }
        else if (CurrState == FishState.NavAgent)
        {
            if (navAgent.remainingDistance < 0.5f && timer > .2f)
                GotoNextPoint();
        }
	}

    float timer = 0f; 

    private void SetState(FishState state)
    {
        switch (state)
        {
            case FishState.NavAgent:
                //ZB_SceneSingletons.debugText2.text = "Switching to NavAgent state";
                navAgent.enabled = true;
                rBody.isKinematic = true;
                TargetLine = null;
                break;
            case FishState.Chasing:
                //ZB_SceneSingletons.debugText2.text = "Switching to Chasing state";
                navAgent.enabled = false;
                rBody.isKinematic = true;
                break;
            case FishState.Hooked:
               // ZB_SceneSingletons.debugText2.text = "Switching to Hooked state";
                navAgent.enabled = false;
                rBody.isKinematic = false;
                break;
        }
    }

    //void ApplyVerticalResistance()
    //{
    //    float yPosition = transform.position.y;

    //    if (yPosition < ZB_SceneSingletons.WaterLevel)
    //    {
    //        if (yPosition > TargetDepth)
    //        {
    //            hookPointRBody.AddForce(-Vector3.up * VerticalResistance * Time.deltaTime * 100f, ForceMode.Force);
    //            //print(-Vector3.up * VerticalResistance * Time.deltaTime);
    //        }
    //        else if (yPosition < TargetDepth)
    //        {
    //            hookPointRBody.AddForce(Vector3.up * VerticalResistance * Time.deltaTime * 100f, ForceMode.Force);
    //            //print(Vector3.up * VerticalResistance * Time.deltaTime);
    //        }
    //    }
    //}

    void ChaseTarget()
    {
        //if (TargetLine.lineStart.transform.position.y < ZB_SceneSingletons.WaterLevel)
        //{
            float step = SwimSpeed * Time.deltaTime;
            Vector3 nextPosition = Vector3.MoveTowards(transform.position, TargetLine.lineStart.transform.position, step);

            transform.position = nextPosition;
            transform.LookAt(TargetLine.lineStart.transform);
        //}
        //else
        //{
        //    //CurrState = FishState.NavAgent;
        //}
    }

    void GotoNextPoint()
    {

        if (Waypoints.Length == 0)
            return;
        timer = 0f;

        navAgent.destination = Waypoints[targetWaypoint].position;

        targetWaypoint = (targetWaypoint + 1) % Waypoints.Length;

    }

    void OnChaseTriggerEnter(Collider other)
    {
        if (other.tag == "Hook")
        {
            TargetLine = other.GetComponent<ZB_VerletLine>();
            CurrState = FishState.Chasing;
        }
    }

    void OnChaseTriggerExit(Collider other)
    {
        CurrState = FishState.NavAgent;
        TargetLine = null;
    }

    void OnMouthTriggerEnter(Collider other)
    {
        print(other.name);
        if (TargetLine == null || !TargetLine.hooked)
        {
            CurrState = FishState.Hooked;
            SetHook();
        }
    }

    void SetHook()
    {
        //ZB_SceneSingletons.debugText1.text = "Hook attached";

        //TargetLine.hooked = true;
        //TargetLine.Attachment = hookPointRBody;
        //TargetLine.LineBreak += OnLineBreak;

        rBody.useGravity = true;
        var joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = TargetLine.lineStartRBody;
        ChaseTrigger.gameObject.SetActive(false);

        this.enabled = false;
    }

    private void OnLineBreak(object sender, EventArgs e)
    {
        CurrState = FishState.NavAgent;
    }

}
