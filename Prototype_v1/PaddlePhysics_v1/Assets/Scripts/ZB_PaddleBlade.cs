using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using VRTK;

/* Author: Zane Brant
    
    Provides information about the paddle it's attached to (represented by four points). 
    Should be used by a PaddleForceManager to calculate forces from Paddles and distribute them. 

*/

public class ZB_PaddleBlade : MonoBehaviour {
    const float WaterHeight = 0f;
    // how many frames of data should be averaged for the velocity vector (if any)?
    const int VelocityFrameBuffer = 4;

    public GameObject[] corners = new GameObject[4];
    public GameObject bladeGO;
    public GameObject forcePoint;
    public LineRenderer submersionLineRenderer;
    public bool showSubmersionLines = false;

    private Vector3[] cornerVectors = new Vector3[4];
    private Vector3 velocityVector = new Vector3(0, 0, 0);

    internal VRTK_ControllerActions mainControlActions = null;
    internal VRTK_ControllerActions secondaryControlActions = null;
    internal ZB_PaddleForceManager associatedBoat = null;


    #region Properties

    public Vector3 Direction { get { return velocityVector.normalized; } }
    public float Velocity { get { return velocityVector.magnitude; } }
    public float SubmergedArea { get
        {
            int cornersSubmerged = CornerVectors.Count(v => v.y < WaterHeight);

            switch (cornersSubmerged)
            {
                case 1:
                    //print(SubmergedAreaTest());
                    //return GetAreaOneSubmergCorner_fast();
                    return GetAreaOneSubmergCorner();
                case 2:
                    //return GetAreaTwoSubmergCorner_fast();
                    return GetAreaTwoSubmergCorner();
                case 3:
                    return GetAreaThreeSubmergCorner();
                case 4:
                    return GetAreaFourSubmergCorners();
                default:
                    return 0f;
            }

        }
    }

    public Vector3[] CornerVectors
    {
        get
        {
            cornerVectors[0] = corners[0].transform.position;
            cornerVectors[1] = corners[1].transform.position;
            cornerVectors[2] = corners[2].transform.position;
            cornerVectors[3] = corners[3].transform.position;
            return cornerVectors;
        }
    }

    public Vector3[] CornersByDepth
    {
        get
        {
            return CornerVectors.OrderBy(v => v.y).ToArray<Vector3>();
        }
    }

    public Vector3 RawForce
    {
        get
        {
            float thrust = Velocity * SubmergedArea;
            float angleOfImpact = Vector3.Angle(transform.up, Direction);

            // normalize between the sides of the paddle
            if (angleOfImpact > 90)
            {
                angleOfImpact = 180 - angleOfImpact;
            }
            // normalize the angle. the greater the angle of impact (obliqueness), the less the intensity 
            float intensityOfImpact = 1f - (angleOfImpact * 0.0111111111111111f);
            thrust *= intensityOfImpact;

            return -Direction * thrust;
        }
    }
    #endregion

    void Start()
    {
        if (showSubmersionLines && submersionLineRenderer == null) {
            try
            {
                submersionLineRenderer = GetComponent<LineRenderer>();
            } catch
            {
                print("ShowSubmersionLines is enabled, but no LineRenderer can be found. Disabling.");
                showSubmersionLines = false;
            }
        }

        previousPos = transform.position;
    }

    Vector3 previousPos;
    void Update()
    {
        SetVelocityVector();
        previousPos = transform.position;
    }

    void FixedUpdate()
    {
        //direction = (transform.position - previousPos).normalized;
    }

    int frameCount = 0;
    Vector3 cumulativeVelocity = new Vector3();
    void SetVelocityVector()
    {
        if (frameCount < VelocityFrameBuffer)
        {
            cumulativeVelocity += (transform.position - previousPos) / Time.deltaTime;
            frameCount++;
        }
        else
        {
            velocityVector = cumulativeVelocity / VelocityFrameBuffer;
            frameCount = 0;
            cumulativeVelocity = new Vector3();
        }
    }

    public float GetAreaOneSubmergCorner_fast()
    {
        Vector3 D = CornersByDepth[0];
        Vector3 C = CornersByDepth[1];
        Vector3 A = CornersByDepth[2];

        float area = .5f * Vector3.Magnitude(CrossProduct((CrossProduct(D, A) * (D.y / (D.y - A.y))), (CrossProduct(D, C) * (D.y / (D.y - C.y)))));

        return area;
        
    }

    public float GetAreaTwoSubmergCorner_fast()
    {
        Vector3 D = CornersByDepth[0];
        Vector3 C = CornersByDepth[1];
        Vector3 A = CornersByDepth[2];

        //float triangleOne = Vector3.Magnitude(CrossProduct((((D.y - C.y) / (D.y - A.y)) * CrossProduct(D, A)), CrossProduct(D, C)));
        //float triangleTwo = Vector3.Magnitude(CrossProduct((C.y / (D.y - A.y)) * CrossProduct(D, A), CrossProduct(D, C)));

        float triangleOne = GetAreaOneSubmergCorner_fast();
        float triangleTwo = .5f * Vector3.Magnitude(CrossProduct((CrossProduct(D, A) * (D.y / (D.y - A.y))), (CrossProduct(D, C) * (D.y / (D.y - C.y)))));


        return (triangleOne + triangleTwo) * .5f;

    }

    public static Vector3 CrossProduct(Vector3 a, Vector3 b)
    {
        Vector3 c = new Vector3(0, 0, 0);
        c.x = (a.y * b.z) - (a.z - b.y);
        c.y = (a.z * b.x) - (a.x - b.z);
        c.z = (a.x * b.y) - (a.y * b.x);

        return c;
    }

