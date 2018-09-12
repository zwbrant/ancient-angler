using UnityEngine;
using System.Collections;

public class ZB_AutoAttachContoller : MonoBehaviour {
    public bool trueForRightController;

    GameObject controller;

    ConfigurableJoint joint;
    GameObject attachPt;

	// Use this for initialization
	void Start () {
        joint = GetComponent<ConfigurableJoint>();

        StartCoroutine(GetRigidBody());

	}

    IEnumerator GetRigidBody()
    {
        yield return new WaitForSeconds(.5f);
        yield return new WaitUntil(BothContollersReady);

        if (trueForRightController)
        {
            controller = GameObject.Find("Controller (right)");
        } else
        {
            controller = GameObject.Find("Controller (left)");

        }

        Transform controllerModel = controller.transform.GetChild(0);

        for (int i = 0; i < controllerModel.childCount; i++)
        {
            if (controllerModel.GetChild(i).name == "tip")
            {
                attachPt = controllerModel.GetChild(i).GetChild(0).gameObject;
                break;
            }
        }

        //transform.position = attachPt.transform.position;

        //Quaternion rot = Quaternion.Euler(attachPt.transform.rotation.x - 90, 0f, attachPt.transform.rotation.z);
        //this.transform.rotation = rot;

        
        controller.transform.position = this.transform.position + new Vector3(0f, 0f, .2f);
        controller.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        joint.connectedBody = attachPt.GetComponent<Rigidbody>();

    }

    public static bool BothContollersReady()
    {
        GameObject rightContoller = GameObject.Find("Controller (right)");
        GameObject leftContoller = GameObject.Find("Controller (left)");

        bool rightReady = (rightContoller != null && rightContoller.transform.childCount > 0 && rightContoller.transform.GetChild(0).childCount > 4);
        bool leftReady = (leftContoller != null && leftContoller.transform.childCount > 0 && leftContoller.transform.GetChild(0).childCount > 4);


        return (rightReady && leftReady);

    }

    // Update is called once per frame
    void Update () {
	
	}
}
