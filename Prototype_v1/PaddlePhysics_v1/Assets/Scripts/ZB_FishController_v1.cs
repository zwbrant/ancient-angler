using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZB_FishController_v1 : MonoBehaviour {
    public GameObject HookPoint;
    public ZB_VerletLine AttachedLine;
    internal LineRenderer lineRenderer;

    // Use this for initialization
    void Start () {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
	}
	
	// Update is called once per frame
	void Update () {
        if (AttachedLine.hooked)
        {
            if (!lineRenderer.enabled)
                lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, HookPoint.transform.position);
            lineRenderer.SetPosition(1, AttachedLine.transform.position);
        } else
        {
            lineRenderer.enabled = false;
        }

    }
}
