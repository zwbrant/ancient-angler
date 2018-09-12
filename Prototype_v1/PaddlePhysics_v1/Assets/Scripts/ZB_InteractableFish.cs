using UnityEngine;
using System.Collections;
using VRTK;

public class ZB_InteractableFish : VRTK_InteractableObject {
    ZB_HookAttach hookAttach;

	// Use this for initialization
	protected override void Start () {
        hookAttach = GetComponent<ZB_HookAttach>();
	}

    // Update is called once per frame
    protected override void Update () {
	
	}

    public override void Grabbed(GameObject grabbingObject)
    {
        base.Grabbed(grabbingObject);

        Destroy(hookAttach.currJoint);
        hookAttach.currJoint = null;
    }
}
