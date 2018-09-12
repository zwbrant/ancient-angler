using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZB_SwimToTarget_v1 : MonoBehaviour {
    public GameObject Target;
    public float SwimSpeed = 1f;
    public float MaxChaseDist = 3f;

    bool _chase;

	// Use this for initialization
	void Start () {
        _chase = false;
	}
	
	// Update is called once per frame
	void Update () {

        if (_chase && Target != null)
        {
            float targetDistance = Vector3.Distance(transform.position, Target.transform.position);
            if (targetDistance < MaxChaseDist)
                ChaseTarget();
        }
	}

    void ChaseTarget()
    {
        float step = SwimSpeed * Time.deltaTime;
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, Target.transform.position, step);

        if (nextPosition.y < 0f)
        {
            transform.position = nextPosition;
        }

        transform.LookAt(Target.transform);
    }

    void OnTriggerEnter(Collider other)
    {
        Target = other.gameObject;
        _chase = true;
    }
}
