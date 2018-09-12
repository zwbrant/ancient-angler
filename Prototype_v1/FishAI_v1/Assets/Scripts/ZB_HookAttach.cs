using UnityEngine;
using System.Collections;

public class ZB_HookAttach : MonoBehaviour {
    Rigidbody rBody;

	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    internal FixedJoint currJoint = null;

    void OnCollisionEnter(Collision collision)
    {
        GameObject gameObj = collision.gameObject;
        if (collision.gameObject.tag == "Hook" && currJoint == null)
        {
            currJoint = gameObj.AddComponent<FixedJoint>();
            currJoint.connectedBody = rBody;
        }

    }
}
