using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamTrack : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 3f;
    public float rotationSpeed = 3f;

    float distance;
    Vector3 position;
    Vector3 newPos;
    Quaternion rotation;
    Quaternion newRot;


    // Use this for initialization
    void Start()
    {
        distance = transform.position.y - target.position.y;
        position = new Vector3(target.position.x, target.position.y + distance, target.position.z);
        rotation = Quaternion.Euler(new Vector3(90, target.rotation.eulerAngles.y, 0f));
    }

    void FixedUpdate()
    {
        if (target)
        {
            this.transform.LookAt(target);
        }
    }
}