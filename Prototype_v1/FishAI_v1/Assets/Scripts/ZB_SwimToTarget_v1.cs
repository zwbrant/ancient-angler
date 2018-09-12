using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZB_SwimToTarget_v2 : MonoBehaviour {
    public float SwimSpeed = 1f;
    public float MaxChaseDist = 3f;
    public float MaxSwimHeight = -0.05f;
    public Transform[] Waypoints;
    public float LinePullStrength; 
    public ZB_CollisionEvent MouthCollisions;
    public GameObject HookPoint;

    // when patrolling
    int _targetWaypoint;
    // when chasing
    GameObject _targetHook;
    ZB_VerletLine _targetLine;
    NavMeshAgent _navAgent;
    bool _chase;
    bool _hooked;
    LineRenderer _hookLineRend;

    float _initSpeed;

	// Use this for initialization
	void Start () {
        _chase = false;
        _navAgent = GetComponent<NavMeshAgent>();
        _initSpeed = _navAgent.speed;
        _hookLineRend = HookPoint.GetComponent<LineRenderer>();
        _hookLineRend.enabled = false;
        MouthCollisions.onTrigger += MouthTriggerEnter;
	}
	
	// Update is called once per frame
	void Update () {

        if (_chase && _targetHook != null)
        {
            float targetDistance = Vector3.Distance(transform.position, _targetHook.transform.position);
            if (targetDistance < MaxChaseDist)
                ChaseTarget();
            else
                _chase = false;
        } else if (!_chase && !_hooked)
        {
            if (_navAgent.remainingDistance < 0.5f)
                GotoNextPoint();
        }

        if (_hooked)
        {
            //if (_targetLine.hookStretchLength > 0f)
            //{
            //    float lineStretchLength = Vector3.Distance(transform.position, _hookLine.transform.position);
            //    transform.position = Vector3.MoveTowards(transform.position, _targetHook.transform.position,
            //        lineStretchLength * LinePullStrength * Time.deltaTime);
            //}

            print(_targetLine.hookDelta);
            transform.position -= _targetLine.hookDelta;

            if (_hookLineRend.enabled && _targetHook != null)
            {
                _hookLineRend.SetPosition(0, HookPoint.transform.position);
                _hookLineRend.SetPosition(1, _targetHook.transform.position);
            }

        }
	}

    void ChaseTarget()
    {
        float step = SwimSpeed * Time.deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, _targetHook.transform.position, step);

        if (nextPosition.y < MaxSwimHeight)
        {
            transform.position = nextPosition;
        } else
        {
            nextPosition.y = MaxSwimHeight - .01f;
            transform.position = nextPosition;
        }


        transform.LookAt(_targetHook.transform);
    }

    void GotoNextPoint()
    {
        if (Waypoints.Length == 0)
            return;

        _navAgent.destination = Waypoints[_targetWaypoint].position;

        _targetWaypoint = (_targetWaypoint + 1) % Waypoints.Length;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hook")
        {
            _navAgent.enabled = false;
            _chase = true;
            _targetHook = other.gameObject;

        }
    }

    void MouthTriggerEnter(Collider other)
    {
        if (!_hooked)
        {

            //other.transform.parent = transform;
            _targetLine = other.GetComponent<ZB_VerletLine>();
            _targetLine.hooked = true;
            //_targetLine.lineStartRBody.isKinematic = true;

            _hookLineRend.enabled = true;
            _hooked = true;
            _chase = false;
            //_navAgent.enabled = true;
            //GotoNextPoint();
        }
    }
}
