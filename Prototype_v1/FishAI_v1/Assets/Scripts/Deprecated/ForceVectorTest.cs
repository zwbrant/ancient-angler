using UnityEngine;
using System.Collections;

public class ForceVectorTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    Rigidbody rBody;
    void FixedUpdate()
    {
        rBody.AddForce(new Vector3(0, 1, 0));
    }
}
