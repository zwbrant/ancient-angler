using UnityEngine;
using System.Collections;

public class ZB_RigidbodyFollow : MonoBehaviour {
    public static Quaternion zeroRotation = new Quaternion();
    public Transform transformToFollow;
    public float xOffset = 0, yOffset = 0, zOffset = 0;


    void Start ()
    {

    }

	void Update () {
        transform.position = transformToFollow.position + new Vector3(xOffset, yOffset, zOffset);
        transform.rotation = transformToFollow.rotation;
    }
}
