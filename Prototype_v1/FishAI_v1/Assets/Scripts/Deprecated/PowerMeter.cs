using UnityEngine;
using System.Collections;

public class PowerMeter : MonoBehaviour {
    public CalculateSubmersion paddleRSubmersion;
    public float meterMultiplier;
    public float meterMax;
    Vector3 startPos;

	// Use this for initialization
	void Start () {
        startPos = transform.position;

	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(startPos.x + Mathf.Clamp(paddleRSubmersion.Velocity * meterMultiplier, 0, meterMax), startPos.y, startPos.z);
	}
}
