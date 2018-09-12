using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZB_FishResistance_v1 : MonoBehaviour {
    public ZB_VerletLine AttachedLine;
    [Range (0, 1)]
    public float BaseResistance;

	// Use this for initialization
	void Start () {
        AttachedLine.fishResistance = this;
	}
	
	// Update is called once per frame
	void Update () {
        float currLineStretch = AttachedLine.hookStretchLength;


	}
}
