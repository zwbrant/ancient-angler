using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZB_MouseSlide : MonoBehaviour {
    public Axis XAxis = Axis.None;
    public bool FlipXDirection = false;
    [Range(0f, 100f)]
    public float XSpeed = 1f;
    public Axis YAxis = Axis.None;
    public bool FlipYDirection = false;
    [Range(0f, 100f)]
    public float YSpeed = 1f;

    public enum Axis { None, X, Y, Z };

	void Start () {
		
	}
	
	void Update () {
        GetMouseX();
        GetMouseY();
	}

    void GetMouseX()
    {
        float xInput = Input.GetAxis("Mouse X") * Time.deltaTime * XSpeed;

        switch (XAxis)
        {
            case Axis.X:
                transform.Translate(new Vector3(xInput, 0f, 0f), Space.World);
                break;
            case Axis.Y:
                transform.Translate(new Vector3(0f, xInput, 0f), Space.World);
                break;
            case Axis.Z:
                transform.Translate(new Vector3(0f, 0f, xInput), Space.World);
                break;
            case Axis.None:
                break;
        }

    }

    void GetMouseY()
    {
        float yInput = Input.GetAxis("Mouse Y") * Time.deltaTime * YSpeed;

        switch (YAxis)
        {
            case Axis.X:
                transform.Translate(new Vector3(yInput, 0f, 0f));
                break;
            case Axis.Y:
                transform.Translate(new Vector3(0f, yInput, 0f));
                break;
            case Axis.Z:
                transform.Translate(new Vector3(0f, 0f, yInput));
                break;
            case Axis.None:
                break;
        }
    }
}
