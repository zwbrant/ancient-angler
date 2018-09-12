using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;

public class FPSCamControl : MonoBehaviour {
    MouseLook mouseLook = new MouseLook();
    [SerializeField][Range(0, 5)]
    private float sensitivity = 1f;

	// Use this for initialization
	void Start () {
        mouseLook.Init(transform, transform);
        rotation = transform.localRotation;
	}

    float xRotation = 0f;
    float yRotation = 0f;
    Quaternion rotation;
	// Update is called once per frame
	void Update () {
        //mouseLook.LookRotation(transform, transform);
        xRotation = Input.GetAxisRaw("Mouse X") * sensitivity;
        yRotation = -Input.GetAxisRaw("Mouse Y") * sensitivity;

        Quaternion currRotation = transform.rotation;
        Quaternion newRotation = Quaternion.Euler(yRotation, xRotation, 0f);
        transform.rotation = Quaternion.Euler(currRotation.eulerAngles + newRotation.eulerAngles);

        //print(newRotation.eulerAngles);

        //transform.localRotation = Quaternion.LookRotation(transform.localRotation.eulerAngles + new Vector3(xRotation, yRotation, 0f));
        //transform.Rotate(new Vector3(xRotation, yRotation, 0f));

        //transform.eulerAngles = new Vector3(xRotation, yRotation, 0f);

        //print(string.Format("Rotating {0} vertically and {1} horizontally", vertRotation, horzRotation));
    }
}
