using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;

public class NonRbody_GrabAttach : VRTK_BaseGrabAttach
{
    protected override void Initialise()
    {
        tracked = false;
        climbable = false;
        kinematic = true;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void ProcessFixedUpdate()
    {
    }
}
