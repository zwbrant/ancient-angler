using UnityEngine;
using System.Collections;

public class ZB_IndirectBoatCollider : MonoBehaviour {
    public ZB_PaddleBlade paddle;

    internal Rigidbody rBody;
    internal static float impactForceMulti = 120f;
    internal static float continuousForceMulti = 170f;
    internal Transform forcePoint;

	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();
        if (paddle.forcePoint != null)
            forcePoint = paddle.forcePoint.transform;
        else
            print(string.Format("Warning paddle {0} has no forcePoint", paddle.name));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer
            == 14 || collision.gameObject.layer == 15)
        {
            rBody.velocity = Vector3.zero;
            if (forcePoint != null)
                paddle.associatedBoat.forceTarget.AddForceAtPosition(collision.relativeVelocity * impactForceMulti, forcePoint.position, ForceMode.Impulse);
            else
                paddle.associatedBoat.forceTarget.AddForce(collision.relativeVelocity * impactForceMulti, ForceMode.Impulse);

            if (paddle.mainControlActions != null)
                paddle.mainControlActions.TriggerHapticPulse((ushort)(collision.relativeVelocity.magnitude * 700f));
        }
    }

    void OnCollisionStay(Collision collisionInfo)
    {   
        if (collisionInfo.gameObject.layer == 14 || collisionInfo.gameObject.layer == 15)
        {
            rBody.velocity = Vector3.zero;

            if (forcePoint != null)
                paddle.associatedBoat.forceTarget.AddForceAtPosition(collisionInfo.relativeVelocity * continuousForceMulti, forcePoint.position);
            else
                paddle.associatedBoat.forceTarget.AddForce(collisionInfo.relativeVelocity * continuousForceMulti);

            if (paddle.mainControlActions != null)
                paddle.mainControlActions.TriggerHapticPulse((ushort)(collisionInfo.relativeVelocity.magnitude * 700f));
        }
    }
}
