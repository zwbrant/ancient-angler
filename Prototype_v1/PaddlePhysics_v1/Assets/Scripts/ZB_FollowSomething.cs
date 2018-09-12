using UnityEngine;
using System.Collections;

public class ZB_FollowSomething : MonoBehaviour {
    public GameObject objToFollow;
    public float yOffset, xOffset, zOffset = 0;
    public bool followRotation;
    public bool followX, followY, followZ = true;




    Vector3 offsetOfObjs;
    PaddleForce boatPaddleForce;
    Quaternion rotationDiff;

	// Use this for initialization
	void Start () {
        //rBodyToFollow = objToFollow.GetComponent<Rigidbody>();
        offsetOfObjs = objToFollow.transform.position - transform.position;
        //if (followRotation)
            //rotationDiff = Quaternion.Inverse(objToFollow.transform.rotation);
        
	}
	
	// Update is called once per frame
	void Update () {
        //transform.position = Vector3.MoveTowards(transform.position, objToFollow.transform.position + new Vector3(xOffset, yOffset, zOffset), 2f);
        //if (followRotation)
        //{
        //    Vector3 targAngles = objToFollow.transform.rotation.eulerAngles;
        //    Vector3 currAngles = transform.rotation.eulerAngles;
        //    float xAngle = (followX) ? targAngles.x - currAngles.x : currAngles.x;
        //    float yAngle = (followY) ? targAngles.y - currAngles.y : currAngles.y;
        //    float zAngle = (followZ) ? targAngles.z - currAngles.z : currAngles.z;

        //    transform.Rotate(new Vector3(xAngle, yAngle, zAngle));
        //    //transform.rotation = Quaternion.Euler(new Vector3(xAngle,
        //    //                                                    yAngle,
        //    //                                                    zAngle));
        //}
    }

    void FixedUpdate()
    {

  
        transform.position = Vector3.MoveTowards(transform.position, objToFollow.transform.position + new Vector3(xOffset, yOffset, zOffset), 2f);
        transform.rotation = objToFollow.transform.rotation;
        //if (followRotation)
        //{
        //    Vector3 targAngles = objToFollow.transform.rotation.eulerAngles;
        //    Vector3 currAngles = transform.rotation.eulerAngles;
        //    float xAngle = (followX) ? targAngles.x - currAngles.x : currAngles.x;
        //    float yAngle = (followY) ? targAngles.y - currAngles.y : currAngles.y;
        //    float zAngle = (followZ) ? targAngles.z - currAngles.z : currAngles.z;

        //    transform.Rotate(new Vector3(xAngle, yAngle, 0f) * .5f);
        //    //transform.rotation = Quaternion.Euler(new Vector3(xAngle,
        //    //                                                    yAngle,
        //    //                                                    zAngle));
        //}
    }
}
