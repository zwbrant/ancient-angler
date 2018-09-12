using UnityEngine;
using System.Collections;
using Valve.VR;

public class PaddleForce : MonoBehaviour {
    public AudioSource audioSource;
    public static AudioUtility audioUtility;
    Rigidbody rbody;
    public float deadZone = 0;
    [Range(0, 10)]
    public float hapticMagnitude = 0.7f;
    public SteamVR_TrackedObject controllerR;
    SteamVR_Controller.Device deviceL;
    SteamVR_Controller.Device deviceR;

    public GameObject forcePointL;
    public GameObject forcePointR;

    public GameObject paddleL;
    public GameObject paddleR;

    CalculateSubmersion paddleRSubmersion;
    CalculateSubmersion paddleLSubmersion;
    bool rSubmerged;
    LineRenderer forceLine; 

    public float thrust;
    [Range (0, 30000)]
    public float paddleThrust;
    [Range (0, 150)]
    public float paddleRotationSens = 10;
    [Range (0, 500)]
    public float downforce;
    public bool restrictVelocity;
    [Range (0, 40)]
    public float optimalPaddleVelocity;
    [Range(0, 1000)]
    public float governedVelocity;

    // Use this for initialization
    void Start () {
        deviceR = SteamVR_Controller.Input(2);
        deviceL = SteamVR_Controller.Input(1);
        rbody = GetComponent<Rigidbody>();
        paddleRSubmersion = paddleR.GetComponent<CalculateSubmersion>();
        paddleLSubmersion = paddleL.GetComponent<CalculateSubmersion>();

        forceLine = GetComponent<LineRenderer>();
        audioUtility = GameObject.FindObjectOfType(typeof(AudioUtility)) as AudioUtility;
        lastRotation = transform.rotation.y;
    }

    bool playingWaterRush = false;
	// Update is called once per frame
	void Update () {
        paddleRSubmersion.transform.parent.Rotate(Input.GetAxis("Mouse ScrollWheel") * paddleRotationSens, 0f, 0f);



        //if (paddleRSubmersion.GetSubmergArea() > 0)
        //{
        //    if (rWaterRushLoopPlaying == false)
        //    {
        //        rWaterRushLoopPlaying = true;

        //        StartCoroutine("RWaterRushLoop");
        //    } else if (paddleRSubmersion.Velocity > optimalPaddleVelocity && playingWaterRush)
        //    {
        //        StopCoroutine("RWaterRushLoop");
        //        StartCoroutine("RWaterRushLoop");
        //    }
        //}

        //else if (rWaterRushLoopPlaying) 
        //{
        //    audioSource.Stop();
        //    StopCoroutine("RWaterRushLoop");
        //    rWaterRushLoopPlaying = false;
        //}
    }



    void FixedUpdate()
    {

        GetRotation();
        //rbody.AddForce(new Vector3(0f, -1f, 0f) * 100f);
        string statusMsg = "";

        if (Input.GetButton("Left"))
        {
            statusMsg += "Left thrust";

            rbody.AddForceAtPosition(transform.forward * thrust, forcePointL.transform.position);
        }
        if (Input.GetButton("Right"))
        {
            statusMsg += (statusMsg == "") ? "Right thrust" : " and Right thrust";

            rbody.AddForceAtPosition(transform.forward * thrust, forcePointR.transform.position);
        }

        ApplyPaddleForce(paddleLSubmersion, forcePointL.transform, deviceL);
        print(paddleLSubmersion.GetSubmergArea());
        ApplyPaddleForce(paddleRSubmersion, forcePointR.transform, deviceR);


        if (statusMsg != "")
            print(statusMsg);
    }

    public float rotationDiff;
    public float lastRotation;
    void GetRotation()
    {
        rotationDiff = transform.rotation.y - lastRotation;
        lastRotation = transform.rotation.y;
    }

    void ApplyPaddleForce(CalculateSubmersion paddleSubmersion, Transform forcePoint, SteamVR_Controller.Device steamVRdevice)
    {
        //try to avoid any unnecessary calculations
        float paddleVelocity = paddleSubmersion.Velocity;

        if (paddleVelocity > deadZone)
        {
            float paddleSubmergArea = paddleSubmersion.GetSubmergArea();
            if (paddleSubmergArea > 0)
            {
                Vector3 paddleDirection = paddleSubmersion.Direction;
                Vector3 currThrust = -paddleDirection * paddleSubmergArea * paddleThrust;
                float angleOfImpact = Vector3.Angle(paddleSubmersion.transform.up, paddleDirection);

                // normalize between the sides of the paddle
                if (angleOfImpact > 90)
                {
                    angleOfImpact = 180 - angleOfImpact;
                }
                // normalized. the greater the angle of impact (obliqueness), the less the intensity 
                float intensityOfImpact = 1f - (angleOfImpact * 0.0111111111111111f);
                currThrust *= (intensityOfImpact);

                Vector3 normalizedThrust = currThrust.normalized;
                if (restrictVelocity && paddleVelocity > optimalPaddleVelocity)
                    currThrust = normalizedThrust * governedVelocity;

                steamVRdevice.TriggerHapticPulse((ushort)(currThrust.magnitude * hapticMagnitude));

                rbody.AddForceAtPosition(currThrust * 2, forcePoint.position);

                // *Debugging*
                //print("Paddle thrust: " + currThrust);
                //print("Intensity of impact: " + intensityOfImpact);
                //print("Paddle angle: " + angleOfImpact);
                //print("Paddle direction: " + paddleRSubmersion.Direction);
                //print("Paddle velocity: " + paddleRSubmersion.Velocity);

                //Debug.DrawLine(paddleRSubmersion.transform.position, paddleRSubmersion.transform.right, Color.yellow);
                //Debug.DrawLine(paddleRSubmersion.transform.position, -paddleRSubmersion.Direction, Color.green);

                //
                //forceLine.SetPositions(new Vector3[] { paddleRSubmersion.transform.position + paddleRSubmersion.transform.up,
                //                                       paddleRSubmersion.transform.position,
                //                                        paddleRSubmersion.transform.position + paddleRSubmersion.Direction * 10 });
                //forceLine.SetVertexCount(2);
                //forceLine.SetPositions(new Vector3[] { forcePointR.transform.position, currThrust });

                //if (currThrust.magnitude <= 0)
                //{
                //    forceLine.SetPositions(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 0) });
                //}
                //else {

                //}
            }
        }

    }

    bool rWaterRushLoopPlaying = false;
    IEnumerator RWaterRushLoop()
    {
        do {
            if (paddleRSubmersion.Velocity <= optimalPaddleVelocity)
            {
                playingWaterRush = true;
                float randIndex = Random.Range(0f, audioUtility.waterRushes.Count - 1f);
                randIndex = (randIndex - (randIndex % 1));
                AudioClip currClip = audioUtility.waterRushes[(int)randIndex];
                audioSource.PlayOneShot(currClip, paddleRSubmersion.Velocity / optimalPaddleVelocity);
                yield return new WaitForSeconds(currClip.length);
            } else
            {
                playingWaterRush = false;
                float randIndex = Random.Range(0f, audioUtility.waterSplashes.Count - 1f);
                randIndex = (randIndex - (randIndex % 1));
                AudioClip currClip = audioUtility.waterSplashes[(int)randIndex];
                audioSource.PlayOneShot(currClip, 1f);
                yield return new WaitForSeconds(currClip.length);
            }

        } while (rWaterRushLoopPlaying);


    }
}
