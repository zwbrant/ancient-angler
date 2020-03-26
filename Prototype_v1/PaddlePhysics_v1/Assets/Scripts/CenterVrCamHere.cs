using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterVrCamHere : MonoBehaviour {
    [Tooltip("Defaults to MainCam")]
    public Camera VrCam;

    private float _startupDelay = 3f;

    // Use this for initialization
    void Start () {
        StartCoroutine(MoveCam());


	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private IEnumerator MoveCam()
    {
        yield return new WaitForSecondsRealtime(_startupDelay);
        if (VrCam == null) { VrCam = Camera.main; }

        // elliminate vertical centering
        Vector3 vrCamPosition = VrCam.transform.position;
        vrCamPosition.y = 0f;

        transform.Translate(-vrCamPosition);
    }
}
