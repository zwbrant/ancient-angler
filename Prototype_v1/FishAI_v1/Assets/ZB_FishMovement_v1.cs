using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZB_FishMovement_v1 : MonoBehaviour {
    NavMeshAgent _navAgent;
    public GameObject NavTarget;

    float _initialBaseOffset;
    Terrain _terrain;

	// Use this for initialization
	void Start () {
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.destination = NavTarget.transform.position;
        _initialBaseOffset = _navAgent.baseOffset;
        _terrain = Terrain.activeTerrain;

    }
	
	// Update is called once per frame
	void Update () {
        _navAgent.baseOffset = Mathf.Lerp(_navAgent.baseOffset, _initialBaseOffset - _terrain.SampleHeight(transform.position), .5f);
        // = _initialBaseOffset - _terrain.SampleHeight(transform.position);
        //print(_terrain.SampleHeight(transform.position));
        _navAgent.destination = NavTarget.transform.position;
        
    }
}
