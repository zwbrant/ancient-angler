using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZB_CollisionEvent : MonoBehaviour {
    public delegate void TriggerDelegate(Collider other);
    public event TriggerDelegate onTrigger;

    public delegate void CollisionDelegate(Collision collision);
    public event CollisionDelegate onCollision;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        onTrigger(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        onCollision(collision);
    }
}
