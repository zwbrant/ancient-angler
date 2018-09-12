using UnityEngine;
using System.Collections;

public class TestBoatController : MonoBehaviour {
    Rigidbody rBody;
    [Range(0, 1000)]
    public float speed = 10;

	// Use this for initialization
	void Start () {
        rBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
            rBody.AddRelativeForce(Vector3.forward * speed);
        if (Input.GetKey(KeyCode.A))
            rBody.AddRelativeForce(Vector3.left * speed);
        if (Input.GetKey(KeyCode.D))
            rBody.AddRelativeForce(Vector3.right * speed);
        if (Input.GetKey(KeyCode.S))
            rBody.AddRelativeForce(Vector3.back * speed);
    }
}
