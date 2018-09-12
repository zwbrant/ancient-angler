using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {
    public bool XAxis;
    public bool YAxis;
    public bool ZAxis;
    public float Speed; 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float speed = Speed * Time.deltaTime;

        if (XAxis)
            transform.Rotate(new Vector3(speed, 0f, 0f));
        if (YAxis)
            transform.Rotate(new Vector3(0f, speed, 0f));
        if (ZAxis)
            transform.Rotate(new Vector3(0f, 0f, speed));
    }
}
