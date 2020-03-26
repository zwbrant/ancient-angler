using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZB_CollisionEvent : MonoBehaviour {
    public delegate void TriggerDelegate(Collider other);
    public event TriggerDelegate onTriggerEnter;
    public event TriggerDelegate onTriggerExit;

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
        if (onTriggerEnter != null)
            onTriggerEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (onTriggerExit != null)
            onTriggerExit(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (onCollision != null)
            onCollision(collision);
    }
}