    public float GetAreaOneSubmergCorner()
    {
        Vector3[] currCornersByDepth = CornersByDepth;

        RaycastHit shortWaterHit;
        RaycastHit tallWaterHit;
        bool shortHit = Physics.Linecast(currCornersByDepth[1], currCornersByDepth[0], out shortWaterHit);
        bool tallHit = Physics.Linecast(currCornersByDepth[2], currCornersByDepth[0], out tallWaterHit);

        Vector3 shortWaterVect = shortWaterHit.point;
        Vector3 tallWaterVect = tallWaterHit.point;

        float submergBase = Math.Abs(Vector3.Distance(currCornersByDepth[0], tallWaterVect));
        float submergHeight = Math.Abs(Vector3.Distance(currCornersByDepth[0], shortWaterVect));
        float submergArea = (submergBase * submergHeight) / 2f;

        //Debug.DrawLine(lowestCorner, shortWaterVect, Color.red);
        //Debug.DrawLine(lowestCorner, tallWaterVect, Color.red);

        if (showSubmersionLines)
        {
            submersionLineRenderer.SetVertexCount(4);
            submersionLineRenderer.SetPositions(new Vector3[] { currCornersByDepth[0], shortWaterVect, tallWaterVect, currCornersByDepth[0] });
        }

        return submergArea;
    }

    public float GetAreaTwoSubmergCorner()
    {
        Vector3[] currCorners = CornersByDepth;

        RaycastHit shortWaterHit;
        RaycastHit tallWaterHit;
        bool shortHit = Physics.Linecast(currCorners[2], currCorners[0], out shortWaterHit);
        bool tallHit = Physics.Linecast(currCorners[3], currCorners[1], out tallWaterHit);

        Vector3 shortWaterVect = shortWaterHit.point;
        Vector3 tallWaterVect = tallWaterHit.point;

        float rightTriHeight = Vector3.Distance(currCorners[1], tallWaterVect);
        float rightTriBase = Vector3.Distance(currCorners[1], currCorners[0]);
        float rightTriArea = (rightTriBase * rightTriHeight) / 2f;

        //Debug.DrawLine(CornersByDepth[1], tallWaterVect, Color.green, 1f, true);
        //Debug.DrawLine(CornersByDepth[1], CornersByDepth[0], Color.green, 1f, true);


        float acuteTriBase = Vector3.Distance(shortWaterVect, tallWaterVect);

        //TODO fix
        float acuteTriHeight = Math.Abs(0 - currCorners[0].y);
        float acuteTriArea = (acuteTriBase * acuteTriHeight) / 2f;

        if (showSubmersionLines)
        {
            submersionLineRenderer.SetVertexCount(6);
            submersionLineRenderer.SetPositions(new Vector3[] { currCorners[0], CornersByDepth[1], tallWaterVect, currCorners[0], shortWaterVect, tallWaterVect });
        }

        //Debug.DrawLine(shortWaterVect, tallWaterVect, Color.blue, 1f, true);
        //Debug.DrawLine(CornersByDepth[1], CornersByDepth[0], Color.blue, 1f, true);

        return rightTriArea + acuteTriArea;
    }

    public float GetAreaThreeSubmergCorner()
    {
        Vector3[] currCorners = CornersByDepth;

        RaycastHit deepWaterHit;
        RaycastHit shallowWaterHit;
        Physics.Linecast(currCorners[3], currCorners[2], out shallowWaterHit);
        Physics.Linecast(currCorners[3], currCorners[1], out deepWaterHit);
        Vector3 deepWaterVect = deepWaterHit.point;
        Vector3 shallowWaterVect = shallowWaterHit.point;

        float shallowRightTriHeight = Vector3.Distance(currCorners[2], shallowWaterVect);
        float shallowRightTriBase = Vector3.Distance(currCorners[0], currCorners[2]);
        float shallowRightTriArea = (shallowRightTriHeight * shallowRightTriBase) / 2f;

        float deepRightTriHeight = Vector3.Distance(currCorners[1], deepWaterVect);
        float deepRightTriBase = Vector3.Distance(currCorners[0], CornersByDepth[1]);
        float deepRightTriArea = (deepRightTriHeight * deepRightTriBase) / 2f;

        float[] acuteTriSides = new float[] {
            Vector3.Distance(deepWaterVect, shallowWaterVect),
            Vector3.Distance(currCorners[0], deepWaterVect),
            Vector3.Distance(currCorners[0], shallowWaterVect)};

        if (showSubmersionLines)
        {
            submersionLineRenderer.SetVertexCount(9);
            submersionLineRenderer.SetPositions(new Vector3[] { currCorners[0], currCorners[2], shallowWaterVect, currCorners[0], currCorners[1], deepWaterVect,
                                                    currCorners[0], shallowWaterVect, deepWaterVect});
        }

        float halfPerim = acuteTriSides.Sum() / 2f;

        float acuteTriArea = (float)Math.Sqrt(halfPerim * (halfPerim - acuteTriSides[0]) * (halfPerim - acuteTriSides[1]) * (halfPerim - acuteTriSides[2]));

        return acuteTriArea + deepRightTriArea + shallowRightTriArea;
    }

    // cache fully submerged paddle area, since it will never change
    private float paddleArea = -1f;
    public float GetAreaFourSubmergCorners()
    {
        if (paddleArea == -1f)
            paddleArea = Vector3.Distance(CornerVectors[0], CornerVectors[1]) * Vector3.Distance(CornerVectors[0], CornerVectors[2]);

        return paddleArea;
    }

}
