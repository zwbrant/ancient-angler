using UnityEngine;
using System.Collections;

public class MirrorRbodyRotation : MonoBehaviour {
    public GameObject GO;

	// Use this for initialization
	void Start () {
        oldY = GO.transform.rotation.eulerAngles.y;
	}

    //   int frameCount = 0;
    //   float cumulativeX = 0f;
    //// Update is called once per frame
    //void Update () {

    //       print(GO.transform.rotation.eulerAngles.y);
    //       float x = 0f;
    //       if (frameCount == 4)
    //       {
    //           x = cumulativeX / 4;
    //           //int(x);
    //           transform.Rotate()

    //           cumulativeX = 0f;
    //           frameCount = 0;
    //       } else
    //       {
    //           cumulativeX += GO.transform.rotation.eulerAngles.y
    //           frameCount++;
    //       }

    //}

    float oldY = 0f;
    void FixedUpdate()
    {
        float currY = GO.transform.rotation.eulerAngles.y;
        float yChange = currY - oldY;
        transform.Rotate(new Vector3(0f, yChange, 0f));
        oldY = currY;
    }
}
